from mcp.server.fastmcp import FastMCP, Context, Image
import logging
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

# Run the server
if __name__ == "__main__":
    mcp.run(transport='stdio')