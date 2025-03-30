using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using AutoCADMCP.Commands;

namespace AutoCADMCP
{
    public class Command
    {
        public string Type { get; set; }
        public JObject Parameters { get; set; }
    }

    public static class Log
    {
        public static void Info(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] INFO: {message}");
        }

        public static void Warning(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] WARNING: {message}");
        }

        public static void Error(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] ERROR: {message}");
        }
    }

    public class AutoCADMCPBridge
    {
        private TcpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;
        private const int Port = 6400;
        private bool _isRunning;
        private static readonly object lockObj = new object();
        private static readonly Dictionary<string, (string commandJson, TaskCompletionSource<string> tcs)> commandQueue = 
            new Dictionary<string, (string commandJson, TaskCompletionSource<string> tcs)>();

        // The IExtensionApplication interface is optional but useful for initialization
        [CommandMethod("STARTMCP")]
        public void StartMCPBridge()
        {
            if (_isRunning)
            {
                Log.Error("MCP Bridge is already running!");
                return;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                StartTcpListener();
                Application.Idle += ProcessCommands;
                _isRunning = true;
                Log.Info($"MCP Bridge started on port {Port}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error starting MCP Bridge: {ex.Message}");
            }
        }

        [CommandMethod("STOPMCP")]
        public void StopMCPBridge()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                StopListener();
                Application.Idle -= ProcessCommands;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error stopping MCP Bridge: {ex.Message}");
            }
        }

        private async void StartTcpListener()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, Port);
                _listener.Start();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        _ = HandleClientAsync(client); // Fire and forget, but we'll handle errors
                    }
                    catch (System.Exception ex) when (!(ex is OperationCanceledException))
                    {
                        // Log error but continue listening
                        Log.Error($"Error accepting client: {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"TCP Listener error: {ex.Message}");
            }
            finally
            {
                StopListener();
            }
        }

        private void StopListener()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _listener?.Stop();
                _isRunning = false;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error stopping listener: {ex.Message}");
            }
            finally
            {
                _listener = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buffer = new byte[8192];
                while (_isRunning)
                {
                    try
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // Client disconnected

                        string commandText = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string commandId = Guid.NewGuid().ToString();
                        var tcs = new TaskCompletionSource<string>();

                        // Special handling for ping command to avoid JSON parsing
                        if (commandText.Trim() == "ping")
                        {
                            // Direct response to ping without going through JSON parsing
                            byte[] pingResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"status\":\"success\",\"result\":{\"message\":\"pong\"}}");
                            await stream.WriteAsync(pingResponseBytes, 0, pingResponseBytes.Length);
                            continue;
                        }

                        lock (lockObj)
                        {
                            commandQueue[commandId] = (commandText, tcs);
                        }

                        string response = await tcs.Task;
                        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"Client handler error: {ex.Message}");
                        break;
                    }
                }
            }
        }

        private static void ProcessCommands(object sender, EventArgs e)
        {
            List<string> processedIds = new();
            lock (lockObj)
            {
                foreach (var kvp in commandQueue.ToList())
                {
                    string id = kvp.Key;
                    string commandText = kvp.Value.commandJson;
                    var tcs = kvp.Value.tcs;

                    try
                    {
                        // Special case handling
                        if (string.IsNullOrEmpty(commandText))
                        {
                            var emptyResponse = new
                            {
                                status = "error",
                                error = "Empty command received"
                            };
                            tcs.SetResult(JsonConvert.SerializeObject(emptyResponse));
                            processedIds.Add(id);
                            continue;
                        }

                        // Trim the command text to remove any whitespace
                        commandText = commandText.Trim();

                        // Non-JSON direct commands handling (like ping)
                        if (commandText == "ping")
                        {
                            var pingResponse = new
                            {
                                status = "success",
                                result = new { message = "pong" }
                            };
                            tcs.SetResult(JsonConvert.SerializeObject(pingResponse));
                            processedIds.Add(id);
                            continue;
                        }

                        // Check if the command is valid JSON before attempting to deserialize
                        if (!IsValidJson(commandText))
                        {
                            var invalidJsonResponse = new
                            {
                                status = "error",
                                error = "Invalid JSON format",
                                receivedText = commandText.Length > 50 ? commandText.Substring(0, 50) + "..." : commandText
                            };
                            tcs.SetResult(JsonConvert.SerializeObject(invalidJsonResponse));
                            processedIds.Add(id);
                            continue;
                        }

                        // Normal JSON command processing
                        var command = JsonConvert.DeserializeObject<Command>(commandText);
                        if (command == null)
                        {
                            var nullCommandResponse = new
                            {
                                status = "error",
                                error = "Command deserialized to null",
                                details = "The command was valid JSON but could not be deserialized to a Command object"
                            };
                            tcs.SetResult(JsonConvert.SerializeObject(nullCommandResponse));
                        }
                        else
                        {
                            string responseJson = ExecuteCommand(command);
                            tcs.SetResult(responseJson);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"Error processing command: {ex.Message}\n{ex.StackTrace}");

                        var response = new
                        {
                            status = "error",
                            error = ex.Message,
                            commandType = "Unknown (error during processing)",
                            receivedText = commandText?.Length > 50 ? commandText.Substring(0, 50) + "..." : commandText
                        };
                        string responseJson = JsonConvert.SerializeObject(response);
                        tcs.SetResult(responseJson);
                    }

                    processedIds.Add(id);
                }

                foreach (var id in processedIds)
                {
                    commandQueue.Remove(id);
                }
            }
        }

        private static string ExecuteCommand(Command command)
        {
            try
            {
                Log.Info($"Executing command: {command.Type} with parameters: {command.Parameters}");

                if (string.IsNullOrEmpty(command.Type))
                {
                    var errorResponse = new
                    {
                        status = "error",
                        error = "Command type cannot be empty",
                        details = "A valid command type is required for processing"
                    };
                    return JsonConvert.SerializeObject(errorResponse);
                }

                // Handle ping command for connection verification
                if (command.Type == "ping")
                {
                    var pingResponse = new { status = "success", result = new { message = "pong" } };
                    return JsonConvert.SerializeObject(pingResponse);
                }

                object result = command.Type switch
                {
                    "DRAW_CIRCLE" => ShapeCommandHandler.DrawCircle(command.Parameters),
                    "GET_CURRENT_WORKSPACE" => WorkspaceCommandHandler.GetCurrentWorkspace(),
                    "SET_CURRENT_WORKSPACE" => WorkspaceCommandHandler.SetCurrentWorkspace(command.Parameters["workspace"].ToString()),
                    _ => throw new System.Exception($"Unknown command type: {command.Type}")
                };

                var response = new { status = "success", result };
                return JsonConvert.SerializeObject(response);
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error executing command {command.Type}: {ex.Message}\n{ex.StackTrace}");
                var response = new
                {
                    status = "error",
                    error = ex.Message,
                    command = command.Type,
                    stackTrace = ex.StackTrace,
                    paramsSummary = command.Parameters != null ? GetParamsSummary(command.Parameters) : "No parameters"
                };
                return JsonConvert.SerializeObject(response);
            }
        }

        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;
            try
            {
                JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static string GetParamsSummary(JObject @params)
        {
            try
            {
                if (@params == null || !@params.HasValues)
                    return "No parameters";

                return string.Join(", ", @params.Properties().Select(p => $"{p.Name}: {p.Value?.ToString()?.Substring(0, Math.Min(20, p.Value?.ToString()?.Length ?? 0))}"));
            }
            catch
            {
                return "Could not summarize parameters";
            }
        }
    }
} 