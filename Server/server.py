from mcp.server.fastmcp import FastMCP, Context, Image
import logging
import textwrap
from dataclasses import dataclass
from contextlib import asynccontextmanager
from typing import AsyncIterator, Dict, Any, List
from config import config
from tools import register_all_tools
from autocad_connection import get_autocad_connection, AutoCADConnection

# Configure logging using settings from config
logging.basicConfig(
    level=getattr(logging, config.log_level),
    format=config.log_format
)
logger = logging.getLogger("AutoCADMCP")

# Global connection state
_autocad_connection: AutoCADConnection = None

@asynccontextmanager
async def server_lifespan(server: FastMCP) -> AsyncIterator[Dict[str, Any]]:
    """Handle server startup and shutdown."""
    global _autocad_connection
    logger.info("AutoCADMCP server starting up")
    try:
        _autocad_connection = get_autocad_connection()
        logger.info("Connected to AutoCAD on startup")
    except Exception as e:
        logger.warning(f"Could not connect to AutoCAD on startup: {str(e)}")
        _autocad_connection = None
    try:
        yield {}
    finally:
        if _autocad_connection:
            _autocad_connection.disconnect()
            _autocad_connection = None
        logger.info("AutoCADMCP server shut down")

# Initialize MCP server
mcp = FastMCP(
    "AutoCADMCP",
    description="AutoCAD Editor integration via Model Context Protocol",
    lifespan=server_lifespan
)

# Register all tools
register_all_tools(mcp)

# TODO: Add mcp prompt
@mcp.prompt("autocad")
async def autocad_prompt(prompt: str) -> str:
    """Prompt for AutoCAD."""
    return textwrap.dedent("""
        ## Best Practices

        1. Always store entity handles returned from creation operations
        2. Check operation results for success/failure
        3. Use appropriate error handling for failed operations
        4. Clean up unused entities to maintain drawing efficiency
        5. Group related operations for better organization
        6. Use appropriate coordinate systems for your operations
        7. By default, x axis is to the right and y axis is up, and z axis is out of the screen. 2D entities are parallel to the xy plane.
        8. Angles are by default in radians.

        ## Error Handling

        Most operations return either:
        - An entity handle (for creation operations)
        - A dictionary containing success/failure status and properties
        - An error message if the operation fails
    """)

# Run the server
if __name__ == "__main__":
    mcp.run(transport='stdio')