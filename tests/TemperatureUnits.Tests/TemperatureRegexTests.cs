using System.Globalization;
using TemperatureUnits;
using Xunit;

namespace TemperatureUnits.Tests;

public class TemperatureRegexTests
{
    [Fact]
    public void ConvertsMultipleTemperaturesInString()
    {
        string input = "Range: -10°C to 5°C";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "kelvin", 2, CultureInfo.InvariantCulture);

        Assert.Equal("Range: 263.15K to 278.15K", result);
    }

    [Fact]
    public void SkipsAlreadyTargetUnit()
    {
        string input = "Already 32°F";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "fahrenheit", 0, CultureInfo.InvariantCulture);

        Assert.Equal(input, result);
    }

    [Fact]
    public void SkipsAlreadyKelvinTargetUnit()
    {
        string input = "Already 273.15K";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "kelvin", 2, CultureInfo.InvariantCulture);

        Assert.Equal(input, result);
    }

    [Fact]
    public void CelsiusTargetShortCircuits()
    {
        string input = "It is 10°C outside";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "celsius", 1, CultureInfo.InvariantCulture);

        Assert.Equal(input, result);
    }

    [Fact]
    public void IgnoresMissingUnits()
    {
        string input = "No temp here 123";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "kelvin", 1, CultureInfo.InvariantCulture);

        Assert.Equal(input, result);
    }

    [Fact]
    public void IgnoresInvalidNumbers()
    {
        string input = "Invalid 12..3°C";

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "fahrenheit", 1, CultureInfo.InvariantCulture);

        Assert.Equal(input, result);
    }

    [Fact]
    public void NullInputReturnsEmptyString()
    {
        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(null, "fahrenheit", 1, CultureInfo.InvariantCulture);

        Assert.Equal(string.Empty, result);
    }
}
