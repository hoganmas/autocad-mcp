from typing import Optional, List, Dict, Any
from mcp.server.fastmcp import FastMCP, Context
from autocad_connection import get_autocad_connection

def register_view_tools(mcp: FastMCP):
    """Register all view tools with the MCP server."""

    @mcp.tool()
    def capture_view(
        ctx: Context,
        target: List[float],
        view_height: float,
        view_direction: List[float],
        perspective_enabled: bool,
        lens_length: float = None,
    ) -> Dict[str, Any]:
        """Capture a view of the current viewport.

        Args:
            ctx: The MCP context
            target: The target point to capture the view from (x, y, z). This point will be the center of the view.
            view_height: The height of the view. This changes the view width to maintain the aspect ratio. A larger value means more of the scene will be captured, effectively zooming out.
            view_direction: The direction the view is facing (x, y, z)
            perspective_enabled: Whether to use a perspective camera
            lens_length: The lens length of the camera if perspective_enabled is true (optional, but required if perspective_enabled is true)

        Returns:
            Dict[str, Any]: A dictionary containing the handle, type, and properties of the created view
        """
        try:
            autocad = get_autocad_connection()

            parameters = {
                "target": target,
                "viewHeight": view_height,
                "viewDirection": view_direction,
                "perspectiveEnabled": perspective_enabled,
            }

            if perspective_enabled:
                parameters["lensLength"] = lens_length

            response = autocad.send_command("CAPTURE_VIEW", parameters)

            if not response.get("success", False):
                return f"Error capturing view: {response.get('error', 'Unknown error')}"
                
            return response.get("result")
        except Exception as e:
            return f"Error capturing view: {str(e)}"
