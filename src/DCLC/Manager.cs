using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MainUI;
using MainUI.Gacha;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using ILObject = Il2CppSystem.Object;
using UObject = UnityEngine.Object;
using ULogger = UnityEngine.Logger;

namespace LimbusLocalizeDCLC
{
    public class DCLC_Manager : MonoBehaviour
    {
        static DCLC_Manager()
        {
            ClassInjector.RegisterTypeInIl2Cpp<DCLC_Manager>();
            GameObject obj = new(nameof(DCLC_Manager));
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<DCLC_Manager>();
        }
        public static DCLC_Manager Instance;
        public DCLC_Manager(IntPtr ptr) : base(ptr) { }
        void OnApplicationQuit() => DCLCMod.CopyLog();
        public static void OpenGlobalPopup(string description, string title = null, string close = "Закрыть", string confirm = "Подтвердить", Action confirmEvent = null, Action closeEvent = null)
        {
            if (!GlobalGameManager.Instance) { return; }
            TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMP_FontAsset fontAsset = Russian_Font.GetRussianFonts(3);
            if (fontAsset)
            {
                TextMeshProUGUI btn_canceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_canceltmp.font = fontAsset;
                btn_canceltmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_canceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
                btn_canceltl.enabled = false;
                btn_canceltmp.text = close;
                TextMeshProUGUI btn_oktmp = globalPopupUI.btn_ok.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_oktmp.font = fontAsset;
                btn_oktmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_oktl = globalPopupUI.btn_ok.GetComponentInChildren<UITextDataLoader>(true);
                btn_oktl.enabled = false;
                btn_oktmp.text = confirm;
                globalPopupUI.tmp_title.font = fontAsset;
                globalPopupUI.tmp_title.fontMaterial = fontAsset.material;
                void TextLoaderEnabled() { btn_canceltl.enabled = true; btn_oktl.enabled = true; }
                confirmEvent += TextLoaderEnabled;
                closeEvent += TextLoaderEnabled;
            }
            globalPopupUI._titleObject.SetActive(!string.IsNullOrEmpty(title));
            globalPopupUI.tmp_title.text = title;
            globalPopupUI.tmp_description.text = description;
            globalPopupUI._confirmEvent = confirmEvent;
            globalPopupUI._closeEvent = closeEvent;
            globalPopupUI.btn_cancel.gameObject.SetActive(!string.IsNullOrEmpty(close));
            globalPopupUI._gridLayoutGroup.cellSize = new Vector2(!string.IsNullOrEmpty(close) ? 500 : 700, 100f);
            globalPopupUI.Open();
        }
        public static void InitLocalizes(DirectoryInfo directory)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                Localizes[fileNameWithoutExtension] = value;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
            {
                InitLocalizes(directoryInfo);
            }

        }
        public static Dictionary<string, string> Localizes = new();
        public static Action FatalErrorAction;
        public static string FatalErrorlog;
        #region Блокировка безвредных предупреждений
        [HarmonyPatch(typeof(ULogger), nameof(ULogger.Log), new Type[]
        {
            typeof(LogType),
            typeof(ILObject)
        })]
        [HarmonyPrefix]
        private static bool Log(ULogger __instance, LogType logType, ILObject message)
        {
            if (logType == LogType.Warning)
            {
                string LogString = ULogger.GetString(message);
                if (!LogString.StartsWith("<color=#0099bc><b>DOTWEEN"))
                    __instance.logHandler.LogFormat(logType, null, "{0}", LogString);
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(ULogger), nameof(ULogger.Log),
        [
            typeof(LogType),
            typeof(ILObject),
            typeof(UObject)
        ])]
        [HarmonyPrefix]
        private static bool Log(ULogger __instance, LogType logType, ILObject message, UObject context)
        {
            if (logType == LogType.Warning)
            {
                string LogString = ULogger.GetString(message);
                if (!LogString.StartsWith("Material"))
                    __instance.logHandler.LogFormat(logType, context, "{0}", LogString);
                return false;
            }
            return true;
        }
        #endregion
        #region Исправление ошибок
        [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
        [HarmonyPrefix]
        private static bool LinkToCrackPosition(GachaEffectEventSystem __instance)
            => __instance._parent.EffectChainCamera;
        #endregion
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        public static void CheckModActions()
        {
            if (DCLC_UpdateChecker.UpdateCall != null)
                OpenGlobalPopup("Доступное обновление " + DCLC_UpdateChecker.Updatelog + "!\nЗакрыть игру и скачать обновление\nДоступно обновление мода\nНажмите OK, чтобы закрыть игру и перейти к скачиванию\nИзменить" + DCLC_UpdateChecker.Updatelog + "Перетащите содержимое архива в папку с игрой", "Мод обновлен!", null, "OK", () =>
                {
                    DCLC_UpdateChecker.UpdateCall.Invoke();
                    DCLC_UpdateChecker.UpdateCall = null;
                    DCLC_UpdateChecker.Updatelog = string.Empty;
                });
            else if (FatalErrorAction != null)
                OpenGlobalPopup(FatalErrorlog, "Критическая ошибка мода!", null, "Откройте ссылку DCLC", () =>
                {
                    FatalErrorAction.Invoke();
                    FatalErrorAction = null;
                    FatalErrorlog = string.Empty;
                });
        }
    }
}
