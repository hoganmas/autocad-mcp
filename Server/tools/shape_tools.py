from typing import Optional
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_shape_tools(mcp: FastMCP):
    """Register all shape management tools with the MCP server."""
    
    @mcp.tool()
    def draw_circle(
        ctx: Context,
        center: list[float],
        radius: float
    ) -> str:
        """Draw a circle in AutoCAD.

        Args:
            ctx: The MCP context
            center: The center of the circle
            radius: The radius of the circle

        Returns:
            str: Success message or error details
        """
        try:
            autocad = get_autocad_connection()
                
            response = autocad.send_command("DRAW_CIRCLE", {
                "center": center,
                "radius": radius
            })
            
            if not response.get("success", False):
                return f"Error drawing circle: {response.get('error', 'Unknown error')} (Center: {center}, Radius: {radius})"
                
            return response.get("message", "Circle drawn successfully")
        except Exception as e:
            return f"Error drawing circle: {str(e)} (Center: {center}, Radius: {radius})"
