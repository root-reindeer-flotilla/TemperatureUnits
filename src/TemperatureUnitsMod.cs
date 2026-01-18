// MIT License - ReindeerFlotilla

using HarmonyLib;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace TemperatureUnits
{
    public class TemperatureUnitsMod : ModSystem
    {
        public const string HarmonyId = "reindeerflotilla.temperatureunits";
        public const string ConfigLibDomain = "temperatureunits";
        public const string SettingTemperatureUnit = "TEMPERATURE_UNIT";
        public const string SettingDecimalPlaces = "DECIMAL_PLACES";

        // Temperature conversion constants
        private const float CelsiusToFahrenheitMultiplier = 1.8f;
        private const float FahrenheitOffset = 32f;
        private const float AbsoluteZeroCelsius = 273.15f;
        private const int MinDecimalPlaces = 0;
        private const int MaxDecimalPlaces = 2;
        private const int DefaultDecimalPlaces = 1;

        public static TemperatureUnitsMod? Instance { get; private set; }
        public static ICoreClientAPI? ClientApi { get; private set; }

        // Settings read dynamically from ConfigLib
        public string TemperatureUnit => GetStringSetting(SettingTemperatureUnit, "fahrenheit").ToLowerInvariant();
        public int DecimalPlaces => int.TryParse(GetStringSetting(SettingDecimalPlaces, "1"), out int dp) ? Math.Clamp(dp, MinDecimalPlaces, MaxDecimalPlaces) : DefaultDecimalPlaces;

        // Regex to match temperature patterns: "25°C", "-10.5°C", "37.8°F", "300K", etc.
        private static readonly Regex TemperatureRegex = new(@"(?<![\d.,])(-?\d+(?:[.,]\d+)?)\s*°?(C|F|K)", RegexOptions.Compiled);
        private CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        private object? _configLibApi;
        private MethodInfo? _getSettingMethod;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            Instance = this;
            ClientApi = api;

            // Set up culture info for number parsing
            CreateCultureInfo(ClientSettings.Language);
            ClientSettings.Inst.String.AddWatcher("language", CreateCultureInfo);

            // Apply Harmony patches
            Harmony harmony = new(HarmonyId);
            
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
#if DEBUG
                api.Logger.Notification("[TemperatureUnits] DEBUG: Harmony patches applied successfully");
#endif
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[TemperatureUnits] Harmony patch error: {ex}");
            }

#if DEBUG
            api.Logger.Notification("[TemperatureUnits] DEBUG: Mod loaded, waiting for ConfigLib...");
#endif
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
            
            if (api.Side != EnumAppSide.Client) return;

            TryInitializeConfigLib(api);
            
#if DEBUG
            api.Logger?.Notification($"[TemperatureUnits] DEBUG: Settings finalized. Initial values - TemperatureUnit: {TemperatureUnit}, DecimalPlaces: {DecimalPlaces}");
#endif
        }

        private void TryInitializeConfigLib(ICoreAPI api)
        {
            try
            {
                _configLibApi = api.ModLoader.GetModSystem("ConfigLib.ConfigLibModSystem");
                
                if (_configLibApi != null)
                {
                    _getSettingMethod = _configLibApi.GetType().GetMethod("GetSetting", new Type[] { typeof(string), typeof(string) });
                    
                    if (_getSettingMethod == null)
                    {
                        api.Logger?.Debug("[TemperatureUnits] ConfigLib found but GetSetting method signature not found");
                        _configLibApi = null;
                    }
#if DEBUG
                    else
                    {
                        api.Logger?.Notification("[TemperatureUnits] DEBUG: ConfigLib found, settings will be read dynamically");
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                api.Logger?.Debug($"[TemperatureUnits] ConfigLib not available: {ex.Message}");
#if DEBUG
                api.Logger?.Debug($"[TemperatureUnits] ConfigLib initialization exception: {ex}");
#endif
            }
        }

        private bool GetBoolSetting(string settingCode, bool defaultValue)
        {
            if (_configLibApi == null || _getSettingMethod == null)
                return defaultValue;

            try
            {
                var setting = _getSettingMethod.Invoke(_configLibApi, new object[] { ConfigLibDomain, settingCode });
                if (setting == null)
                    return defaultValue;

                var valueProp = setting.GetType().GetProperty("Value");
                if (valueProp == null)
                {
                    ClientApi?.Logger?.Debug($"[TemperatureUnits] ConfigLib setting '{settingCode}' has no Value property");
                    return defaultValue;
                }

                var value = valueProp.GetValue(setting);
                if (value == null)
                    return defaultValue;

                var asBoolMethod = value.GetType().GetMethod("AsBool", new Type[] { typeof(bool) });
                if (asBoolMethod == null)
                {
                    ClientApi?.Logger?.Debug($"[TemperatureUnits] ConfigLib value for '{settingCode}' has no AsBool method");
                    return defaultValue;
                }

                var result = asBoolMethod.Invoke(value, new object[] { defaultValue });
                return result != null ? (bool)result : defaultValue;
            }
            catch (Exception ex)
            {
                ClientApi?.Logger?.Debug($"[TemperatureUnits] Error reading bool setting '{settingCode}': {ex.Message}");
#if DEBUG
                ClientApi?.Logger?.Debug($"[TemperatureUnits] Exception details: {ex}");
#endif
            }
            
            return defaultValue;
        }

        private string GetStringSetting(string settingCode, string defaultValue)
        {
            if (_configLibApi == null || _getSettingMethod == null)
                return defaultValue;

            try
            {
                var setting = _getSettingMethod.Invoke(_configLibApi, new object[] { ConfigLibDomain, settingCode });
                if (setting == null)
                    return defaultValue;

                var valueProp = setting.GetType().GetProperty("Value");
                if (valueProp == null)
                {
                    ClientApi?.Logger?.Debug($"[TemperatureUnits] ConfigLib setting '{settingCode}' has no Value property");
                    return defaultValue;
                }

                var value = valueProp.GetValue(setting);
                if (value == null)
                    return defaultValue;

                var asStringMethod = value.GetType().GetMethod("AsString", new Type[] { typeof(string) });
                if (asStringMethod == null)
                {
                    ClientApi?.Logger?.Debug($"[TemperatureUnits] ConfigLib value for '{settingCode}' has no AsString method");
                    return defaultValue;
                }

                var result = asStringMethod.Invoke(value, new object[] { defaultValue });
                return result != null ? (string)result : defaultValue;
            }
            catch (Exception ex)
            {
                ClientApi?.Logger?.Debug($"[TemperatureUnits] Error reading string setting '{settingCode}': {ex.Message}");
#if DEBUG
                ClientApi?.Logger?.Debug($"[TemperatureUnits] Exception details: {ex}");
#endif
            }
            
            return defaultValue;
        }

        public override void Dispose()
        {
            new Harmony(HarmonyId).UnpatchAll(HarmonyId);
            ClientSettings.Inst.String.RemoveWatcher("language", CreateCultureInfo);
            Instance = null;
            ClientApi = null;
            _configLibApi = null;
            _getSettingMethod = null;
            base.Dispose();
        }

        private void CreateCultureInfo(string language)
        {
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(language);
            }
            catch (CultureNotFoundException)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
        }

        internal static string ReplaceTemperaturesInternal(string? text, string targetUnit, int decimalPlaces, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? "";
            }

            string normalizedTargetUnit = string.IsNullOrWhiteSpace(targetUnit) ? "celsius" : targetUnit.ToLowerInvariant();
            if (normalizedTargetUnit == "celsius")
            {
                return text;
            }

            int clampedDecimalPlaces = Math.Clamp(decimalPlaces, MinDecimalPlaces, MaxDecimalPlaces);
            CultureInfo cultureInfoToUse = culture ?? CultureInfo.InvariantCulture;

            return TemperatureRegex.Replace(text, match =>
            {
                string sourceUnit = match.Groups[2].Value.ToUpperInvariant();

                // Skip if already in target unit
                if ((normalizedTargetUnit == "fahrenheit" && sourceUnit == "F") ||
                    (normalizedTargetUnit == "kelvin" && sourceUnit == "K"))
                {
                    return match.Value;
                }

                string numberStr = match.Groups[1].Value.Replace(',', '.');
                if (!float.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float sourceTemp))
                {
                    return match.Value;
                }

                // First convert source to Celsius if needed
                float celsius = sourceUnit switch
                {
                    "C" => sourceTemp,
                    "F" => (sourceTemp - FahrenheitOffset) * 5f / 9f,
                    "K" => sourceTemp - AbsoluteZeroCelsius,
                    _ => sourceTemp
                };

                // Then convert Celsius to target unit
                float targetTemp = normalizedTargetUnit switch
                {
                    "fahrenheit" => celsius * CelsiusToFahrenheitMultiplier + FahrenheitOffset,
                    "kelvin" => celsius + AbsoluteZeroCelsius,
                    _ => celsius
                };

                string unitSymbol = normalizedTargetUnit switch
                {
                    "fahrenheit" => "°F",
                    "kelvin" => "K",
                    _ => "°C"
                };

                // Format based on configured decimal places
                string format = clampedDecimalPlaces switch
                {
                    MinDecimalPlaces => "0",
                    1 => "0.0",
                    MaxDecimalPlaces => "0.00",
                    _ => "0.0"
                };

                string formatted = targetTemp.ToString(format, cultureInfoToUse);

                return formatted + unitSymbol;
            });
        }

        /// <summary>
        /// Replaces temperatures in text with the configured unit.
        /// Conversion formulas:
        /// - C to F: F = C × 9/5 + 32
        /// - C to K: K = C + 273.15
        /// </summary>
        public static string ReplaceTemperatures(string? text)
        {
            if (Instance == null || string.IsNullOrEmpty(text))
            {
                return text ?? "";
            }

            return ReplaceTemperaturesInternal(text, Instance.TemperatureUnit, Instance.DecimalPlaces, Instance.cultureInfo);
        }
    }

    // ==================== HARMONY PATCHES ====================

    /// <summary>
    /// Patch Lang.Get to convert temperatures in localized strings
    /// </summary>
    [HarmonyPatch(typeof(Lang))]
    public static class LangPatch
    {
        [HarmonyPatch(nameof(Lang.Get))]
        [HarmonyPatch(new Type[] { typeof(string), typeof(object[]) })]
        [HarmonyPostfix]
        public static void GetPostfix(ref string __result)
        {
            __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
        }
    }

    /// <summary>
    /// Patch string.Format to catch formatted temperature strings
    /// </summary>
    [HarmonyPatch(typeof(string))]
    public static class StringFormatPatch
    {
        [HarmonyPatch(nameof(string.Format), new Type[] { typeof(string), typeof(object) })]
        [HarmonyPostfix]
        public static void FormatPostfix1(ref string __result)
        {
            if (__result != null && __result.Contains("°"))
            {
                __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
            }
        }

        [HarmonyPatch(nameof(string.Format), new Type[] { typeof(string), typeof(object), typeof(object) })]
        [HarmonyPostfix]
        public static void FormatPostfix2(ref string __result)
        {
            if (__result != null && __result.Contains("°"))
            {
                __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
            }
        }

        [HarmonyPatch(nameof(string.Format), new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) })]
        [HarmonyPostfix]
        public static void FormatPostfix3(ref string __result)
        {
            if (__result != null && __result.Contains("°"))
            {
                __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
            }
        }

        [HarmonyPatch(nameof(string.Format), new Type[] { typeof(string), typeof(object[]) })]
        [HarmonyPostfix]
        public static void FormatPostfixArray(ref string __result)
        {
            if (__result != null && __result.Contains("°"))
            {
                __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
            }
        }
    }

    /// <summary>
    /// Patch TextDrawUtil.DrawTextLine as a fallback
    /// </summary>
    [HarmonyPatch(typeof(TextDrawUtil), nameof(TextDrawUtil.DrawTextLine))]
    public static class TextDrawUtilPatch
    {
        public static void Prefix(ref string text)
        {
            if (text != null && text.Contains("°"))
            {
                text = TemperatureUnitsMod.ReplaceTemperatures(text);
            }
        }
    }

#if DEBUG
    /// <summary>
    /// DEBUG ONLY: Patch StringBuilder.ToString to catch dynamically built strings
    /// This is removed in Release builds for performance
    /// </summary>
    [HarmonyPatch(typeof(System.Text.StringBuilder))]
    public static class StringBuilderPatch
    {
        [HarmonyPatch(nameof(System.Text.StringBuilder.ToString), new Type[] { })]
        [HarmonyPostfix]
        public static void ToStringPostfix(System.Text.StringBuilder __instance, ref string __result)
        {
            if (__result != null && __result.Contains("°"))
            {
                __result = TemperatureUnitsMod.ReplaceTemperatures(__result);
            }
        }
    }
#endif
}
