using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace AutoCADMCP.Commands
{
    public static class ViewCommands
    {
        private static ViewTableRecord originalView;

        [MCPCommand("CAPTURE_VIEW")]
        public static object CaptureView(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    Editor ed = doc.Editor;
                    Database db = doc.Database;
    
                    var target = parameters["target"].ToObject<double[]>();
                    var viewHeight = parameters["viewHeight"].ToObject<double>();
                    var viewDirection = parameters["viewDirection"].ToObject<double[]>();
                    var perspectiveEnabled = parameters["perspectiveEnabled"].ToObject<bool>();
                    var lensLength = perspectiveEnabled ? parameters["lensLength"].ToObject<double>() : 0;

                    // var viewDirection = (OrthographicView)Enum.Parse(typeof(OrthographicView), parameters["viewDirection"].Value<string>(), true);
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            originalView = ed.GetCurrentView();

                            // Create a new view definition
                            ViewTableRecord view = new ViewTableRecord();

                            // Resize the view to the new height
                            var viewWidth = originalView.Width * viewHeight / originalView.Height;

                            // Set the view direction
                            var forwardVector = (new Vector3d(viewDirection[0], viewDirection[1], viewDirection[2])).GetNormal();
                            // var rightVector = (new Vector3d(-viewDirection[1], viewDirection[0], 0)).GetNormal();
                            //var upVector = forwardVector.CrossProduct(rightVector).GetNormal();

                            // Set view direction and target
                            view.ViewDirection = forwardVector;
                            view.Target = new Point3d(target[0], target[1], target[2]);
                            view.CenterPoint = Point2d.Origin;

                            view.Height = viewHeight;
                            view.Width = viewWidth;

                            // view.PerspectiveEnabled = perspectiveEnabled;

                            if (perspectiveEnabled)
                            {
                                // view.LensLength = lensLength;
                            }

                            // Apply it to the editor (model space)
                            ed.SetCurrentView(view);

                            // Commit the transaction
                            trans.Commit();
                        }
                        catch (System.Exception e)
                        {
                            trans.Abort();
                            throw e;
                        }
                    }

                    string imagePath = @"C:\Temp\render_output.png";
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                    System.IO.File.Delete(imagePath);

                    string renderScript = $"-RENDER\nLow\n\n\n\nYes\n{imagePath}\nRENDERWINDOWCLOSE\n";
                    doc.SendStringToExecute(renderScript, true, false, false);

                    doc.CommandEnded += OnCommandEnded;

                    Log.Info($"Rendering complete. Image saved to: {imagePath}");
                    return imagePath;
                },
                (isSuccess) => isSuccess ? "View captured successfully!" : "Failed to capture view!"
            );
        }

        private static void OnCommandEnded(object sender, CommandEventArgs e)
        {
            Log.Info("Command ended: " + e.GlobalCommandName);
            // Restore the original view
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            doc.CommandEnded -= OnCommandEnded;

            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    doc.Editor.SetCurrentView(originalView);
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    trans.Abort();
                    Log.Error("Failed to restore view: " + ex.Message);
                }
            }
        }
    }
}