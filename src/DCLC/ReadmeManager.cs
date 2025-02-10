using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using LocalSave;
using MainUI;
using MainUI.NoticeUI;
using SimpleJSON;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace LimbusLocalizeDCLC
{
    public static class DCLC_ReadmeManager
    {
    public static NoticeUIPopup NoticeUIInstance;
    public static RedDotWriggler RedDotNotice;
    public static List<Notice> ReadmeList = new();
    public static Dictionary<string, Sprite> ReadmeSprites;
    public static System.Collections.Generic.Dictionary<string, Action> ReadmeActions;
        static DCLC_ReadmeManager()
        {
            InitReadmeList();
            InitReadmeSprites();
            ReadmeActions = new System.Collections.Generic.Dictionary<string, Action>
            {
                {
                    "Action_Issues",()=>
                    {
                        DCLCMod.CopyLog();
                        DCLCMod.OpenGamePath();
                        Application.OpenURL(DCLCMod.DCLCLink + "/issues?q=is:issue");
                    }
                }
            };
        }
        public static void UIInitialize()
        {
            Action _close = Close;
            NoticeUIInstance._popupPanel.closeEvent.AddListener(_close);
            NoticeUIInstance._arrowScroll.Initialize();
            NoticeUIInstance._titleViewManager.Initialized();
            NoticeUIInstance._contentViewManager.Initialized();
            NoticeUIInstance.btn_back._onClick.AddListener(_close);
            Action eventNotice_onClick = NoticeUIInstance.EventTapClickEvent;
            Action systemNotice_onClick = NoticeUIInstance.SystemTapClickEvent;
            NoticeUIInstance.btn_eventNotice._onClick.AddListener(eventNotice_onClick);
            NoticeUIInstance.btn_systemNotice._onClick.AddListener(systemNotice_onClick);
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "Версия";
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "Объявления";
        }
        public static void Open()
        {
            NoticeUIInstance.Open();
            NoticeUIInstance._popupPanel.Open();
            var notices = ReadmeList;
            NoticeUIInstance._systemNotices = notices.FindAll((Func<Notice, bool>)Findsys);
            NoticeUIInstance._eventNotices = notices.FindAll((Func<Notice, bool>)Findeve);
            NoticeUIInstance.EventTapClickEvent();
            NoticeUIInstance.btn_eventNotice.Cast<UISelectedButton>().SetSelected(true);
            return;

            bool Findsys(Notice x)
            {
                return x.noticeType == NOTICE_TYPE.System;
            }

            bool Findeve(Notice x)
            {
                return x.noticeType == NOTICE_TYPE.Event;
            }
        }
        public static void InitReadmeSprites()
        {
            ReadmeSprites = new Dictionary<string, Sprite>();

            foreach (FileInfo fileInfo in new DirectoryInfo(DCLCMod.ModPath + "/Localize/Readme").GetFiles().Where(f => f.Extension is ".jpg" or ".png"))
            {
                Texture2D texture2D = new(2, 2);
                ImageConversion.LoadImage(texture2D, File.ReadAllBytes(fileInfo.FullName));
                Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                texture2D.name = fileNameWithoutExtension;
                sprite.name = fileNameWithoutExtension;
                UObject.DontDestroyOnLoad(sprite);
                sprite.hideFlags |= HideFlags.HideAndDontSave;
                ReadmeSprites[fileNameWithoutExtension] = sprite;
            }
        }
        public static void InitReadmeList()
        {
            ReadmeList.Clear();
            foreach (var notices in JSONNode.Parse(File.ReadAllText(DCLCMod.ModPath + "/Localize/Readme/Readme.json"))[0].AsArray.m_List)
            {
                ReadmeList.Add(HandleDynamicType(notices.ToString()));
            }
        }
        public static Notice HandleDynamicType(string jsonPayload)
        {
            var noticetype = typeof(NoticeSynchronousDataList).GetMethod("get_noticeFormats")!.Invoke(SynchronousDataManager.Instance.NoticeSynchronousDataList, null)!.GetType()
                .GetGenericArguments()[0];

            var deserializedObject = typeof(JsonUtility).GetMethod("FromJson", [typeof(string)])!
                .MakeGenericMethod(noticetype).Invoke(null, [jsonPayload]);

            return Activator.CreateInstance(typeof(Notice), deserializedObject, LOCALIZE_LANGUAGE.KR) as Notice;
        }
        public static void Close()
        {
            Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.Save();
            NoticeUIInstance._popupPanel.Close();
            UpdateNoticeRedDot();
        }
    
        public static void UpdateNoticeRedDot()
        {
            RedDotNotice?.gameObject.SetActive(IsValidRedDot());
        }
        public static bool IsValidRedDot()
        {
            var i = 0;
            var count = ReadmeList.Count;
            while (i < count)
            {
                var readme = ReadmeList[i];
                if (!readme.StartDate.isFuture && !readme.EndDate.isPast &&
                    !UserLocalSaveDataRoot.Instance.NoticeRedDotSaveModel.TryCheckId(readme.ID)) return true;
                i++;
            }

            return false;
        }
        #region Про объявления
        [HarmonyPatch(typeof(UserLocalNoticeRedDotModel), nameof(UserLocalNoticeRedDotModel.InitNoticeList))]
        [HarmonyPrefix]
        private static bool InitNoticeList(UserLocalNoticeRedDotModel __instance, List<int> severNoticeList)
        {
            //UpdateChecker.ReadmeUpdate(); TODO
            if (__instance.idList.RemoveAll((Func<int, bool>)Func) > 0)
                __instance.isChanged = true;
            __instance.Save();
            UpdateNoticeRedDot();
            return false;

            bool Func(int id)
            {
                return !severNoticeList.Contains(id) && ReadmeList.FindAll((Func<Notice, bool>)Func2).Count == 0;

                bool Func2(Notice notice)
                {
                    return notice.ID == id;
                }
            }
        }

        [HarmonyPatch(typeof(NoticeUIPopup), nameof(NoticeUIPopup.Initialize))]
        [HarmonyPostfix]
        private static void NoticeUIPopupInitialize(NoticeUIPopup __instance)
        {
            if (!NoticeUIInstance)
            {
                var NoticeUIPopupInstance = UObject.Instantiate(__instance, __instance.transform.parent);
                NoticeUIInstance = NoticeUIPopupInstance;
                UIInitialize();
            }
        }
        [HarmonyPatch(typeof(MainLobbyUIPanel), nameof(MainLobbyUIPanel.Initialize))]
        [HarmonyPostfix]
        private static void MainLobbyUIPanelInitialize(MainLobbyUIPanel __instance)
        {
            var UIButtonInstance = UObject.Instantiate(__instance.button_notice, __instance.button_notice.transform.parent).Cast<MainLobbyRightUpperUIButton>();
            RedDotNotice = UIButtonInstance.gameObject.GetComponentInChildren<RedDotWriggler>(true);
            UpdateNoticeRedDot();
            UIButtonInstance._onClick.RemoveAllListeners();
            Action onClick = Open;
            UIButtonInstance._onClick.AddListener(onClick);
            UIButtonInstance.transform.SetSiblingIndex(1);
            var spriteSetting = ScriptableObject.CreateInstance<ButtonSprites>();
            spriteSetting._enabled = ReadmeSprites["Readme_Zero_Button"];
            spriteSetting._hover = ReadmeSprites["Readme_Zero_Button"];
            UIButtonInstance.spriteSetting = spriteSetting;
            var transform = __instance.button_notice.transform.parent;
            var layoutGroup = transform.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.childScaleHeight = true;
            layoutGroup.childScaleWidth = true;
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).localScale = new Vector3(0.77f, 0.77f, 1f);
        }
        [HarmonyPatch(typeof(NoticeUIContentImage), nameof(NoticeUIContentImage.SetData))]
        [HarmonyPrefix]
        private static bool ImageSetData(NoticeUIContentImage __instance, string formatValue)
        {
            if (!formatValue.StartsWith("Readme_")) return true;
            var image = ReadmeSprites[formatValue];
            __instance.gameObject.SetActive(true);
            __instance.SetImage(image);
            return false;
        }
        [HarmonyPatch(typeof(NoticeUIContentHyperLink), nameof(NoticeUIContentHyperLink.OnPointerClick))]
        [HarmonyPrefix]
        private static bool HyperLinkOnPointerClick(NoticeUIContentHyperLink __instance)
        {
            string url = __instance.tmp_main.text;
            if (url.StartsWith("<link"))
            {
                int startIndex = url.IndexOf('=');
                if (startIndex != -1)
                {
                    int endIndex = url.IndexOf('>', startIndex + 1);
                    if (endIndex != -1)
                    {
                        url = url.Substring(startIndex + 1, endIndex - startIndex - 1);
                    }
                }
                if (url.StartsWith("Action_"))
                {
                    ReadmeActions[url]?.Invoke();
                    return false;
                }
            }
            Application.OpenURL(url);
            return false;
        }
        #endregion
    }
}
