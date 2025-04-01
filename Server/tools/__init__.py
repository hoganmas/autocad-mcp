from .shape_tools import register_shape_tools
from .workspace_tools import register_workspace_tools
from .entity_tools import register_entity_tools
from .shape3d_tools import register_shape3d_tools

def register_all_tools(mcp):
    """Register all tools with the MCP server."""
    register_shape_tools(mcp)
    register_workspace_tools(mcp)
    register_entity_tools(mcp)
    register_shape3d_tools(mcp)
