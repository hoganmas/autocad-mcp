from typing import Optional, List, Dict, Any
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_text_tools(mcp: FastMCP):
    """Register all text tools with the MCP server."""

    @mcp.tool()
    def create_dimension(
        ctx: Context,
        start_point: List[float],
        end_point: List[float],
        dimension_line_point: List[float],
        text: str = None
    ) -> Dict[str, Any]:
        """Create a dimension. This is a label that is used to measure the distance between two points.
        
        Args:
            ctx: The MCP context
            start_point: The start point of the dimension to be measured
            end_point: The end point of the dimension to be measured
            dimension_line_point: The point on the dimension line. This is where the dimension line will be drawn.
            text: Overwrite the default text of the dimension (optional)

        Returns:
            Dict[str, Any]: A dictionary containing the handle, type, and properties of the created dimension
        """
        try:
            autocad = get_autocad_connection()

            parameters = {
                "startPoint": start_point,
                "endPoint": end_point,
                "dimensionLinePoint": dimension_line_point,
            }

            if text:
                parameters["text"] = text

            response = autocad.send_command("CREATE_DIMENSION", parameters)

            if not response.get("success", False):
                return f"Error creating dimension: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating dimension: {str(e)}"

    @mcp.tool()
    def create_text_label(
        ctx: Context,
        position: List[float],
        height: float,
        rotation: float,
        text: str,
        normal: List[float] = None,
        horizontal_mode: str = None,
    ) -> Dict[str, Any]:
        """Create a text label. This is a label that is used to display text on the screen.
        
        Args:
            ctx: The MCP context
            position: The position of the text label
            height: The height of the text label
            rotation: The rotation of the text label
            text: The text of the text label
            normal: The normal of the plane that the text label is on (optional)
            horizontal_mode: The horizontal mode of the text label (optional). Must be one of "TextLeft", "TextCenter", "TextRight", "TextAlign", "TextMid", or "TextFit".

        Returns:
            Dict[str, Any]: A dictionary containing the handle, type, and properties of the created text label
        """
        try:
            autocad = get_autocad_connection()

            parameters = {
                "position": position,
                "height": height,
                "rotation": rotation,
                "text": text,
            }

            if normal:
                parameters["normal"] = normal

            if horizontal_mode:
                parameters["horizontalMode"] = horizontal_mode.lower()

            response = autocad.send_command("CREATE_TEXT_LABEL", parameters)

            if not response.get("success", False):
                return f"Error creating text label: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error creating text label: {str(e)}"
