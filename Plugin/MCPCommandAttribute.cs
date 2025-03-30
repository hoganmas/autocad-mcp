using System;

namespace AutoCADMCP
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MCPCommandAttribute : Attribute
    {
        public string CommandType { get; }

        public MCPCommandAttribute(string commandType)
        {
            CommandType = commandType;
        }
    }
} 