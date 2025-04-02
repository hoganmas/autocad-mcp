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
    public static class SolidCreationCommands
    {
        [MCPCommand("CREATE_BOX")]
        public static object CreateBox(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var size = parameters["size"].ToObject<double[]>();
                    
                    using (Solid3d box = new Solid3d())
                    {
                        box.SetDatabaseDefaults();
                        box.CreateBox(size[0], size[1], size[2]);
                        box.TransformBy(Matrix3d.Displacement(new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0)));

                        btr.AppendEntity(box);
                        trans.AddNewlyCreatedDBObject(box, true);
                        return box.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Box created successfully!" : "Failed to create box!"
            );
        }

        [MCPCommand("CREATE_FRUSTUM")]
        public static object CreateFrustum(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var radiusX = parameters["radiusX"].Value<double>();
                    var radiusY = parameters["radiusY"].Value<double>();
                    var topRadius = parameters["topRadius"].Value<double>();
                    var height = parameters["height"].Value<double>();
                    var centerPoint = new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0);
                                        
                    using (Solid3d frustum = new Solid3d())
                    {
                        frustum.SetDatabaseDefaults();
                        frustum.CreateFrustum(height, radiusX, radiusY, topRadius);
                        frustum.TransformBy(Matrix3d.Displacement(centerPoint));

                        btr.AppendEntity(frustum);
                        trans.AddNewlyCreatedDBObject(frustum, true);
                        return frustum.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Frustum created successfully!" : "Failed to create frustum!"
            );
        }

        [MCPCommand("CREATE_SPHERE")]
        public static object CreateSphere(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var radius = parameters["radius"].Value<double>();
                    var centerPoint = new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0);

                    using (Solid3d sphere = new Solid3d())
                    {
                        sphere.SetDatabaseDefaults();
                        sphere.CreateSphere(radius);
                        sphere.TransformBy(Matrix3d.Displacement(centerPoint));

                        btr.AppendEntity(sphere);
                        trans.AddNewlyCreatedDBObject(sphere, true);
                        return sphere.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Sphere created successfully!" : "Failed to create sphere!"
            );
        }

        [MCPCommand("CREATE_TORUS")]
        public static object CreateTorus(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters   
                    var center = parameters["center"].ToObject<double[]>();
                    var radius = parameters["radius"].Value<double>();
                    var tubeRadius = parameters["tubeRadius"].Value<double>();
                    var centerPoint = new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0);
                    
                    using (Solid3d torus = new Solid3d())
                    {   
                        torus.SetDatabaseDefaults();
                        torus.CreateTorus(radius, tubeRadius);
                        torus.TransformBy(Matrix3d.Displacement(centerPoint));

                        btr.AppendEntity(torus);
                        trans.AddNewlyCreatedDBObject(torus, true);
                        return torus.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Torus created successfully!" : "Failed to create torus!"
            );
        }
        
        [MCPCommand("CREATE_PYRAMID")]
        public static object CreatePyramid(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var height = parameters["height"].Value<double>();
                    var sides = parameters["sides"].Value<int>();
                    var radius = parameters["radius"].Value<double>();
                    var topRadius = parameters["topRadius"].Value<double>();
                    var centerPoint = new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0);
                    
                    using (Solid3d pyramid = new Solid3d())
                    {
                        pyramid.SetDatabaseDefaults();
                        pyramid.CreatePyramid(height, sides, radius, topRadius);
                        pyramid.TransformBy(Matrix3d.Displacement(centerPoint));

                        btr.AppendEntity(pyramid);
                        trans.AddNewlyCreatedDBObject(pyramid, true);
                        return pyramid.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Pyramid created successfully!" : "Failed to create pyramid!"
            );
        }
        
        [MCPCommand("CREATE_WEDGE")]
        public static object CreateWedge(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var size = parameters["size"].ToObject<double[]>();
                    var centerPoint = new Vector3d(center[0], center[1], center.Length > 2 ? center[2] : 0);
                    
                    using (Solid3d wedge = new Solid3d())
                    {
                        wedge.SetDatabaseDefaults();
                        wedge.CreateWedge(size[0], size[1], size[2]);
                        wedge.TransformBy(Matrix3d.Displacement(centerPoint));

                        btr.AppendEntity(wedge);
                        trans.AddNewlyCreatedDBObject(wedge, true);
                        return wedge.Handle.Value;
                    }
                },
                (isSuccess) => isSuccess ? "Wedge created successfully!" : "Failed to create wedge!"
            );
        }
    }        
}