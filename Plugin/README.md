# Simple AutoCAD Plugin Example

This is a simple AutoCAD plugin that demonstrates how to create a basic command to draw a circle in AutoCAD.

## Features

- Adds a new command `SIMPLECIRCLE` to AutoCAD
- Allows users to draw a circle by specifying a center point and radius
- Demonstrates basic AutoCAD .NET API usage

## Requirements

- AutoCAD 2024 (or compatible version)
- .NET Framework 4.8
- Visual Studio 2019 or later (for development)

## Installation

1. Build the solution in Visual Studio
2. Copy the compiled DLL (`SimpleAutoCADPlugin.dll`) from the `bin/Debug` or `bin/Release` folder
3. Load the plugin in AutoCAD using one of these methods:
   - Use the `NETLOAD` command in AutoCAD and select the DLL
   - Copy the DLL to one of AutoCAD's trusted locations (see AutoCAD documentation for details)

## Usage

1. Start AutoCAD
2. Load the plugin using the `NETLOAD` command
3. Type `SIMPLECIRCLE` in the command line
4. Follow the prompts to:
   - Select a center point for the circle
   - Specify the radius of the circle

## Development Notes

- The plugin uses AutoCAD's .NET API
- Main functionality is implemented in `SimpleCircleCommand.cs`
- Uses transaction management for safe database operations
- Includes error handling and user feedback

## Troubleshooting

If the plugin doesn't load:
- Ensure the DLL is not blocked by Windows security
- Verify that all required AutoCAD references are available
- Check that the .NET Framework version matches your AutoCAD installation 