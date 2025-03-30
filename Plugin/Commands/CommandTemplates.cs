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
    public static class CommandTemplates
    {        
        public static object Modify(JObject parameters,
            Action<BlockTableRecord, Transaction, JObject> modifier,
            Func<bool, string> messageGenerator = null)
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

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

                        modifier(btr, trans, parameters);

                        // Commit the transaction
                        trans.Commit();
                        ed.Regen();

                        return new
                        {
                            success = true,
                            message = messageGenerator?.Invoke(true) ?? "Operation completed successfully!",
                        };
                    }
                    catch (System.Exception ex)
                    {
                        trans.Abort();

                        return new
                        {
                            success = false,
                            error =  $"{messageGenerator?.Invoke(false) ?? "Operation failed!"}: {ex.Message}",
                            stackTrace = ex.StackTrace
                        };
                    }
                }
            }
        }
    }
}