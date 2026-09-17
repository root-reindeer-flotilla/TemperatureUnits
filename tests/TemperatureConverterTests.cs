using System.Globalization;
using Xunit;

namespace TemperatureUnits.Tests
{
    public class TemperatureConverterTests
    {
        [Fact]
        public void ReplaceTemperatures_ReturnsEmptyString_WhenTextIsNull()
        {
            var result = TemperatureConverter.ReplaceTemperatures(null, "fahrenheit", 1);
            Assert.Equal("", result);
        }

        [Fact]
        public void ReplaceTemperatures_ReturnsEmpty_WhenTextIsEmpty()
        {
            var result = TemperatureConverter.ReplaceTemperatures("", "fahrenheit", 1);
            Assert.Equal("", result);
        }

        [Fact]
        public void ReplaceTemperatures_ReturnsOriginal_WhenTargetIsCelsius()
        {
            var text = "It is 25°C outside.";
            var result = TemperatureConverter.ReplaceTemperatures(text, "celsius", 1);
            Assert.Equal(text, result);
        }

        [Theory]
        [InlineData("Water boils at 100°C.", "fahrenheit", 1, "Water boils at 212.0°F.")]
        [InlineData("Freezing is 0°C.", "fahrenheit", 1, "Freezing is 32.0°F.")]
        [InlineData("Cold day -10°C.", "fahrenheit", 1, "Cold day 14.0°F.")]
        [InlineData("Body temp 37°C.", "fahrenheit", 1, "Body temp 98.6°F.")]
        public void ReplaceTemperatures_ConvertsCelsiusToFahrenheit(string input, string targetUnit, int decimals, string expected)
        {
            var result = TemperatureConverter.ReplaceTemperatures(input, targetUnit, decimals);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Absolute zero -273.15°C.", "kelvin", 2, "Absolute zero 0.00K.")] // -273.15 + 273.15 = 0
        [InlineData("Water boils at 100°C.", "kelvin", 0, "Water boils at 373K.")]
        public void ReplaceTemperatures_ConvertsCelsiusToKelvin(string input, string targetUnit, int decimals, string expected)
        {
            var result = TemperatureConverter.ReplaceTemperatures(input, targetUnit, decimals);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ReplaceTemperatures_HandlesCommaInInput()
        {
            var input = "It is 20,5°C.";
            // 20.5 C -> F: 20.5 * 1.8 + 32 = 36.9 + 32 = 68.9
            var expected = "It is 68.9°F.";
            var result = TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 1);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ReplaceTemperatures_IgnoresExistingTargetUnit()
        {
            var input = "It is already 100°F.";
            var result = TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 1);
            Assert.Equal(input, result);
        }

        [Fact]
        public void ReplaceTemperatures_ConvertsKelvinToFahrenheit()
        {
            // 300K -> C: 300 - 273.15 = 26.85
            // C -> F: 26.85 * 1.8 + 32 = 48.33 + 32 = 80.33
            var input = "Temp is 300K.";
            var result = TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 2);
            // 80.33
            Assert.Equal("Temp is 80.33°F.", result);
        }

        [Fact]
        public void ReplaceTemperatures_UsesProvidedCultureForOutput()
        {
            var culture = new CultureInfo("de-DE"); // Uses comma for decimal
            var input = "25°C"; // 77 F
            var result = TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 1, culture);
            Assert.Equal("77,0°F", result);
        }

        [Fact]
        public void ReplaceTemperatures_RespectsDecimalPlaces()
        {
            var input = "37°C"; // 98.6 F

            Assert.Equal("99°F", TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 0));
            Assert.Equal("98.6°F", TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 1));
            Assert.Equal("98.60°F", TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 2));
        }

        [Fact]
        public void ReplaceTemperatures_HandlesMultipleMatches()
        {
            var input = "Low 10°C, High 20°C.";
            // 10 C -> 50 F
            // 20 C -> 68 F
            var expected = "Low 50.0°F, High 68.0°F.";
            var result = TemperatureConverter.ReplaceTemperatures(input, "fahrenheit", 1);
            Assert.Equal(expected, result);
        }
    }
}
