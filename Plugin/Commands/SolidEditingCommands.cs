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
    public static class SolidEditingCommands
    {
        [MCPCommand("COMBINE_SOLIDS")]
        public static object CombineSolids(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters, 
                (entities, btr, trans, parameters) => {
                    var operationName = parameters["operationType"].Value<string>();

                    BooleanOperationType operationType;

                    if (operationName == "union")
                    {
                        operationType = BooleanOperationType.BoolUnite;
                    }
                    else if (operationName == "intersection")
                    {
                        operationType = BooleanOperationType.BoolIntersect;
                    }
                    else if (operationName == "difference")
                    {
                        operationType = BooleanOperationType.BoolSubtract;
                    }
                    else
                    {
                        throw new System.Exception($"Invalid operation type: {operationName}");
                    }

                    var initialSolid = entities[0] as Solid3d;
                    if (initialSolid == null)
                    {
                        throw new System.Exception($"Entity {entities[0].Handle.Value} ({entities[0].GetType().Name}) is not a solid!");
                    }
                    
                    for (int i = 1; i < entities.Count; i++)
                    {
                        if (entities[i] is Solid3d solid)
                        {
                            initialSolid.BooleanOperation(operationType, solid);
                        }
                        else
                        {
                            throw new System.Exception($"Entity {entities[i].Handle.Value} ({entities[i].GetType().Name}) is not a solid!");
                        }
                    }
                    
                    return new {
                        handle = initialSolid.Handle.Value,
                        type = initialSolid.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(initialSolid)
                    };
                },
                (isSuccess) => isSuccess ? "Solids combined successfully!" : "Failed to combine solids!"
            );
        }

        [MCPCommand("SWEEP_SOLID")]
        public static object SweepSolid(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters, 
                (entities, btr, trans, parameters) => {
                    if (entities.Count != 2)
                    {
                        throw new System.Exception("Sweep solid requires exactly two entities!");
                    }

                    var profile = entities[0];
                    var path = entities[1];

                    SweepOptionsBuilder builder = new();
                    var options = parameters["options"].ToObject<JObject>();

                    if (options.ContainsKey("align"))
                    {
                        builder.Align = (SweepOptionsAlignOption)Enum.Parse(typeof(SweepOptionsAlignOption), options["align"].Value<string>(), true);
                    }

                    if (options.ContainsKey("bank"))
                    {
                        builder.Bank = options["bank"].Value<bool>();
                    }

                    if (options.ContainsKey("basePoint"))
                    {
                        var basePoint = options["basePoint"].ToObject<double[]>();
                        builder.BasePoint = new Point3d(basePoint[0], basePoint[1], basePoint.Length > 2 ? basePoint[2] : 0);
                    }

                    if (options.ContainsKey("draftAngle"))
                    {
                        builder.DraftAngle = options["draftAngle"].Value<double>();
                    }

                    if (options.ContainsKey("twistAngle"))
                    {
                        builder.TwistAngle = options["twistAngle"].Value<double>();
                    }

                    if (options.ContainsKey("scaleFactor"))
                    {
                        builder.ScaleFactor = options["scaleFactor"].Value<double>();
                    }

                    SweepOptions sweepOptions = builder.ToSweepOptions();

                    var solid = new Solid3d();
                    solid.SetDatabaseDefaults();
                    solid.CreateSweptSolid(profile, path, sweepOptions);

                    btr.AppendEntity(solid);
                    trans.AddNewlyCreatedDBObject(solid, true);
                    
                    profile.Erase();
                    path.Erase();

                    return new {
                        handle = solid.Handle.Value,
                        type = solid.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(solid)
                    };
                },
                (isSuccess) => isSuccess ? "Sweep solid created successfully!" : "Failed to sweep solid!"
            );
        }
    }
}