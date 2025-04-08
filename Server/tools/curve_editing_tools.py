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
        """Creates a region from a list of 2D entities in AutoCAD. Deletes the input entities.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to create the region from

        Requires:
            len(entity_handles) > 0
            entity_handles must represent a set of closed 2D loops

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
    def extrude_regions(
        ctx: Context,
        entity_handles: List[int],
        distances: List[float]
    ) -> List[Dict[str, Any]]:
        """Extrude a region in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the regions to extrude
            distances: The extrusion distances for each region

        Requires:
            len(entity_handles) == len(distances)
            distances must be a list of positive numbers
            entity_handles must represent a set of regions. This will not work for other entity types.

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the handle, type, and properties of the newly created extruded solids
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("EXTRUDE_REGIONS", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "distance": distance
                    }
                    for distance in distances
                ]
            })

            if not response.get("success", False):
                return f"Error extruding regions: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error extruding regions: {str(e)}"

    @mcp.tool()
    def combine_regions(
        ctx: Context,
        entity_handles: List[int],
        operation_type: str
    ) -> Dict[str, Any]:
        """Applies a boolean operation to the regions to create a new region. This deletes the input regions.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the regions to combine
            operation_type: The type of operation to perform. Must be one of "union", "intersection", or "difference" (case-insensitive).

        Requires:
            len(entity_handles) > 0
            entity_handles must represent a set of regions. This will not work for other entity types.
            operation_type must be one of "union", "intersection", or "difference" (case-insensitive).

        Returns:
            Dict[str, Any]: Dictionary containing the handle, type, and properties of the newly created combined region

            For a union operation, the result will be the set union of the input regions.
            For an intersection operation, the result will be the set intersection of the input regions.
            For a difference operation, the result will be the set difference of the first region and the union of all subsequent regions.
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("COMBINE_REGIONS", {
                "entityIds": entity_handles,
                "operationType": operation_type.lower()
            })

            if not response.get("success", False):
                return f"Error combining regions: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error combining regions: {str(e)}"
