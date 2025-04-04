from .workspace_tools import register_workspace_tools
from .entity_tools import register_entity_tools
from .curve_creation_tools import register_curve_creation_tools
from .solid_creation_tools import register_solid_creation_tools
from .editing_tools import register_editing_tools
from .curve_editing_tools import register_curve_editing_tools

def register_all_tools(mcp):
    """Register all tools with the MCP server."""
    register_workspace_tools(mcp)
    register_entity_tools(mcp)
    register_curve_creation_tools(mcp)
    register_solid_creation_tools(mcp)
    register_editing_tools(mcp)
    register_curve_editing_tools(mcp)
