from .shape_tools import register_shape_tools

def register_all_tools(mcp):
    """Register all tools with the MCP server."""
    register_shape_tools(mcp)
