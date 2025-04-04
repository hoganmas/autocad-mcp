from typing import Optional, List, Dict, Any
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_curve_editing_tools(mcp: FastMCP):
    """Register all curve editing tools with the MCP server."""

    @mcp.tool()
    def offset_curve(
        ctx: Context,
        entity_handle: int,
        distance: float
    ) -> List[Dict[str, Any]]:
        """Offset a curve in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handle: The handle of the entity to offset
            distance: The offset distance

        Requires:
            distance > 0

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the handle, type, and properties of the offset curves
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("OFFSET_CURVE", {
                "entityId": entity_handle,
                "distance": distance
            })

            if not response.get("success", False):
                return f"Error offsetting curve: {response.get('error', 'Unknown error')}"  
                
            return response.get("result")
        except Exception as e:
            return f"Error offsetting curve: {str(e)}"

    @mcp.tool()
    def create_region(
        ctx: Context,
        entity_handles: List[int]
    ) -> Dict[str, Any]:
        """Create a region from a list of 2D entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to create the region from

        Requires:
            len(entity_handles) > 0

        Returns:
            Dict[str, Any]: Dictionary containing the handle, type, and properties of the created region
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_REGION", {
                "entityIds": entity_handles
            })

            if not response.get("success", False):
                return f"Error creating region: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating region: {str(e)}"

    @mcp.tool()
    def extrude_region(
        ctx: Context,
        entity_handle: int,
        distance: float
    ) -> Dict[str, Any]:
        """Extrude a region in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handle: The handle of the entity to extrude
            distance: The extrusion distance

        Requires:
            distance > 0

        Returns:
            Dict[str, Any]: Dictionary containing the handle, type, and properties of the newly created extruded solid
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("EXTRUDE_REGION", {
                "entityId": entity_handle,
                "distance": distance
            })

            if not response.get("success", False):
                return f"Error extruding region: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error extruding region: {str(e)}"
