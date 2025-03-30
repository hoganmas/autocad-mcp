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
    public static class ShapeCommandHandler
    {
        [MCPCommand("DRAW_CIRCLE")]
        public static object DrawCircle(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var radius = parameters["radius"].Value<double>();
                    var centerPoint = new Point3d(center[0], center[1], center.Length > 2 ? center[2] : 0);

                    // Create a circle
                    using (Circle circle = new Circle(centerPoint, Vector3d.ZAxis, radius))
                    {
                        // Add the circle to the drawing
                        btr.AppendEntity(circle);
                        trans.AddNewlyCreatedDBObject(circle, true);
                    }
                },
                (isSuccess) => isSuccess ? "Circle created successfully!" : "Failed to create circle!"
            );
        }

        [MCPCommand("DRAW_LINE")]
        public static object DrawLine(JObject parameters)
        {
            return CommandTemplates.Modify(parameters,
                (btr, trans, parameters) => {
                    // Extract parameters
                    var start = parameters["start"].ToObject<double[]>();
                    var end = parameters["end"].ToObject<double[]>();
                    var startPoint = new Point3d(start[0], start[1], start.Length > 2 ? start[2] : 0);
                    var endPoint = new Point3d(end[0], end[1], end.Length > 2 ? end[2] : 0);

                    // Create a line
                    using (Line line = new Line(startPoint, endPoint))
                    {
                        btr.AppendEntity(line);
                        trans.AddNewlyCreatedDBObject(line, true);
                    }
                },  
                (isSuccess) => isSuccess ? "Line created successfully!" : "Failed to create line!"
            );
        }

        [MCPCommand("DRAW_POLYLINE")]
        public static object DrawPolyline(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var points = parameters["points"].ToObject<double[][]>();

                    using (Polyline polyline = new Polyline())
                    {
                        // Add points to the polyline
                        for (int i = 0; i < points.Length; i++)
                        {
                            var point = points[i];
                            var pt2d = new Point2d(point[0], point[1]);
                            polyline.AddVertexAt(i, pt2d, 0, 0, 0);
                        }
                    
                        // Add the polyline to the drawing
                        btr.AppendEntity(polyline);
                        trans.AddNewlyCreatedDBObject(polyline, true);
                    }
                },
                (isSuccess) => isSuccess ? "Polyline created successfully!" : "Failed to create polyline!"
            );
        }

        [MCPCommand("DRAW_RECTANGLE")]
        public static object DrawRectangle(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var width = parameters["width"].Value<double>();
                    var height = parameters["height"].Value<double>();
                    var centerPoint = new Point3d(center[0], center[1], center.Length > 2 ? center[2] : 0);

                    // Create a rectangle using a polyline
                    using (Polyline rectangle = new Polyline())
                    {
                        // Calculate corner points
                        double halfWidth = width / 2;
                        double halfHeight = height / 2;
                        
                        // Add vertices in clockwise order
                        rectangle.AddVertexAt(0, new Point2d(centerPoint.X - halfWidth, centerPoint.Y - halfHeight), 0, 0, 0);
                        rectangle.AddVertexAt(1, new Point2d(centerPoint.X + halfWidth, centerPoint.Y - halfHeight), 0, 0, 0);
                        rectangle.AddVertexAt(2, new Point2d(centerPoint.X + halfWidth, centerPoint.Y + halfHeight), 0, 0, 0);
                        rectangle.AddVertexAt(3, new Point2d(centerPoint.X - halfWidth, centerPoint.Y + halfHeight), 0, 0, 0);
                        
                        // Close the rectangle
                        rectangle.Closed = true;

                        btr.AppendEntity(rectangle);
                        trans.AddNewlyCreatedDBObject(rectangle, true);
                    }
                },
                (isSuccess) => isSuccess ? "Rectangle created successfully!" : "Failed to create rectangle!"
            );
        }

        [MCPCommand("DRAW_ELLIPSE")]
        public static object DrawEllipse(JObject parameters)
        {
            return CommandTemplates.Modify(parameters,
                (btr, trans, parameters) => {
                    // Extract parameters
                    var center = parameters["center"].ToObject<double[]>();
                    var majorAxis = parameters["majorAxis"].ToObject<double[]>();
                    var minorAxis = parameters["minorAxis"].ToObject<double[]>();
                    
                    var centerPoint = new Point3d(center[0], center[1], center.Length > 2 ? center[2] : 0);
                    var majorVec = new Vector3d(majorAxis[0], majorAxis[1], majorAxis.Length > 2 ? majorAxis[2] : 0);
                    var minorVec = new Vector3d(minorAxis[0], minorAxis[1], minorAxis.Length > 2 ? minorAxis[2] : 0);

                    // Create an ellipse
                    using (Ellipse ellipse = new Ellipse(centerPoint, Vector3d.ZAxis, majorVec, minorVec.Length / majorVec.Length, 0, 2 * Math.PI))
                    {
                        btr.AppendEntity(ellipse);
                        trans.AddNewlyCreatedDBObject(ellipse, true);
                    }
                },
                (isSuccess) => isSuccess ? "Ellipse created successfully!" : "Failed to create ellipse!"
            );
        }   

        [MCPCommand("DRAW_POLYGON")]
        public static object DrawPolygon(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var points = parameters["points"].ToObject<double[][]>();

                    // Create a polygon using a polyline
                    using (Polyline polygon = new Polyline())
                    {
                        // Add points to the polygon
                        for (int i = 0; i < points.Length; i++)
                        {
                            var point = points[i];
                            var pt2d = new Point2d(point[0], point[1]);
                            polygon.AddVertexAt(i, pt2d, 0, 0, 0);
                        }

                        // Close the polygon
                        polygon.Closed = true;

                        // Add the polygon to the drawing
                        btr.AppendEntity(polygon);
                        trans.AddNewlyCreatedDBObject(polygon, true);
                    }
                },
                (isSuccess) => isSuccess ? "Polygon created successfully!" : "Failed to create polygon!"
            );
        }

        [MCPCommand("DRAW_POLYFACE")]
        public static object DrawPolyface(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    // Extract parameters
                    var points = parameters["points"].ToObject<double[][]>();

                    // Create a 3D polyline instead of a polyface mesh
                    using (Polyline3d polyline3d = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection(), true))
                    {
                        // Add points to the 3D polyline
                        foreach (var point in points)
                        {
                            var pt3d = new Point3d(point[0], point[1], point.Length > 2 ? point[2] : 0);
                            polyline3d.AppendVertex(new PolylineVertex3d(pt3d));
                        }

                        // Add to the drawing
                        btr.AppendEntity(polyline3d);
                        trans.AddNewlyCreatedDBObject(polyline3d, true);
                    }
                },
                (isSuccess) => isSuccess ? "3D polyline created successfully!" : "Failed to create 3D polyline!"
            );
        }
    }
}