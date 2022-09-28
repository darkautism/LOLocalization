using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace LOLocalizationTC
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal new static ManualLogSource Log;
        public static int langindex = 4;
        public static string langtext = "tc";
        public override void Load()
        {
            Log = base.Log;
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.LoadTextAsset))]
    public static class LoadTextAssetPatch
    {
        static void Prefix(TextAsset __result, ref string name, string bundleName = "")
        {
            Plugin.Log.LogInfo($"LoadTextAsset: {name} {bundleName}");
            if (name == "Table_Localization_ja")
                name = "Table_Localization_"+Plugin.langtext;
        }
    }


    [HarmonyPatch(typeof(Localization), nameof(Localization.AddCSV))]
    public static class LoadCSVPatch
    {
        static void Prefix(ref BetterList<string> newValues, Il2CppStringArray newLanguages, Dictionary<string, int> languageIndices)
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
            Plugin.Log.LogDebug($"AddCSV({ newValues.size}): {sb}");
            if (newValues.size >= 6 && newValues[Plugin.langindex] != null && newValues[Plugin.langindex].Trim().Length != 0)
            {
                newValues[1] = newValues[Plugin.langindex];
                newValues[2] = newValues[Plugin.langindex];
            }
        }
    }

}
