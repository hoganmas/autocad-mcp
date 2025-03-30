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
        public static object Run(JObject parameters,
            Func<Document, JObject, object> func,
            Func<bool, string> messageGenerator = null)
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Lock the document
            using (DocumentLock docLock = doc.LockDocument())
            {
                try
                {
                    var result = func(doc, parameters);

                    return new
                    {
                        success = true,
                        message = messageGenerator?.Invoke(true) ?? "Operation completed successfully!",
                        result = result
                    };
                }
                catch (System.Exception ex)
                {
                    return new
                    {
                        success = false,
                        error =  $"{messageGenerator?.Invoke(false) ?? "Operation failed!"}: {ex.Message}",
                        stackTrace = ex.StackTrace
                    };
                }
            }
        }

        public static object Modify(JObject parameters,
            Func<BlockTableRecord, Transaction, JObject, object> modifier,
            Func<bool, string> messageGenerator = null)
        {
            return Run(parameters,
                (doc, parameters) => {
                    // Start a transaction
                    using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            // Get the current space (model space or paper space)
                            BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                            // Open the Block table record Model space for write
                            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                            var result = modifier(btr, trans, parameters);

                            // Commit the transaction
                            trans.Commit();
                            doc.Editor.Regen();

                            return result;
                        }
                        catch (System.Exception ex)
                        {
                            trans.Abort();
                            throw ex;
                        }
                    }
                }, 
                messageGenerator
            );
        }

        public static object Access(JObject parameters,
            Func<BlockTableRecord, Transaction, JObject, object> accessor,
            Func<bool, string> messageGenerator = null)
        {
            return Run(parameters,
                (doc, parameters) => {
                    // Start a transaction
                    using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            // Get the current space (model space or paper space)
                            BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                            // Open the Block table record Model space for read
                            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                            var result = accessor(btr, trans, parameters);

                            // Commit the transaction
                            trans.Commit();

                            return result;
                        }
                        catch (System.Exception ex)
                        {
                            trans.Abort();
                            throw ex;
                        }
                    }
                }, 
                messageGenerator
            );
        }
    }
}