# AutoCAD MCP (Model Control Protocol)

AutoCAD MCP is a protocol implementation that allows programmatic control of AutoCAD through a standardized interface. This tool enables automation of drawing creation, modification, and property management in AutoCAD.

## Installation

1. Ensure you have AutoCAD installed on your system
2. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/autocad-mcp.git
   cd autocad-mcp
   ```
3. Run the MCP server
    ```bash
    cd Server
    uv run server.py
    ```
4. Build the AutoCAD plugin.
    ```bash
    cd Plugin
    donet build
    ```
5. Open AutoCAD and run the NETLOAD command. Select the plugin located at:
    autocad-mcp/Plugin/bin/Debug/net47/AutoCADMCP.dll 
6. In AutoCAD run the STARTMCP command to start the MCP Bridge.

## Project Structure

- `Server/`: Contains the MCP server implementation
- `Plugin/`: Contains the AutoCAD plugin implementation

## License

This project is licensed under the MIT License - see the LICENSE file for details.
