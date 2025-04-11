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
    public static class TextCommands
    {
        [MCPCommand("CREATE_DIMENSION")]
        public static object CreateDimension(JObject parameters)
        {
            var dimstyle = Application.DocumentManager.MdiActiveDocument.Database.Dimstyle;

            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    var startPoint = parameters["startPoint"].ToObject<double[]>();
                    var endPoint = parameters["endPoint"].ToObject<double[]>();
                    
                    var dimensionLinePoint = parameters["dimensionLinePoint"].ToObject<double[]>();

                    var text = parameters.ContainsKey("text") ? parameters["text"].Value<string>() : "";

                    var dimension = new AlignedDimension(
                        new Point3d(startPoint[0], startPoint[1], startPoint.Length > 2 ? startPoint[2] : 0),
                        new Point3d(endPoint[0], endPoint[1], endPoint.Length > 2 ? endPoint[2] : 0),
                        new Point3d(dimensionLinePoint[0], dimensionLinePoint[1], dimensionLinePoint.Length > 2 ? dimensionLinePoint[2] : 0),
                        text,
                        dimstyle
                    );

                    btr.AppendEntity(dimension);
                    trans.AddNewlyCreatedDBObject(dimension, true);

                    return new {
                        handle = dimension.Handle.Value,
                        type = dimension.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(dimension)
                    };
                },
                (isSuccess) => isSuccess ? "Dimension created successfully!" : "Failed to create dimension!"
            );
        }

        [MCPCommand("CREATE_TEXT_LABEL")]
        public static object CreateTextLabel(JObject parameters)
        {
            return CommandTemplates.Modify(parameters, 
                (btr, trans, parameters) => {
                    var position = parameters["position"].ToObject<double[]>();
                    var height = parameters["height"].Value<double>();
                    var rotation = parameters.ContainsKey("rotation") ? parameters["rotation"].Value<double>() : 0;

                    var textString = parameters["text"].Value<string>();
                    
                    DBText text = new DBText();
                    text.SetDatabaseDefaults();
                    
                    text.TextString = textString;
                    text.Height = height;
                    text.Rotation = rotation;
                    text.Position = new Point3d(position[0], position[1], position.Length > 2 ? position[2] : 0);
                    
                    if (parameters.TryGetValue("normal", out var normalParam))
                    {
                        var normal = normalParam.ToObject<double[]>();
                        text.Normal = new Vector3d(normal[0], normal[1], normal.Length > 2 ? normal[2] : 0);
                    }

                    if (parameters.TryGetValue("horizontalMode", out var horizontalModeParam))
                    {
                        var horizontalMode = (TextHorizontalMode)Enum.Parse(typeof(TextHorizontalMode), horizontalModeParam.Value<string>(), true);
                        text.HorizontalMode = horizontalMode;
                    }

                    btr.AppendEntity(text);
                    trans.AddNewlyCreatedDBObject(text, true);

                    return new {
                        handle = text.Handle.Value,
                        type = text.GetType().Name,
                        properties = EntityCommands.GetEntityProperties(text)
                    };
                },
                (isSuccess) => isSuccess ? "Text label created successfully!" : "Failed to create text label!"
            );
        }
    }
}

