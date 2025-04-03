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
    public static class EditingCommands
    {
        [MCPCommand("MOVE_ENTITIES")]
        public static object MoveEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var delta = parameters["delta"].ToObject<double[]>();
                    var deltaVector = new Vector3d(delta[0], delta[1], delta.Length > 2 ? delta[2] : 0);
                    ent.TransformBy(Matrix3d.Displacement(deltaVector));

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(ent)
                    };
                },
                (isSuccess) => isSuccess ? "Entities moved successfully!" : "Failed to move entities!"
            );
        }

        [MCPCommand("ROTATE_ENTITIES")]
        public static object RotateEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var angle = parameters["angle"].ToObject<double>();
                    var axis = parameters["axis"].ToObject<double[]>();
                    var origin = parameters["origin"].ToObject<double[]>();
                    var axisVector = new Vector3d(axis[0], axis[1], axis.Length > 2 ? axis[2] : 0);
                    var originPoint = new Point3d(origin[0], origin[1], origin.Length > 2 ? origin[2] : 0);

                    ent.TransformBy(Matrix3d.Rotation(angle, axisVector, originPoint));

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(ent)
                    };
                },
                (isSuccess) => isSuccess ? "Entities rotated successfully!" : "Failed to rotate entities!"
            );
        }

        [MCPCommand("SCALE_ENTITIES")]
        public static object ScaleEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var scale = parameters["scale"].ToObject<double>();
                    var origin = parameters["origin"].ToObject<double[]>();
                    var originPoint = new Point3d(origin[0], origin[1], origin.Length > 2 ? origin[2] : 0);

                    ent.TransformBy(Matrix3d.Scaling(scale, originPoint));

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(ent)
                    };
                }
            );
        }

        [MCPCommand("MIRROR_ENTITIES")]
        public static object MirrorEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var origin = parameters["origin"].ToObject<double[]>();
                    var normal = parameters["normal"].ToObject<double[]>();
                    var originPoint = new Point3d(origin[0], origin[1], origin.Length > 2 ? origin[2] : 0);
                    var normalVector = new Vector3d(normal[0], normal[1], normal.Length > 2 ? normal[2] : 0);
                    var plane = new Plane(originPoint, normalVector);

                    ent.TransformBy(Matrix3d.Mirroring(plane));

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(ent)
                    };
                },
                (isSuccess) => isSuccess ? "Entities mirrored successfully!" : "Failed to mirror entities!"
            );
        }

        [MCPCommand("DELETE_ENTITIES")]
        public static object DeleteEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    ent.Erase();

                    return new {
                        handle = ent.Handle.Value,
                        type = ent.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(ent)
                    };
                },
                (isSuccess) => isSuccess ? "Entities deleted successfully!" : "Failed to delete entities!"
            );
        }

        [MCPCommand("DUPLICATE_ENTITIES")]
        public static object DuplicateEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var delta = parameters["delta"].ToObject<double[]>();
                    var deltaVector = new Vector3d(delta[0], delta[1], delta.Length > 2 ? delta[2] : 0);

                    var copy = ent.Clone() as Entity;
                    btr.AppendEntity(copy);
                    trans.AddNewlyCreatedDBObject(copy, true);

                    copy.TransformBy(Matrix3d.Displacement(deltaVector));

                    return new {
                        handle = copy.Handle.Value,
                        type = copy.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(copy)
                    };
                },
                (isSuccess) => isSuccess ? "Entities duplicated successfully!" : "Failed to duplicate entities!"
            );
        }

        [MCPCommand("EXPLODE_ENTITIES")]
        public static object ExplodeEntities(JObject parameters)
        {
            return CommandTemplates.ModifyEntities(parameters,
                (ent, btr, trans, parameters) => {
                    var fragments = new DBObjectCollection();
                    ent.Explode(fragments);

                    var entityInfos = new List<object>();

                    foreach (var fragment in fragments)
                    {
                        if (fragment is Entity entity)
                        {
                            btr.AppendEntity(entity);
                            trans.AddNewlyCreatedDBObject(entity, true);
                            entityInfos.Add(new {
                                handle = entity.Handle.Value,
                                type = entity.GetType().Name,
                                properties = EntityCommands.GetEntityProperties(entity)
                            });
                        }
                    }

                    ent.Erase();

                    return entityInfos;
                },
                (isSuccess) => isSuccess ? "Entities exploded successfully!" : "Failed to explode entities!"
            );
        }
    }
}