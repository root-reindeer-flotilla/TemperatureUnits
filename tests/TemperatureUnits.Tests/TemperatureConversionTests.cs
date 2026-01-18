using System.Globalization;
using TemperatureUnits;
using Xunit;

namespace TemperatureUnits.Tests;

public class TemperatureConversionTests
{
    [Theory]
    [InlineData("0°C", "32°F")]
    [InlineData("100°C", "212°F")]
    [InlineData("-40°C", "-40°F")]
    public void CelsiusToFahrenheit_KeyValues(string input, string expected)
    {
        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal(input, "fahrenheit", 0, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FahrenheitToKelvin_UsesCelsiusIntermediate()
    {
        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal("32°F", "kelvin", 2, CultureInfo.InvariantCulture);

        Assert.Equal("273.15K", result);
    }

    [Fact]
    public void KelvinToFahrenheit_ConvertsThroughCelsius()
    {
        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal("273.15K", "fahrenheit", 1, CultureInfo.InvariantCulture);

        Assert.Equal("32.0°F", result);
    }
}
