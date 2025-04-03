using HarmonyLib;
using Voice;
using BattleUI.Dialog;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using FMOD.Studio;
using UnityEngine.UI;
using UnityEngine.Playables;
using BattleUI;

namespace LimbusLocalizeDCLC
{
    public static class BVL_lyrics
    {

        public static Dictionary<string, Color> ColorSchemes = new ()
        {
            {"Yi Sang", new Color(0.827f, 0.875f, 0.906f, 1.0f)},       // #d3dfe7
            {"Faust", new Color(0.996f, 0.698f, 0.71f, 1.0f)},          // #feb2b5
            {"Don Quixote", new Color(0.996f, 0.933f, 0.141f, 1.0f)},   // #feee24
            {"Ryōshū", new Color(0.804f, 0, 0, 1.0f)},                  // #cd0000
            {"Meursault", new Color(0.353f, 0.412f, 0.686f, 1.0f)},     // #5a69af
            {"Hong Lu", new Color(0.361f, 0.996f, 0.863f, 1.0f)},       // #5cfedc
            {"Heathcliff", new Color(0.553f, 0.392f, 0.749f, 1.0f)},    // #8d64bf
            {"Ishmael", new Color(0.996f, 0.58f, 0, 1.0f)},             // #fe9400
            {"Rodion", new Color(0.569f, 0.216f, 0.216f, 1.0f)},        // #913737
            {"Sinclair",new Color(0.541f, 0.608f, 0.082f, 1.0f)},       // #8a9b15
            {"Outis", new Color(0.416f, 0.592f, 0.451f, 1.0f)},         // #6a9773
            {"Gregor", new Color(0.624f, 0.349f, 0.114f, 1.0f)},        // #9f591d
        };

        public static Dictionary<string, Color32> ColorSchemes32 = new ()
        {
            {"Yi Sang", new Color32(211, 223, 231, 255)},       // #d3dfe7
            {"Faust", new Color32(254, 178, 181, 255)},          // #feb2b5
            {"Don Quixote", new Color32(254, 238, 36, 255)},   // #feee24
            {"Ryōshū", new Color32(205, 0, 0, 255)},                  // #cd0000
            {"Meursault", new Color32(90, 105, 175, 255)},     // #5a69af
            {"Hong Lu", new Color32(92, 254, 220, 255)},       // #5cfedc
            {"Heathcliff", new Color32(141, 100, 191, 255)},    // #8d64bf
            {"Ishmael", new Color32(254, 148, 0, 255)},             // #fe9400
            {"Rodion", new Color32(145, 55, 55, 255)},        // #913737
            {"Sinclair",new Color32(138, 155, 21, 255)},       // #8a9b15
            {"Outis", new Color32(106, 151, 115, 255)},         // #6a9773
            {"Gregor", new Color32(159, 89, 29, 255)},        // #9f591d
        };


        [HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.CreateVoiceInstance))]
        [HarmonyPostfix]
        [HarmonyDebug]
        private static void CreateVoiceInstance(string path, bool isSpecial)
        {
            if (!_unitView) return;

            path = path[VoiceGenerator.VOICE_EVENT_PATH.Length..];

            if (path.StartsWith("Announcer")) return;

            if (path.StartsWith("login")) return;

            if (path.StartsWith("battleentry")) return;

            if (!path.StartsWith("battle_")) return;

            Debug.Log("voice event path: " + path);

            string personalityID = "";

            if (path.StartsWith("battle_special_") || path.StartsWith("battle_s3")) //For Erlking
                personalityID = path.Split('_')[2];
            else
                personalityID = path.Split('_')[^2];

            Debug.Log("personalityID: " + personalityID);

            string personalityName = Singleton<TextDataSet>.Instance.PersonalityList.GetData(personalityID).name;

            Debug.Log("Name: " + personalityName + "\n");


            if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue(personalityID, out var dataList)) return;

            var gradientController = SingletonBehavior<OutterGradiantEffectController>.Instance;
            var ShadowImage = gradientController._dialogText_Upper.gameObject.GetComponentInParent<Image>(true);

            gradientController._dialogText_Upper.m_fontStyle = FontStyles.Normal;

            foreach (var data in dataList.dataList)
                if (path.Equals(data.id))
                {
                    gradientController._dialogText_Upper.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, ColorSchemes[$"{personalityName}"]);
                    gradientController.SetDialog_Upper($"</color><size=120%>{data.dlg}</size>\n", 0, 5);
                    EnableandDisableShadow(gradientController._dialogText_Upper, ShadowImage, 5);
                    
                    gradientController._dialogText_Upper.faceColor = ColorSchemes32[$"{personalityName}"];
                    break;
                }
        }

        public static string LastSkillId = "";

        [HarmonyPatch(typeof(VoiceGenerator), nameof(VoiceGenerator.PlaySkillVoice))]
        [HarmonyPostfix]
        [HarmonyDebug]
        private static void PlaySkillVoice(EventInstance instance, int id, bool isSpecial, float volume)
        {
            Debug.Log("id: " + id);
            string personalityName = Singleton<TextDataSet>.Instance.PersonalityList.GetData(id).name;
            Debug.Log("Name: " + personalityName);

            instance.getDescription(out var EventDescription);

            EventDescription.getPath(out string path);

            path = path[VoiceGenerator.VOICE_EVENT_PATH.Length..];

            Debug.Log("path after: " + path);

            if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue(id.ToString(), out var dataList)) return;

            var gradientController = SingletonBehavior<OutterGradiantEffectController>.Instance;
            var ShadowImage = gradientController._dialogText_Upper.gameObject.GetComponentInParent<Image>(true);

            string[] pathParts = path.Split('_');
            string ThisSkillId = $"{pathParts[0]}" + "_" + $"{pathParts[1]}" + "_" + $"{pathParts[2]}";

            gradientController._dialogText_Upper.m_fontStyle = FontStyles.Normal;

            foreach (var data in dataList.dataList)
                if (path.Equals(data.id))
                {
                    Debug.Log("LastSkillId: " + LastSkillId);
                    Debug.Log("phrase: " + data.dlg + "\n");
                    gradientController._dialogText_Upper.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, ColorSchemes[$"{personalityName}"]);
                    if(ThisSkillId.Equals(LastSkillId))
                        gradientController.SetDialog_Upper(gradientController._dialogText_Upper.text + " " + $"<size=120%>{data.dlg}</size>", 0, 5);
                    else
                        gradientController.SetDialog_Upper($"</color><size=120%>{data.dlg}</size>", 0, 5);
                    EnableandDisableShadow(gradientController._dialogText_Upper, ShadowImage, 5);
                    gradientController._dialogText_Upper.faceColor = ColorSchemes32[$"{personalityName}"];
                    LastSkillId = ThisSkillId;
                    break;
                }
        }

        async static void EnableandDisableShadow(TextMeshProUGUI Text, Image Shadow, float duration)
        {
            if(!Shadow.enabled) Shadow.enabled = true;
            await Task.Delay((int)(duration * 1300));
            if(!Text.enabled) Shadow.enabled = false;
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.SetPlayVoice))]
        [HarmonyPrefix]
        private static void BattleUnitView_Func(BattleUnitView __instance, BattleCharacterVoiceType key, bool isSpecial,
    BattleSkillViewer skillViewer)
        {
            _unitView = __instance;
        }
        private static BattleUnitView _unitView;

        [HarmonyPatch(typeof(LoadingSceneManager), nameof(LoadingSceneManager.SetHintText))]
        [HarmonyPrefix]
        private static void LoadingSceneManager_Init14(LoadingSceneManager __instance)
        {
            _unitView = null;
        }
    }
}