using HarmonyLib;
using TMPro;
using MainUI;
using UnityEngine;
using System.Linq;

namespace LimbusLocalizeDCLC
{
    internal class DCLC_TextUI
    {
        [HarmonyPatch(typeof(StageInfoUI), nameof(StageInfoUI.SetDataOpen))]
        [HarmonyPostfix]
        private static void StageInfoUI_Init(StageInfoUI __instance)
        {
            TextMeshProUGUI Lv_textTMP = __instance._stageInfoDisplay.rect_level.GetComponentInChildren<TextMeshProUGUI>(true);
            Lv_textTMP.text = Lv_textTMP.text.Replace("LV.", "Ур. ");  //TODO Rewrite
            Lv_textTMP.font = Russian_Font.GetRussianFonts(2);

            TextMeshProUGUI Title_textTMP = __instance._stageInfoDisplay.rect_title.GetChild(2).GetComponentInChildren<TextMeshProUGUI>(true);  //TODO Rewrite
            Title_textTMP.characterSpacing = 1;
        }

        [HarmonyPatch(typeof(NetworkingUI), "Initialize")]
        [HarmonyPostfix]
        private static void NetworkingUI_Init(NetworkingUI __instance)
        {
            TextMeshProUGUI connecting_textTMP = __instance._connectingRoot.GetComponentInChildren<TextMeshProUGUI>(true);
            connecting_textTMP.text = "СОЕДИНЕНИЕ";
            connecting_textTMP.fontSize = 98;
            connecting_textTMP.font = Russian_Font.GetRussianFonts(0);
        }


    }
}