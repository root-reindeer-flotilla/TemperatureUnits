# Temperature Units

A client-side mod for Vintage Story that converts all in-game temperature displays from Celsius to Fahrenheit.

## Features

- Converts all temperature displays (weather, body temp, furnaces, etc.) to Fahrenheit
- Toggle between Celsius and Fahrenheit via ConfigLib settings
- Client-side only — no server installation required

## Requirements

- Vintage Story 1.21.0+
- [ConfigLib](https://mods.vintagestory.at/configlib) (optional, for settings GUI)

## Installation

1. Download the latest release from [Mod DB](https://mods.vintagestory.at/show/mod/38965) or [GitHub Releases](https://github.com/root-reindeer-flotilla/TemperatureUnits/releases)
2. Place the `.zip` file in your `VintagestoryData/Mods` folder
3. Launch the game

## Configuration

If ConfigLib is installed, you can toggle Fahrenheit on/off in **Mod Settings**.

Without ConfigLib, Fahrenheit is enabled by default.

## Building from Source

```bash
# Debug build (with logging)
dotnet build -c Debug

# Release build (optimized)
dotnet build -c Release
```

Output zips are created in `bin/Debug/` or `bin/Release/`.

## License

[MIT License](LICENSE)

## Author

ReindeerFlotilla



