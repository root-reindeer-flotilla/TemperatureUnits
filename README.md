# Temperature Units

A client-side mod for Vintage Story that converts all in-game temperature displays to Fahrenheit or Kelvin.

## Features

- Converts all temperature displays (weather, body temp, furnaces, etc.) to your preferred unit
- Choose between Celsius, Fahrenheit, or Kelvin
- Configurable decimal places (0, 1, or 2) — default is 1
- Client-side only — no server installation required

## Requirements

- Vintage Story 1.21.0+
- [ConfigLib](https://mods.vintagestory.at/configlib) (optional, for settings GUI)

## Installation

1. Download the latest release from [Mod DB](https://mods.vintagestory.at/show/mod/38965) or [GitHub Releases](https://github.com/root-reindeer-flotilla/TemperatureUnits/releases)
2. Place the `.zip` file in your `VintagestoryData/Mods` folder
3. Launch the game

## Configuration

If ConfigLib is installed, you can select your temperature unit and decimal places in **Mod Settings**.

Without ConfigLib, Fahrenheit with 1 decimal place is used by default.

## Building from Source

```bash
# Debug build (with logging)
dotnet build -c Debug

# Release build (optimized)
dotnet build -c Release
```

Output zips are created in `bin/Debug/` or `bin/Release/`.

## Tests

The test project references Vintage Story assemblies. Make sure `VINTAGESTORY_PATH` points to your install.

```bash
export VINTAGESTORY_PATH=/path/to/Vintagestory
dotnet test tests/TemperatureUnits.Tests/TemperatureUnits.Tests.csproj
```

## License

[MIT License](LICENSE)

## Author

ReindeerFlotilla
