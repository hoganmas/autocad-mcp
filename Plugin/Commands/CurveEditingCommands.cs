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
    public static class CurveEditingCommands
    {
        [MCPCommand("OFFSET_CURVE")]
        public static object OffsetCurve(JObject parameters)
        {
            return CommandTemplates.ModifyEntity(parameters,
                (ent, btr, trans, parameters) => {
                    // Extract parameters
                    var distance = parameters["distance"].Value<double>();

                    var entityIds = new List<long>();

                    // Get the curve entity
                    if (ent is Curve curve)
                    {
                        // Offset the curve
                        var offsetCurves = curve.GetOffsetCurves(distance);

                        // Add the offset curves to the drawing
                        foreach (var offsetCurve in offsetCurves)
                        {
                            if (offsetCurve is Curve offsetCurveEntity)
                            {
                                btr.AppendEntity(offsetCurveEntity);
                                trans.AddNewlyCreatedDBObject(offsetCurveEntity, true);
                                entityIds.Add(offsetCurveEntity.Handle.Value);
                            }
                        }

                        return entityIds;
                    }
                    else
                    {
                        throw new System.Exception("Entity is not a curve");
                    }
                }
            );
        }

        [MCPCommand("CREATE_REGIONS")]
        public static object CreateRegions(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (entities, btr, trans, parameters) => {
                    DBObjectCollection curves = new DBObjectCollection();

                    foreach (var ent in entities)
                    {
                        if (ent is Curve curve)
                        {
                            curves.Add(curve);
                        }
                        else
                        {
                            throw new System.Exception($"Entity {ent.Handle.Value} ({ent.GetType().Name}) is not a curve!");
                        }
                    }

                    using (DBObjectCollection regionCollection = Region.CreateFromCurves(curves))
                    {
                        if (regionCollection.Count == 0)
                        {
                            throw new System.Exception("Failed to create region.");
                        }

                        var regionInfos = new List<object>();

                        Log.Info($"Created {regionCollection.Count} regions");

                        foreach (Region region in regionCollection)
                        {
                            btr.AppendEntity(region);
                            trans.AddNewlyCreatedDBObject(region, true);

                            regionInfos.Add(new {
                                handle = region.Handle.Value,
                                type = region.GetType().Name,
                                properties = EntityCommands.GetEntityProperties(region)
                            });
                        }

                        return regionInfos;
                    }
                },
                (isSuccess) => isSuccess ? "Region created successfully!" : "Failed to create region!"
            );
        }

        [MCPCommand("EXTRUDE_REGIONS")]
        public static object ExtrudeRegions(JObject parameters)
        {
            return CommandTemplates.ModifyEachEntity(parameters,
                (ent, btr, trans, parameters) => {

                    Log.Info($"Extruding region {ent.Handle.Value}");
                    // Extract parameters
                    var distance = parameters["distance"].Value<double>();

                    Log.Info($"Distance: {distance}");

                    if (ent is Region region)
                    {
                        Log.Info($"Region: {region.Handle.Value}");
                        // Create a Solid3D from the region
                        Solid3d solid = new Solid3d();
                        solid.Extrude(region, distance, 0);

                        Log.Info($"Solid: {solid.Handle.Value}");

                        // Add the new solid to Model Space
                        btr.AppendEntity(solid);
                        trans.AddNewlyCreatedDBObject(solid, true);

                        Log.Info($"Solid added to drawing");

                        return new {
                            handle = solid.Handle.Value,
                            type = solid.GetType().Name,
                            properties = EntityCommands.GetEntityProperties(solid)
                        };
                    }
                    else
                    {
                        throw new System.Exception("Entity is not a region");
                    }
                },
                (isSuccess) => isSuccess ? "Regions extruded successfully!" : "Failed to extrude regions!"
            );
        }
    }
}
