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
    public static class ShapeCommandHandler
    {
        public static object DrawCircle(JObject parameters)
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Extract parameters
            var center = parameters["center"].ToObject<double[]>();
            var radius = parameters["radius"].Value<double>();
            var centerPoint = new Point3d(center[0], center[1], center.Length > 2 ? center[2] : 0);

            // Lock the document
            using (DocumentLock docLock = doc.LockDocument())
            {
                // Start a transaction
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Get the current space (model space or paper space)
                        BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);

                        // Open the Block table record Model space for write
                        BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        
                        // Create a circle
                        using (Circle circle = new Circle(centerPoint, Vector3d.ZAxis, radius))
                        {
                            // Add the circle to the drawing
                            btr.AppendEntity(circle);
                            trans.AddNewlyCreatedDBObject(circle, true);
                        }

                        // Commit the transaction
                        trans.Commit();

                        return new
                        {
                            success = true,
                            message = $"Circle created successfully!",
                        };
                    }
                    catch (System.Exception ex)
                    {
                        trans.Abort();

                        return new
                        {
                            success = false,
                            error = $"Failed to create circle: {ex.Message}",
                            stackTrace = ex.StackTrace
                        };
                    }
                }
            }
        }
    }
}