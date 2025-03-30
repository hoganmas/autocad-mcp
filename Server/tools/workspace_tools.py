from typing import Any, Dict
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_workspace_tools(mcp):
    """Register all workspace-related tools with the MCP server."""

    @mcp.tool()
    def get_current_workspace(ctx: Context) -> str:
        """Get the current workspace mode in AutoCAD.

        Returns:
            str: Current workspace mode
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("GET_CURRENT_WORKSPACE")

            if not response.get("success", False):
                return f"Error getting current workspace: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error getting current workspace: {str(e)}"

    @mcp.tool()
    def set_current_workspace(ctx: Context, workspace: str) -> str:
        """Set the current workspace in AutoCAD.

        Args:
            workspace: Name of the workspace to set (e.g., "Drafting & Annotation" or "3D Modeling")

        Returns:
            str: Current workspace mode
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("SET_CURRENT_WORKSPACE", {"workspace": workspace})

            if not response.get("success", False):
                return f"Error setting current workspace: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error setting current workspace: {str(e)}"

    