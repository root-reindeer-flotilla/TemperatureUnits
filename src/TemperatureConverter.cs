using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TemperatureUnits
{
    public static class TemperatureConverter
    {
        // Regex to match temperature patterns: "25°C", "-10.5°C", "37.8°F", "300K", etc.
        private static readonly Regex TemperatureRegex = new(@"(-?\d+(?:[.,]\d+)?)\s*°?(C|F|K)", RegexOptions.Compiled);

        /// <summary>
        /// Replaces temperatures in text with the configured unit.
        /// Conversion formulas:
        /// - C to F: F = C × 9/5 + 32
        /// - C to K: K = C + 273.15
        /// </summary>
        public static string ReplaceTemperatures(string? text, string targetUnit, int decimalPlaces, CultureInfo? cultureInfo = null)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? "";

            // If set to Celsius (game default), no conversion needed
            // Note: logic assumes the original game uses Celsius, which seems to be the case as the regex matches C, F, K but treats them as source
            // Wait, if targetUnit is celsius, the original code returned early.
            if (targetUnit == "celsius")
                return text;

            cultureInfo ??= CultureInfo.InvariantCulture;

            return TemperatureRegex.Replace(text, match =>
            {
                string sourceUnit = match.Groups[2].Value.ToUpperInvariant();

                // Skip if already in target unit
                if ((targetUnit == "fahrenheit" && sourceUnit == "F") ||
                    (targetUnit == "kelvin" && sourceUnit == "K"))
                    return match.Value;

                string numberStr = match.Groups[1].Value.Replace(',', '.');
                if (!float.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float sourceTemp))
                {
                    return match.Value;
                }

                // First convert source to Celsius if needed
                float celsius = sourceUnit switch
                {
                    "C" => sourceTemp,
                    "F" => (sourceTemp - 32f) * 5f / 9f,
                    "K" => sourceTemp - 273.15f,
                    _ => sourceTemp
                };

                // Then convert Celsius to target unit
                float targetTemp = targetUnit switch
                {
                    "fahrenheit" => celsius * 1.8f + 32f,
                    "kelvin" => celsius + 273.15f,
                    _ => celsius
                };

                string unitSymbol = targetUnit switch
                {
                    "fahrenheit" => "°F",
                    "kelvin" => "K",
                    _ => "°C"
                };

                // Format based on configured decimal places
                string format = decimalPlaces switch
                {
                    0 => "0",
                    1 => "0.0",
                    2 => "0.00",
                    _ => "0.0"
                };

                string formatted = targetTemp.ToString(format, cultureInfo);

                return formatted + unitSymbol;
            });
        }
    }
}
