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
    }
}
