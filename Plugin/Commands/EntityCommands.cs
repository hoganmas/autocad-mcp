using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutoCADMCP.Commands
{
    public static class EntityCommandHandler
    {
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
    }
} 