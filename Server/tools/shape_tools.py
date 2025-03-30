from typing import Optional, List
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_shape_tools(mcp: FastMCP):
    """Register all shape management tools with the MCP server."""
    
    @mcp.tool()
    def draw_circle(
        ctx: Context,
        center: List[float],
        radius: float
    ) -> int:
        """Draw a circle in AutoCAD.

        Args:
            ctx: The MCP context
            center: The center point coordinates [x, y, z]
            radius: The radius of the circle

        Returns:
            int: Entity handle of the newly created circle
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_CIRCLE", {
                "center": center,
                "radius": radius
            })
            
            if not response.get("success", False):
                return f"Error drawing circle: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing circle: {str(e)}"

    @mcp.tool()
    def draw_line(
        ctx: Context,
        start: List[float],
        end: List[float]
    ) -> int:
        """Draw a line in AutoCAD.

        Args:
            ctx: The MCP context
            start: The start point coordinates [x, y, z]
            end: The end point coordinates [x, y, z]

        Returns:
            int: Entity handle of the newly created line
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_LINE", {
                "start": start,
                "end": end
            })
            
            if not response.get("success", False):
                return f"Error drawing line: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing line: {str(e)}"

    @mcp.tool()
    def draw_polyline(
        ctx: Context,
        points: List[List[float]]
    ) -> int:
        """Draw a polyline in AutoCAD.

        Args:
            ctx: The MCP context
            points: List of point coordinates [[x1, y1, z1], [x2, y2, z2], ...]

        Returns:
            int: Entity handle of the newly created polyline
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_POLYLINE", {
                "points": points
            })
            
            if not response.get("success", False):
                return f"Error drawing polyline: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing polyline: {str(e)}"

    @mcp.tool()
    def draw_rectangle(
        ctx: Context,
        center: List[float],
        width: float,
        height: float
    ) -> int:
        """Draw a rectangle in AutoCAD.

        Args:
            ctx: The MCP context
            center: The center point coordinates [x, y, z]
            width: The width of the rectangle
            height: The height of the rectangle

        Returns:
            int: Entity handle of the newly created rectangle
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_RECTANGLE", {
                "center": center,
                "width": width,
                "height": height
            })
            
            if not response.get("success", False):
                return f"Error drawing rectangle: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing rectangle: {str(e)}"

    @mcp.tool()
    def draw_ellipse(
        ctx: Context,
        center: List[float],
        major_axis: List[float],
        minor_axis: List[float]
    ) -> int:
        """Draw an ellipse in AutoCAD.

        Args:
            ctx: The MCP context
            center: The center point coordinates [x, y, z]
            major_axis: The major axis vector [x, y, z]
            minor_axis: The minor axis vector [x, y, z]

        Returns:
            int: Entity handle of the newly created ellipse
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_ELLIPSE", {
                "center": center,
                "majorAxis": major_axis,
                "minorAxis": minor_axis
            })
            
            if not response.get("success", False):
                return f"Error drawing ellipse: {response.get('error', 'Unknown error')}"
                
            return response.get("message", "Ellipse drawn successfully")
        except Exception as e:
            return f"Error drawing ellipse: {str(e)}"

    @mcp.tool()
    def draw_polygon(
        ctx: Context,
        points: List[List[float]]
    ) -> int:
        """Draw a polygon in AutoCAD.

        Args:
            ctx: The MCP context
            points: List of vertex coordinates [[x1, y1, z1], [x2, y2, z2], ...]

        Returns:
            int: Entity handle of the newly created polygon
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_POLYGON", {
                "points": points
            })
            
            if not response.get("success", False):
                return f"Error drawing polygon: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing polygon: {str(e)}"

    @mcp.tool()
    def draw_polyface(
        ctx: Context,
        points: List[List[float]]
    ) -> int:
        """Draw a polyface mesh in AutoCAD.

        Args:
            ctx: The MCP context
            points: List of vertex coordinates [[x1, y1, z1], [x2, y2, z2], ...]

        Returns:
            int: Entity handle of the newly created polyface
        """
        try:
            autocad = get_autocad_connection()
            response = autocad.send_command("DRAW_POLYFACE", {
                "points": points
            })
            
            if not response.get("success", False):
                return f"Error drawing polyface: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error drawing polyface: {str(e)}"
