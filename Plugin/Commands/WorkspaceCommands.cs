using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADMCP.Commands
{
    public static class WorkspaceCommandHandler
    {
        public static object GetCurrentWorkspace()
        {
            try
            {
                // Get the current document
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    throw new System.Exception("No active document");
                }

                // Get the current workspace name
                string currentWorkspace = Application.GetSystemVariable("WSCURRENT") as string;
                
                return new
                {
                    success = true,
                    workspace = currentWorkspace ?? "Unknown"
                };
            }
            catch (System.Exception ex)
            {
                return new
                {
                    success = false,
                    error = $"Failed to get workspace: {ex.Message}",
                    workspace = "Unknown"
                };
            }
        }

        public static object SetCurrentWorkspace(string workspace)
        {
            try
            {
                // Get the current document
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    throw new System.Exception("No active document");
                }

                // Set the workspace using WSCURRENT system variable
                Application.SetSystemVariable("WSCURRENT", workspace);

                return new
                {
                    success = true,
                    workspace = workspace
                };
            }
            catch (System.Exception ex)
            {
                return new
                {
                    success = false,
                    error = $"Failed to set workspace: {ex.Message}",
                    workspace = "Unknown"
                };
            }
        }
    }
} 