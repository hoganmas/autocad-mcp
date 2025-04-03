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
        internal static List<PropertyInfo> GetEntityPropertyInfo(Entity ent)
        {
            var type = ent.GetType();
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList().Where(
                p => p.CanRead && p.CanWrite && p.DeclaringType == type
            ).ToList();
        }

        internal static Dictionary<string, object> GetEntityProperties(Entity ent)
        {
            var propertyInfo = GetEntityPropertyInfo(ent);
            return propertyInfo.ToDictionary(p => p.Name, p => p.GetValue(ent));
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
            return CommandTemplates.ModifyEachEntity(parameters,
                (ent, btr, trans, parameters) => {
                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = GetEntityProperties(ent)
                    };                                    
                }
            );
        }

        [MCPCommand("SET_ENTITY_PROPERTIES")]
        public static object SetEntityProperties(JObject parameters)
        {
            return CommandTemplates.ModifyEachEntity(parameters,
                (ent, btr, trans, parameters) => {
                    // Set the properties of the entity
                    var inputProperties = parameters.ToObject<Dictionary<string, object>>();
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

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = outputProperties
                    };
                },
                (isSuccess) => isSuccess ? "Entity properties set successfully!" : "Failed to set entity properties!"
            );
        }
    }
} 