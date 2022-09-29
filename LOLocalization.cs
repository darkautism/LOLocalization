using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
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

        public static Font replaceFont = null;
        public override void Load()
        {
            Inst = this;
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(LOLocalization));
        }


        [HarmonyPostfix, HarmonyPatch(typeof(ResourceManager), "LoadFont")]
        public static void LoadFontPatch(ref Font __result, string fontName)
        {
            if (replaceFont == null)
            {
                string path = $"{Paths.PluginPath}/Localization/font";
                var ab = AssetBundle.LoadFromFile(path);
                replaceFont = ab.LoadAsset<Font>("msjhbd");
                ab.Unload(false);
            }
            __result = replaceFont;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.LoadTextAsset))]
        static void LoadTextAssetPatch(TextAsset __result, ref string name, string bundleName = "")
        {
            Log.LogInfo($"LoadTextAsset: {name} {bundleName}");
            if (name == "Table_Localization_ja")
                name = "Table_Localization_" + LOLocalization.langtext;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Localization), nameof(Localization.AddCSV))]
        static void AddCSVPatch(ref BetterList<string> newValues, Il2CppStringArray newLanguages, Dictionary<string, int> languageIndices)
        {
            int langindex = 3;
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
            if (newValues.size >= 6 && newValues[LOLocalization.langindex] != null && newValues[LOLocalization.langindex].Trim().Length != 0)
            {
                newValues[1] = newValues[LOLocalization.langindex];
                newValues[2] = newValues[LOLocalization.langindex];
            }
        }
    }
}
