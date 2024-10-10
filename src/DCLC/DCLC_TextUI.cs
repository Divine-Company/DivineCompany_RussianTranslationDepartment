using HarmonyLib;
using TMPro;
using MainUI;
using UnityEngine;

namespace LimbusLocalizeDCLC
{
    internal class DCLC_TextUI
    {

        [HarmonyPatch(typeof(BattlePassUIPopup), nameof(BattlePassUIPopup.localizeHelper.Initialize))]
        [HarmonyPostfix]
        private static void BattlePass_Init(BattlePassUIPopup __instance)
        {
            TextMeshProUGUI dailyMissionButton_textTMP = __instance.dailyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true);
            dailyMissionButton_textTMP.m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            dailyMissionButton_textTMP.fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

            TextMeshProUGUI weeklyMissionButton_textTMP = __instance.weeklyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true);
            weeklyMissionButton_textTMP.m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            weeklyMissionButton_textTMP.fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

            TextMeshProUGUI seasonMissionButton_textTMP = __instance.seasonMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true);
            seasonMissionButton_textTMP.m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            seasonMissionButton_textTMP.fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

            //__instance._levelExpUI.currentPassLevel.GetComponentInChildren<RectTransform>(true).sizeDelta = new Vector2(140, 60);
        }

        
    }
}