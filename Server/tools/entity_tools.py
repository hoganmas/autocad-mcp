from typing import Any, Dict, List
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_entity_tools(mcp):
    """Register all entity-related tools with the MCP server."""

    @mcp.tool()
    def get_all_entities(ctx: Context) -> List[int]:
        """Get all entities in the current drawing.

        Returns:
            List[int]: List of entity handles
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("GET_ALL_ENTITIES")

            if not response.get("success", False):
                return f"Error getting all entities: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error getting all entities: {str(e)}"

    @mcp.tool()
    def get_selected_entities(ctx: Context) -> List[int]:
        """Get all selected entities in the current drawing.

        Returns:
            List[int]: List of entity handles
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("GET_SELECTED_ENTITIES")

            if not response.get("success", False):
                return f"Error getting selected entities: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error getting selected entities: {str(e)}"

    