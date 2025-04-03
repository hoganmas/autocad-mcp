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

        public static object ModifyEntity(JObject parameters,
            Func<Entity, BlockTableRecord, Transaction, JObject, object> modifier,
            Func<bool, string> messageGenerator = null)
        {
            return Run(parameters,
                (doc, parameters) => {
                    long entityId = parameters["entityId"].Value<long>();

                    Handle handle = new Handle(entityId);
                    if (doc.Database.TryGetObjectId(handle, out ObjectId objId))
                    {
                        // Start a transaction
                        using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                        {
                            try 
                            {
                                // Get the current space (model space or paper space)
                                BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                                // Open the Block table record Model space for write
                                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                                Entity ent = trans.GetObject(objId, OpenMode.ForWrite) as Entity;

                                var result = modifier(ent, btr, trans, parameters);

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
                    }
                    else
                    {
                        throw new System.Exception("Entity not found");
                    }
                }, 
                messageGenerator
            );
        }

        public static object ModifyEntities(JObject parameters,
            Func<Entity, BlockTableRecord, Transaction, JObject, object> modifier,
            Func<bool, string> messageGenerator = null)
        {
            return Run(parameters,
                (doc, parameters) => {
                    Log.Info("cp1");
                    var entityIds = parameters["entityIds"].ToObject<List<long>>();

                    Log.Info("cp2");

                    var entityParameters = parameters.ContainsKey("entityParameters") ?
                        parameters["entityParameters"].ToObject<List<JObject>>() : null;

                    Log.Info("cp3");

                    // Start a transaction
                    using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                    {
                        try 
                        {
                            Log.Info("cp4");

                            // Get the current space (model space or paper space)
                            BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                            // Open the Block table record Model space for write
                            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                            Log.Info("cp5");

                            var results = new List<object>();
                            for (int i = 0; i < entityIds.Count; i++)
                            {
                                Log.Info("cp6");
                                if (doc.Database.TryGetObjectId(new Handle(entityIds[i]), out ObjectId objId))
                                {
                                    Log.Info("cp7");
                                    
                                    Entity ent = trans.GetObject(objId, OpenMode.ForWrite) as Entity;

                                    Log.Info("cp8");
                                    var result = modifier(ent, btr, trans, entityParameters != null ? entityParameters[i] : null);
                                    results.Add(result);
                                }
                                else
                                {
                                    throw new System.Exception("Entity not found");
                                }
                            }

                            // Commit the transaction
                            trans.Commit();
                            doc.Editor.Regen();

                            return results;
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