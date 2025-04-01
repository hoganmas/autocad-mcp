from typing import Optional, List
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_shape3d_tools(mcp: FastMCP):
    """Register all shape 3D management tools with the MCP server."""
    
    @mcp.tool()
    def create_box(ctx: Context, center: List[float], size: List[float]) -> int:
        """Create a 3D box.

        Args:
            ctx: The MCP context
            size: The size of the box [x, y, z]

        Returns:
            int: Entity handle of the newly created box
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_BOX", {
                "center": center,
                "size": size
            })

            if not response.get("success", False):
                return f"Error creating box: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating box: {str(e)}"

    @mcp.tool()
    def create_frustum(ctx: Context, center: List[float], radiusX: float, radiusY: float, topRadius: float, height: float) -> int:
        """Create a 3D frustum.

        Args:   
            ctx: The MCP context
            center: The center point of the frustum [x, y, z]
            radiusX: The radius of the frustum at the base [x]
            radiusY: The radius of the frustum at the base [y]
            topRadius: The radius of the frustum at the top [z]
            height: The height of the frustum [z]

        Returns:    
            int: Entity handle of the newly created frustum
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_FRUSTUM", {
                "center": center,   
                "radiusX": radiusX,
                "radiusY": radiusY,
                "height": height,
                "topRadius": topRadius
            })

            if not response.get("success", False):
                return f"Error creating frustum: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating frustum: {str(e)}"

    @mcp.tool()
    def create_sphere(ctx: Context, center: List[float], radius: float) -> int:
        """Create a 3D sphere.      

        Args:
            ctx: The MCP context
            center: The center point of the sphere [x, y, z]
            radius: The radius of the sphere            

        Returns:
            int: Entity handle of the newly created sphere
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_SPHERE", {  
                "center": center,
                "radius": radius
            })

            if not response.get("success", False):
                return f"Error creating sphere: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating sphere: {str(e)}"

    @mcp.tool()
    def create_torus(ctx: Context, center: List[float], radius: float, tubeRadius: float) -> int:
        """Create a 3D torus.   

        Args:
            ctx: The MCP context
            center: The center point of the torus [x, y, z]
            radius: The radius of the torus
            tubeRadius: The radius of the tube of the torus 

        Returns:
            int: Entity handle of the newly created torus
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_TORUS", {   
                "center": center,
                "radius": radius,
                "tubeRadius": tubeRadius
            })

            if not response.get("success", False):
                return f"Error creating torus: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating torus: {str(e)}"

    @mcp.tool()
    def create_pyramid(ctx: Context, center: List[float], height: float, sides: int, radius: float, topRadius: float) -> int:
        """Create a 3D pyramid.

        Args:
            ctx: The MCP context
            center: The center point of the pyramid [x, y, z]
            height: The height of the pyramid [z]
            sides: The number of sides of the pyramid
            radius: The radius of the pyramid at the base
            topRadius: The radius of the pyramid at the top

        Returns:
            int: Entity handle of the newly created pyramid
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_PYRAMID", {
                "center": center,
                "height": height,
                "sides": sides,
                "radius": radius,
                "topRadius": topRadius
            })

            if not response.get("success", False):
                return f"Error creating pyramid: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating pyramid: {str(e)}"

    @mcp.tool()
    def create_wedge(ctx: Context, center: List[float], size: List[float]) -> int:
        """Create a 3D wedge.

        Args:
            ctx: The MCP context
            center: The center point of the wedge [x, y, z]
            size: The size of the wedge [x, y, z]

        Returns:
            int: Entity handle of the newly created wedge   
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("CREATE_WEDGE", {
                "center": center,
                "size": size
            })

            if not response.get("success", False):
                return f"Error creating wedge: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating wedge: {str(e)}"
