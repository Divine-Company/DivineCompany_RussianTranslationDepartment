using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LimbusLocalizeDCLC
{
    public static class DCLC_LoadingManager
    {
        static List<string> LoadingTexts = new();
        static string Touhou;
        static readonly string Raw = "<bounce f=0.5>Загрузка...</bounce>";
        //public static ConfigEntry<bool> RandomLoadText = DCLCMod.DCLC_Settings.Bind("DCLC Settings", "RandomLoadText", true, "Решает, будут ли появляться случайные фразы загрузки или же базовая версия ( true | false )");
        static DCLC_LoadingManager() => InitLoadingTexts();
        public static void InitLoadingTexts()
        {
            LoadingTexts = File.ReadAllLines(DCLCMod.ModPath + "/Localize/Readme/LoadingTexts.md").ToList();
            for (int i = 0; i < LoadingTexts.Count; i++)
            {
                string LoadingText = LoadingTexts[i];
                LoadingTexts[i] = "<bounce f=0.5>" + LoadingText.Remove(0, 2) + "</bounce>";
            }
            Touhou = LoadingTexts[0];
            LoadingTexts.RemoveAt(0);
        }
        public static T SelectOne<T>(List<T> list)
            => list.Count == 0 ? default : list[Random.Range(0, list.Count)];
        [HarmonyPatch(typeof(LoadingSceneManager), nameof(LoadingSceneManager.Start))]
        [HarmonyPostfix]
        private static void LSM_Start(LoadingSceneManager __instance)
        {
            var loadingText = __instance._loadingText;
            loadingText.font = Russian_Font.GetRussianFonts(0);
            loadingText.fontMaterial = Russian_Font.GetRussianFonts(0).material;
            loadingText.fontSize = 40;
            loadingText.text = Raw;
            /*if (!RandomLoadText.Value)
                return;
            var loadingText = __instance._loadingText;
            loadingText.font = Russian_Font.GetRussianFonts(0);
            loadingText.fontMaterial = Russian_Font.GetRussianFonts(0).material;
            loadingText.fontSize = 40;
            int random = Random.Range(0, 100);
            if (random < 25)
                loadingText.text = Raw;
            else if (random < 50)
                loadingText.text = Touhou;
            else
                loadingText.text = SelectOne(LoadingTexts);*/
        }
    }
}
