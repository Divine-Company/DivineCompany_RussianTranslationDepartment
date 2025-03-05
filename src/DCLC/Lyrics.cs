using HarmonyLib;
using Voice;
using BattleUI.Dialog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using FMOD.Studio;
using UnityEngine.UI;

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

            string personalityID = "";

            Debug.Log("voice event path: " + path);

            if (path.StartsWith("battle_special_"))
                personalityID = path.Split('_')[2];
            else
                personalityID = path.Split('_')[^2];

            Debug.Log("personalityID: " + personalityID);
            Debug.Log("mainVoice multiVoicesCount: " + VoiceGenerator.multiVoices.Count);
            Debug.Log("mainVoice personalityID: " + VoiceGenerator.mainVoice.personalityID);

            string personalityName = Singleton<TextDataSet>.Instance.PersonalityList.GetData(personalityID).name;

            Debug.Log("Voice volume: " + VoiceGenerator.volume);
            Debug.Log("Voice Length: " + VoiceGenerator.GetCurrentVoiceLength());
            Debug.Log("Name: " + personalityName + "\n");


            if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue(personalityID, out var dataList)) return;

            var gradientController = SingletonBehavior<OutterGradiantEffectController>.Instance;

            gradientController._dialogText_Upper.m_fontStyle = TMPro.FontStyles.Normal;
            //gradientController._dialogText_Upper.fontMaterial = ;
            gradientController._dialogText_Upper.color = new Color(1, 1, 1, 1);

            // Создаем эффект тени
            Shadow shadowEffect = gradientController._dialogText_Upper.GetComponent<Shadow>();
            if (!shadowEffect)
            {
                shadowEffect = gradientController._dialogText_Upper.gameObject.AddComponent<Shadow>();
            }

            // Настраиваем параметры тени
            shadowEffect.effectColor = new Color(0, 0, 0, 0.5f); // Черный цвет с прозрачностью 50%
            shadowEffect.effectDistance = new Vector2(2, -2);   // Смещение тени (X, Y)
            shadowEffect.useGraphicAlpha = true;                // Использовать прозрачность текста

            string text_color = $"{ColorUtility.ToHtmlStringRGBA(ColorSchemes[$"{personalityName}"])}";
            

            foreach (var data in dataList.dataList)
                if (path.Equals(data.id))
                {
                    gradientController.SetDialog_Upper($"<size=120%><color=#{text_color}>{data.dlg}</color></size>\n", 0, 5);
                    break;
                }
            //gradientController._dialogText_Upper.text = null;
            //;
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

            if (!Singleton<TextDataSet>.Instance.personalityVoiceText._voiceDictionary.TryGetValue($"{id}", out var dataList)) return;

            var gradientController = SingletonBehavior<OutterGradiantEffectController>.Instance;

            string[] pathParts = path.Split('_');
            string ThisSkillId = $"{pathParts[0]}" + "_" + $"{pathParts[1]}" + "_" + $"{pathParts[2]}";

            gradientController._dialogText_Upper.m_fontStyle = TMPro.FontStyles.Normal;
            //gradientController._dialogText_Upper.fontMaterial = gradientController._dialogText_Upper.font.material;
            gradientController._dialogText_Upper.color = new Color(1, 1, 1, 1);

            // Создаем эффект тени
            Shadow shadowEffect = gradientController._dialogText_Upper.GetComponent<Shadow>();
            if (!shadowEffect)
            {
                shadowEffect = gradientController._dialogText_Upper.gameObject.AddComponent<Shadow>();
            }
            // Настраиваем параметры тени
            shadowEffect.effectColor = new Color(0, 0, 0, 0.5f); // Черный цвет с прозрачностью 50%
            shadowEffect.effectDistance = new Vector2(2, -2);   // Смещение тени (X, Y)
            shadowEffect.useGraphicAlpha = true;                // Использовать прозрачность текста

            string text_color = $"{ColorUtility.ToHtmlStringRGBA(ColorSchemes[$"{personalityName}"])}";

            foreach (var data in dataList.dataList)
                if (path.Equals(data.id))
                {
                    Debug.Log("LastSkillId: " + LastSkillId);
                    Debug.Log("phrase: " + data.dlg + "\n");
                    if(ThisSkillId.Equals(LastSkillId))
                        gradientController.SetDialog_Upper(gradientController._dialogText_Upper.text + " " + $"<size=120%><color=#{text_color}>{data.dlg}</color></size>", 0, 5);
                    else
                        gradientController.SetDialog_Upper($"<size=120%><color=#{text_color}>{data.dlg}</color></size>", 0, 5);
                    LastSkillId = ThisSkillId;
                    break;
                }
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