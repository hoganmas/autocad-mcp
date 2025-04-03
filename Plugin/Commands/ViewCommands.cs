using System;
using System.Collections.Generic;
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
        [MCPCommand("CAPTURE_VIEW")]
        public static object CaptureView(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    Editor ed = doc.Editor;
                    Database db = doc.Database;
    
                    var cameraLocation = parameters["cameraLocation"].Value<double[]>();
                    var target = parameters["target"].Value<double[]>();

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        /*
                        try
                        {
                            ViewTableRecord view = ed.GetCurrentView();

                            view.SetView(
                                new Point3d(cameraLocation[0], cameraLocation[1], cameraLocation.Length > 2 ? cameraLocation[2] : 0),
                                new Point3d(target[0], target[1], target.Length > 2 ? target[2] : 0)
                            );

                            // Apply the view to the active viewport
                            doc.Editor.SetCurrentView(view);

                            // Commit the transaction
                            tr.Commit();
                        }
                        catch (Exception e)
                        {
                            tr.Abort();
                            throw e;
                        }
                        */

                        // Capture the view
                        string imagePath = @"C:\Temp\render_output.png";

                        string renderCommand = $"-RENDER\n";
                        string saveCommand = $"-SAVEIMG\n{imagePath}\n";

                        // Run the render command
                        ed.Command(renderCommand);
                        ed.Command(saveCommand);

                        Log.Info($"Rendering complete. Image saved to: {imagePath}");
                        return imagePath;
                    }
                },
                (isSuccess) => isSuccess ? "View captured successfully!" : "Failed to capture view!"
            );
        }
    }
}