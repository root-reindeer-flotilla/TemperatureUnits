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
        public const string SettingUseFahrenheit = "USE_FAHRENHEIT";
        public const string SettingDecimalPlaces = "DECIMAL_PLACES";

        public static TemperatureUnitsMod? Instance { get; private set; }
        public static ICoreClientAPI? ClientApi { get; private set; }

        // Settings read dynamically from ConfigLib
        public bool UseFahrenheit => GetBoolSetting(SettingUseFahrenheit, true);
        public int DecimalPlaces => Math.Clamp(GetIntSetting(SettingDecimalPlaces, 1), 0, 2);

        // Regex to match temperature patterns: "25°C", "-10.5°C", "37.8°F", etc.
        private static readonly Regex TemperatureRegex = new(@"(-?\d+(?:[.,]\d+)?)\s*°(C|F)", RegexOptions.Compiled);
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
            Mod.Logger.Notification($"[TemperatureUnits] DEBUG: Settings finalized. Initial values - UseFahrenheit: {UseFahrenheit}, DecimalPlaces: {DecimalPlaces}");
#endif
        }

        private void TryInitializeConfigLib(ICoreAPI api)
        {
            try
            {
                _configLibApi = api.ModLoader.GetModSystem("ConfigLib.ConfigLibModSystem");
                
                if (_configLibApi != null)
                {
                    _getSettingMethod = _configLibApi.GetType().GetMethod("GetSetting");
#if DEBUG
                    api.Logger.Notification($"[TemperatureUnits] DEBUG: ConfigLib found, settings will be read dynamically");
#endif
                }
            }
            catch
#if DEBUG
                (Exception ex)
#endif
            {
#if DEBUG
                api.Logger.Debug($"[TemperatureUnits] ConfigLib not available: {ex.Message}");
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
                if (setting != null)
                {
                    var valueProp = setting.GetType().GetProperty("Value");
                    var value = valueProp?.GetValue(setting);
                    
                    if (value != null)
                    {
                        var asBoolMethod = value.GetType().GetMethod("AsBool", new Type[] { typeof(bool) });
                        if (asBoolMethod != null)
                        {
                            return (bool)asBoolMethod.Invoke(value, new object[] { defaultValue })!;
                        }
                    }
                }
            }
            catch { }
            
            return defaultValue;
        }

        private int GetIntSetting(string settingCode, int defaultValue)
        {
            if (_configLibApi == null || _getSettingMethod == null)
                return defaultValue;

            try
            {
                var setting = _getSettingMethod.Invoke(_configLibApi, new object[] { ConfigLibDomain, settingCode });
                if (setting != null)
                {
                    var valueProp = setting.GetType().GetProperty("Value");
                    var value = valueProp?.GetValue(setting);
                    
                    if (value != null)
                    {
                        var asIntMethod = value.GetType().GetMethod("AsInt", new Type[] { typeof(int) });
                        if (asIntMethod != null)
                        {
                            return (int)asIntMethod.Invoke(value, new object[] { defaultValue })!;
                        }
                    }
                }
            }
            catch { }
            
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

        /// <summary>
        /// Replaces Celsius temperatures in text with Fahrenheit equivalents.
        /// Uses NIST formula: F = C × 9/5 + 32
        /// </summary>
        public static string ReplaceTemperatures(string? text)
        {
            if (Instance == null || !Instance.UseFahrenheit || string.IsNullOrEmpty(text))
                return text ?? "";

            return TemperatureRegex.Replace(text, match =>
            {
                string unit = match.Groups[2].Value;
                
                if (unit == "F")
                    return match.Value;

                string numberStr = match.Groups[1].Value.Replace(',', '.');
                if (!float.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float celsius))
                {
                    return match.Value;
                }

                float fahrenheit = celsius * 1.8f + 32f;

                // Format based on configured decimal places
                string format = Instance.DecimalPlaces switch
                {
                    0 => "0",
                    1 => "0.#",
                    2 => "0.##",
                    _ => "0.#"
                };
                
                string formatted = fahrenheit.ToString(format, Instance.cultureInfo);

                return formatted + "°F";
            });
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
