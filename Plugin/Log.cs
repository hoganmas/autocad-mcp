using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace AutoCADMCP
{
    public static class Log
    {
        public static void Info(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] INFO: {message}");
        }

        public static void Warning(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] WARNING: {message}");
        }

        public static void Error(string message)
        {
            // Get the current document's editor to display the message
            Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage($"\n[AUTOCAD MCP] ERROR: {message}");
        }
    }
}
