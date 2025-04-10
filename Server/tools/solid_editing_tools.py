from typing import Optional, List, Dict, Any
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_solid_editing_tools(mcp: FastMCP):
    """Register all solid 3D editing tools with the MCP server."""
    
    @mcp.tool()
    def combine_solids(
        ctx: Context,
        entities: List[int],
        operation_type: str
    ) -> Dict[str, Any]:
        """Applies a boolean operation to the solids to create a new solid. This deletes the input solids.

        Args:
            ctx: The MCP context
            entities: A list of entity handles to combine
            operation_type: The type of operation to perform. Must be one of "union", "intersection", or "difference" (case-insensitive).

        Returns:
            Dict[str, Any]: Dictionary containing the handle, type, and properties of the newly created combined solid

            For a union operation, the result will be the set union of the input solids.
            For an intersection operation, the result will be the set intersection of the input solids.
            For a difference operation, the result will be the set difference of the first solid and the union of all subsequent solids.
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("COMBINE_SOLIDS", {
                "entityIds": entities,
                "operationType": operation_type.lower()
            })

            if not response.get("success", False):
                return f"Error combining solids: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error combining solids: {str(e)}"

    @mcp.tool()
    def sweep_solid(
        ctx: Context,
        profile: int,
        path: int,
        options: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Sweeps a profile along a path to create a new solid. This deletes the input profile and path.

        Args:
            ctx: The MCP context
            profile: The entity handle of the profile to sweep
            path: The entity handle of the path to sweep along
            options: A dictionary of options for the sweep (see below)

        Options:
            align: The alignment of the profile. Must be one of "NoAlignment", "AlignSweepEntityToPath", "TranslateSweepEntityToPath", or "TranslatePathToSweepEntity" (case-insensitive).
            bank: Whether to bank the profile.
            basePoint: The base point of the profile.
            draftAngle: The draft angle of the profile.
            twistAngle: The twist angle of the profile.
            scaleFactor: The scale factor of the profile.

        Returns:
            Dict[str, Any]: Dictionary containing the handle, type, and properties of the newly created swept solid
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("SWEEP_SOLID", {
                "entityIds": [profile, path],
                "options": options
            })

            if not response.get("success", False):
                return f"Error sweeping solid: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error sweeping solid: {str(e)}"
