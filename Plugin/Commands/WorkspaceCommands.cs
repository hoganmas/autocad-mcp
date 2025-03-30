using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCADMCP.Commands
{
    public static class WorkspaceCommandHandler
    {
        [MCPCommand("GET_CURRENT_WORKSPACE")]
        public static object GetCurrentWorkspace(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    return Application.GetSystemVariable("WSCURRENT") as string;
                },
                (isSuccess) => isSuccess ? "Workspace retrieved successfully!" : "Failed to retrieve workspace!"
            );
        }

        [MCPCommand("SET_CURRENT_WORKSPACE")]
        public static object SetCurrentWorkspace(JObject parameters)
        {
            return CommandTemplates.Run(parameters,
                (doc, parameters) => {
                    string workspace = parameters["workspace"].ToString();
                    Application.SetSystemVariable("WSCURRENT", workspace);
                    return Application.GetSystemVariable("WSCURRENT") as string;
                },
                (isSuccess) => isSuccess ? "Workspace set successfully!" : "Failed to set workspace!"
            );
        }
    }
} 