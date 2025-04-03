from typing import Optional, List, Dict, Any
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_editing_tools(mcp: FastMCP):
    """Register all editing tools with the MCP server."""
    
    @mcp.tool()
    def move_entities(
        ctx: Context,
        entity_handles: List[int],
        deltas: List[List[float]]
    ) -> List[Dict[str, Any]]:
        """Move entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to move
            deltas: The deltas to move the entities by [x, y, z]

        Requires:
            len(entity_handles) == len(deltas)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the updated properties of the moved entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("MOVE_ENTITIES", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "delta": delta
                    }
                    for delta in deltas
                ]
            })
            
            if not response.get("success", False):
                return f"Error moving entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error moving entities: {str(e)}"
    
    @mcp.tool()
    def rotate_entities(
        ctx: Context,
        entity_handles: List[int],
        angles: List[float],
        axes: List[List[float]],
        origins: List[List[float]]
    ) -> List[Dict[str, Any]]:
        """Rotate entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to rotate
            angles: The angles to rotate the entities
            axes: The axes to rotate the entities around [x, y, z]
            origins: The origins to rotate the entities around [x, y, z]

        Requires:
            len(entity_handles) == len(angles) == len(axes) == len(origins)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the updated properties of the rotated entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("ROTATE_ENTITIES", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "angle": angle, 
                        "axis": axis,
                        "origin": origin
                    }
                    for angle, axis, origin in zip(angles, axes, origins)
                ]
            })

            if not response.get("success", False):
                return f"Error rotating entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error rotating entities: {str(e)}"

    @mcp.tool()
    def scale_entities(
        ctx: Context,
        entity_handles: List[int],
        scales: List[float],
        origins: List[List[float]]
    ) -> List[Dict[str, Any]]:
        """Scale entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to scale
            scales: The scales to scale the entities by
            origins: The origins to scale the entities around [x, y, z]

        Requires:
            len(entity_handles) == len(scales) == len(origins)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the updated properties of the scaled entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("SCALE_ENTITIES", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "scale": scale,
                        "origin": origin
                    }
                    for scale, origin in zip(scales, origins)
                ]
            })

            if not response.get("success", False):
                return f"Error scaling entities: {response.get('error', 'Unknown error')}"  
                
            return response.get("result")
        except Exception as e:
            return f"Error scaling entities: {str(e)}"

    @mcp.tool()
    def mirror_entities(
        ctx: Context,
        entity_handles: List[int],
        origins: List[List[float]],
        normals: List[List[float]]
    ) -> List[Dict[str, Any]]:
        """Mirror entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to mirror
            origins: The origins to mirror the entities around [x, y, z]
            normals: The normal of the plane to mirror the entities around [x, y, z]

        Requires:
            len(entity_handles) == len(origins) == len(normals)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the updated properties of the mirrored entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("MIRROR_ENTITIES", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "origin": origin,
                        "normal": normal
                    }
                    for origin, normal in zip(origins, normals)
                ]
            })

            if not response.get("success", False):
                return f"Error mirroring entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error mirroring entities: {str(e)}"

    @mcp.tool()
    def delete_entities(
        ctx: Context,
        entity_handles: List[int]
    ) -> List[Dict[str, Any]]:
        """Delete entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to delete

        Requires:
            len(entity_handles) > 0

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the updated properties of the deleted entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DELETE_ENTITIES", {
                "entityIds": entity_handles
            })

            if not response.get("success", False):
                return f"Error deleting entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error deleting entities: {str(e)}" 
        
    @mcp.tool()
    def duplicate_entities(
        ctx: Context,
        entity_handles: List[int],
        deltas: List[List[float]]
    ) -> List[Dict[str, Any]]:
        """Duplicate entities in AutoCAD.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to duplicate
            deltas: A positional offset applied to the newly created entities [x, y, z]

        Requires:
            len(entity_handles) == len(deltas)

        Returns:
            List[Dict[str, Any]]: List of dictionaries containing the properties of the newly created entities
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DUPLICATE_ENTITIES", {
                "entityIds": entity_handles,
                "entityParameters": [
                    {
                        "delta": delta
                    }
                    for delta in deltas
                ]
            })

            if not response.get("success", False):
                return f"Error duplicating entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error duplicating entities: {str(e)}"

    @mcp.tool()
    def explode_entities(
        ctx: Context,
        entity_handles: List[int]
    ) -> List[List[Dict[str, Any]]]:  
        """Explode entities in AutoCAD. This will split the entities into their individual components.

        Args:
            ctx: The MCP context
            entity_handles: The handles of the entities to explode

        Requires:
            len(entity_handles) > 0

        Returns:
            List[List[Dict[str, Any]]]: List of lists of dictionaries containing the handle, type, and properties of the resulting entities.
            Each list of dictionaries corresponds to the entities created from the entity at the same index in the entity_handles list.
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("EXPLODE_ENTITIES", {
                "entityIds": entity_handles
            })

            if not response.get("success", False):
                return f"Error exploding entities: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error exploding entities: {str(e)}"
