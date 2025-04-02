using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutoCADMCP.Commands
{
    public static class EntityCommands
    {
        private static List<PropertyInfo> GetEntityPropertyInfo(Entity ent)
        {
            var type = ent.GetType();
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList().Where(
                p => p.CanRead && p.CanWrite && p.DeclaringType == type
            ).ToList();
        }


        [MCPCommand("GET_ALL_ENTITIES")]
        public static object GetAllEntities(JObject parameters)
        {
            return CommandTemplates.Access(parameters,
                (btr, trans, parameters) => {
                    List<long> entityIds = new List<long>();

                    foreach (ObjectId objId in btr)
                    {
                        entityIds.Add(objId.Handle.Value);
                    }

                    return entityIds;
                },
                (isSuccess) => isSuccess ? "Entities retrieved successfully!" : "Failed to retrieve entities!"
            );
        }

        [MCPCommand("GET_SELECTED_ENTITIES")]
        public static object GetSelectedEntities(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    List<long> entityIds = new List<long>();
                    var selection = doc.Editor.SelectImplied();
                    if (selection.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in selection.Value.GetObjectIds())
                        {
                            entityIds.Add(objId.Handle.Value);
                        }
                    }

                    return entityIds;
                },
                (isSuccess) => isSuccess ? "Entities retrieved successfully!" : "Failed to retrieve entities!"
            );
        }

        [MCPCommand("GET_ENTITY_PROPERTIES")]
        public static object GetEntityProperties(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    List<long> entityIds = parameters["entityIds"].ToObject<List<long>>();

                    List<object> entityInfos = new List<object>();

                    foreach (long entityId in entityIds)
                    {
                        Handle handle = new Handle(entityId);
                        if (doc.Database.TryGetObjectId(handle, out ObjectId objId))
                        {
                            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                            {
                                try 
                                {
                                    Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                                    if (ent != null)
                                    {
                                        var propertyInfo = GetEntityPropertyInfo(ent);
                                        var properties = propertyInfo.ToDictionary(p => p.Name, p => p.GetValue(ent));

                                        entityInfos.Add(new {
                                            handle = handle,
                                            type = ent.GetType().Name,
                                            properties = properties
                                        });                                    
                                    }
                                    trans.Commit();
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
                            throw new System.Exception($"Invalid handle! Handle {handle} does not correspond to any object in the database.");
                        }
                    }

                    return entityInfos;
                },
                (isSuccess) => isSuccess ? "Entity info retrieved successfully!" : "Failed to retrieve entity info!"
            );
        }

        [MCPCommand("SET_ENTITY_PROPERTIES")]
        public static object SetEntityProperties(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    List<long> entityIds = parameters["entityIds"].ToObject<List<long>>();

                    List<object> entityInfos = new List<object>();

                    foreach (long entityId in entityIds)
                    {
                        Handle handle = new Handle(entityId);
                        if (doc.Database.TryGetObjectId(handle, out ObjectId objId))
                        {
                            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
                            {
                                try 
                                {
                                    Entity ent = trans.GetObject(objId, OpenMode.ForWrite) as Entity;
                                    if (ent != null)
                                    {
                                        // Set the properties of the entity
                                        var inputProperties = parameters["properties"].ToObject<Dictionary<string, object>>();
                                        var outputProperties = new Dictionary<string, object>();

                                        var propertyInfo = GetEntityPropertyInfo(ent);
                                        foreach (var property in propertyInfo)
                                        {
                                            if (inputProperties.TryGetValue(property.Name, out object value))
                                            {
                                                property.SetValue(ent, value);
                                                outputProperties[property.Name] = value;
                                            }
                                        }

                                        entityInfos.Add(new {
                                            handle = handle,
                                            type = ent.GetType().Name,
                                            properties = outputProperties
                                        });
                                    }
                                    trans.Commit();
                                    doc.Editor.Regen();
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
                            throw new System.Exception($"Invalid handle! Handle {handle} does not correspond to any object in the database.");
                        }
                    }

                    return entityInfos;
                },
                (isSuccess) => isSuccess ? "Entity info retrieved successfully!" : "Failed to retrieve entity info!"
            );
        }

    }
} 