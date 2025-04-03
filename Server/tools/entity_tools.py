from typing import Any, Dict, List
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_entity_tools(mcp: FastMCP):
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

    @mcp.tool()
    def get_entity_properties(ctx: Context, entity_handles: List[int]) -> List[Dict[str, Any]]:
        """Get properties of an entity.

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing entity types and properties
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("GET_ENTITY_PROPERTIES", {
                "entityIds": entity_handles
            })

            if not response.get("success", False):
                return f"Error getting entity properties: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error getting entity properties: {str(e)}"

    @mcp.tool()
    def set_entity_properties(ctx: Context, entity_handles: List[int], properties: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Set properties of an entity.

        Args:
            entity_handles: The handles of the entities to set properties for
            properties: The properties to set for each entity

        Requires:
            len(entity_handles) == len(properties)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing updated entity types and properties
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("SET_ENTITY_PROPERTIES", {
                "entityIds": entity_handles,
                "entityParameters": properties
            })

            if not response.get("success", False):
                return f"Error setting entity properties: {response.get('error', 'Unknown error')}"

            return response.get("result")
        except Exception as e:
            return f"Error setting entity properties: {str(e)}"
