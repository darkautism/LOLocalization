using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

namespace LOLocalization
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LOLocalization : BasePlugin
    {
        public static LOLocalization Inst;
        internal new static ManualLogSource Log;
        public static int langindex = 4;
        public static string langtext = "tc";

        public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> communityLocalizationPatch = null;
        public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> communityTable_LocalizationPatch = null;
        public static System.Collections.Generic.Dictionary<string, Font> fonts = null;

        private static ConfigEntry<string> configLanguage;
        private static ConfigEntry<string> configFont;
        

        public override void Load()
        {
            Inst = this;
            Log = base.Log;
            communityLocalizationPatch = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
            communityLocalizationPatch.Add("tc",LoadLocalization("localization_tc"));
            communityLocalizationPatch.Add("en", LoadLocalization("localization_en"));
            communityTable_LocalizationPatch = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
            communityTable_LocalizationPatch.Add("en", LoadLocalization("table_localization_en"));
            communityTable_LocalizationPatch.Add("tc", LoadLocalization("table_localization_tc"));

            configLanguage = Config.Bind("General","language","tc","Accept tc,en");
            configFont = Config.Bind("General", "font", "sourcehansans", "Accept sourcehansans,msjhbd");
            if (configLanguage.Value == "en")
            {
                langindex = 3;
                langtext = "en";
            }
            else
            {
                langindex = 4;
                langtext = "tc";
            }

            Harmony.CreateAndPatchAll(typeof(LOLocalization));
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        System.Collections.Generic.Dictionary<string, string> LoadLocalization(string lang) {
            System.Collections.Generic.Dictionary<string, string>  ret = new System.Collections.Generic.Dictionary<string, string>();
            string path = $"{Paths.PluginPath}/Localization/{lang}.csv";
            if (File.Exists(path))
            {
                try
                {
                    ret = new System.Collections.Generic.Dictionary<string, string>();
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false,
                    };
                    using (var reader = new StreamReader(path))
                    using (var csv = new CsvReader(reader, config))
                    {
                        while (csv.Read())
                        {
                            string key = csv.GetField(0);
                            string value = csv.GetField(1);
                            //Log.LogInfo($"'{key} {value}'");
                            ret[key] = value;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.LogError(e.Message);
                    Log.LogError($"The {path} is wrong format.");
                }
            }
            return ret;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ResourceManager), "LoadFont")]
        public static void LoadFontPatch(ref Font __result, string fontName)
        {
            if (fonts == null)
            {
                fonts = new System.Collections.Generic.Dictionary<string, Font>();
                string path = $"{Paths.PluginPath}/Localization/fonts";
                if (System.IO.File.Exists(path))
                {
                    var ab = AssetBundle.LoadFromFile(path);
                    string[] a = ab.GetAllAssetNames().ToArray();
                    Log.LogInfo(a.Join(null,","));
                    foreach (string name in a) {
                        string newname = Path.GetFileName(name).Split('.')[0];
                        fonts.Add(newname, ab.LoadAsset<Font>(newname));
                    }
                    ab.Unload(false);
                }
            }
            Font replaceFont;
            if (fonts.TryGetValue(configFont.Value, out replaceFont))
            {
                __result = replaceFont;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.LoadTextAsset))]
        static void LoadTextAssetPatch(TextAsset __result, ref string name, string bundleName = "")
        {
            if (name == "Table_Localization_ja")
                name = "Table_Localization_" + langtext;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Localization), nameof(Localization.AddCSV))]
        static void AddCSVPatch(ref BetterList<string> newValues, Il2CppStringArray newLanguages, Dictionary<string, int> languageIndices)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < newValues.size; i++)
                sb.Append(newValues[i]);
            if (newLanguages != null)
            {
                sb.AppendLine();
                for (int i = 0; i < newLanguages.Length; i++)
                    sb.Append(newLanguages[i]);
            }
            Log.LogDebug($"AddCSV({ newValues.size}): {sb}");
            if (newValues.size >= 6 && newValues[langindex] != null && newValues[langindex].Trim().Length != 0)
            {
                newValues[1] = newValues[langindex];
                newValues[2] = newValues[langindex];
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(Localization), nameof(Localization.Get))]
        static bool GetPatch(ref string __result, string key, bool warnIfMissing = true)
        {
            if (communityLocalizationPatch.ContainsKey(configLanguage.Value) &&
                communityLocalizationPatch[configLanguage.Value].ContainsKey(key)) {
                __result = communityLocalizationPatch[configLanguage.Value][key];
                return false;
            }
            return true;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(DataManager), nameof(DataManager.GetLocalizationTable))]
        static bool GetLocalizationPatch(ref string __result, string key)
        {
            if (communityTable_LocalizationPatch.ContainsKey(configLanguage.Value) &&
                communityTable_LocalizationPatch[configLanguage.Value].ContainsKey(key))
            {
                __result = communityTable_LocalizationPatch[configLanguage.Value][key].Replace("&n", "\n");
                return false;
            }
            return true;
        }
    }
}
