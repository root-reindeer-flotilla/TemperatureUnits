using System.Globalization;
using TemperatureUnits;
using Xunit;

namespace TemperatureUnits.Tests;

public class TemperatureFormattingTests
{
    [Theory]
    [InlineData(-2, "32°F")]
    [InlineData(5, "32.00°F")]
    public void DecimalPlacesClamp_ToSupportedRange(int decimalPlaces, string expected)
    {
        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal("0°C", "fahrenheit", decimalPlaces, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatsWithCultureSpecificDecimalSeparator()
    {
        CultureInfo culture = CultureInfo.CreateSpecificCulture("fr-FR");

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal("10°C", "fahrenheit", 1, culture);

        Assert.Equal("50,0°F", result);
    }

    [Fact]
    public void ParsesCommaDecimalInput()
    {
        CultureInfo culture = CultureInfo.CreateSpecificCulture("fr-FR");

        string result = TemperatureUnitsMod.ReplaceTemperaturesInternal("10,5°C", "fahrenheit", 1, culture);

        Assert.Equal("50,9°F", result);
    }
}
