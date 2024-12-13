using BepInEx.Configuration;
using HarmonyLib;
using LocalSave;
using MainUI;
using StorySystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LimbusLocalizeDCLC
{
    public static class DCLC_Russian_Setting
    {
        public static ConfigEntry<bool> IsUseRussian = LCB_DCLCMod.DCLC_Settings.Bind("DCLC Settings", "IsUseRussian", true, "Использовать Русский ( true | false )");
        static bool _isuserussian;
        static Toggle Russian_Setting;
        [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.InitLanguage))]
        [HarmonyPrefix]
        private static bool InitLanguage(SettingsPanelGame __instance, LocalGameOptionData option)
        {
            if (!Russian_Setting)
            {
                Toggle original = __instance._languageToggles[0];
                Transform parent = original.transform.parent;
                var _languageToggle = UnityEngine.Object.Instantiate(original, parent);
                var cntmp = _languageToggle.GetComponentInChildren<TextMeshProUGUI>(true);
                cntmp.font = LCB_Russian_Font.tmprussianfonts[0];
                cntmp.fontMaterial = LCB_Russian_Font.tmprussianfonts[0].material;
                cntmp.text = "<size=40>Русский</size>";
                Russian_Setting = _languageToggle;
                parent.localPosition = new Vector3(parent.localPosition.x - 306f, parent.localPosition.y, parent.localPosition.z);
                while (__instance._languageToggles.Count > 3)
                    __instance._languageToggles.RemoveAt(__instance._languageToggles.Count - 1);
                __instance._languageToggles.Add(_languageToggle);
            }
            foreach (Toggle tg in __instance._languageToggles)
            {
                tg.onValueChanged.RemoveAllListeners();
                Action<bool> onValueChanged = (isOn) =>
                {
                    if (!isOn)
                        return;
                    __instance.OnClickLanguageToggleEx(__instance._languageToggles.IndexOf(tg));
                };
                tg.onValueChanged.AddListener(onValueChanged);
                tg.SetIsOnWithoutNotify(false);
            }
            LOCALIZE_LANGUAGE language = option.GetLanguage();
            if (_isuserussian = IsUseRussian.Value)
                Russian_Setting.SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.KR)
                __instance._languageToggles[0].SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.EN)
                __instance._languageToggles[1].SetIsOnWithoutNotify(true);
            else if (language == LOCALIZE_LANGUAGE.JP)
                __instance._languageToggles[2].SetIsOnWithoutNotify(true);
            __instance._lang = language;
            return false;
        }
        [HarmonyPatch(typeof(SettingsPanelGame), nameof(SettingsPanelGame.ApplySetting))]
        [HarmonyPostfix]
        private static void ApplySetting() => IsUseRussian.Value = _isuserussian;
        private static void OnClickLanguageToggleEx(this SettingsPanelGame __instance, int tgIdx)
        {
            _isuserussian = false;

            switch (tgIdx)
            {
                case 0:
                    __instance._lang = LOCALIZE_LANGUAGE.KR;
                    break;
                case 1:
                    __instance._lang = LOCALIZE_LANGUAGE.EN;
                    break;
                case 2:
                    __instance._lang = LOCALIZE_LANGUAGE.JP;
                    break;
                case 3:
                    _isuserussian = true;
                    break;
            }

        }
        [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneOffset), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool TimeZoneOffset(ref int __result)
        {
            if (!IsUseRussian.Value)
                return true;

            __result = 3;
            return false;
        }
        [HarmonyPatch(typeof(DateUtil), nameof(DateUtil.TimeZoneString), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool TimeZoneString(ref string __result)
        {
            if(!IsUseRussian.Value)
                return true;

            __result = "MSK";
            return false;
        }
        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.ViewCancelTextTypo_Lack))]
        [HarmonyPrefix]
        private static bool ViewCancelTextTypo_Lack(BattleUnitView __instance, CanceledData data)
        {
            if(!IsUseRussian.Value)
                return true;
            if (data?._lackOfBuffs?.Count > 0)
                __instance.UIManager.bufTypoUI.OpenBufTypo(BUF_TYPE.Negative,
                    TextDataManager.Instance.BufList.GetData(data._lackOfBuffs[0].ToString()).GetName() + " недостаточно",
                    data._lackOfBuffs[0]);
        return false;

        }
        [HarmonyPatch(typeof(StoryPlayData), nameof(StoryPlayData.GetDialogAfterClearingAllCathy))]
        [HarmonyPrefix]
        private static bool GetDialogAfterClearingAllCathy(Scenario curStory, Dialog dialog, ref string __result)
        {
            if(!IsUseRussian.Value)
                return true;

            __result = dialog.Content;
            UserDataManager instance = Singleton<UserDataManager>.Instance;
            if ("P10704".Equals(curStory.ID) && instance != null && instance._unlockCodeData != null && instance._unlockCodeData.CheckUnlockStatus(106) && dialog.Id == 3)
            {
                __result = __result.Replace("Кэти", "■■■■■");
            }
            return false;

        }
        [HarmonyPatch(typeof(Util), nameof(Util.GetDlgAfterClearingAllCathy))]
        [HarmonyPrefix]
        private static bool GetDlgAfterClearingAllCathy(string dlgId, string originString, ref string __result)
        {
            if(!IsUseRussian.Value)
                return true;
            
            __result = originString;
            UserDataManager instance = Singleton<UserDataManager>.Instance;
            if (instance == null || instance._unlockCodeData == null || !instance._unlockCodeData.CheckUnlockStatus(106))
                return false;
            if ("battle_defeat_10707_1".Equals(dlgId))
                __result = __result.Replace("Кэти", "■■■■■");
            else if ("battle_dead_10704_1".Equals(dlgId))
                __result = __result.Replace("Кэтрин", "■■■■■");
            return false;

        }

    }
}