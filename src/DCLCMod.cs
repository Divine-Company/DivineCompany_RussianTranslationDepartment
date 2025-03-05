using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LimbusLocalizeDCLC
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class DCLCMod : BasePlugin
    {
        public static ConfigFile DCLC_Settings;
        public static string ModPath;
        public static string GamePath;
        public const string GUID = "Com.Bright.LocalizeLimbusCompany";
        public const string NAME = "DivineCompany";
        public const string VERSION = "0.2.2";
        public const string AUTHOR = "Original: Bright; Fork: KreeperHLC and Helck1";
        public const string DCLCLink = "https://github.com/Divine-Company/DivineCompany_RussianTranslationDepartment";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static Harmony Harmony = new(NAME);
        public static void OpenDCLCURL() => Application.OpenURL(DCLCLink);
        public static void OpenGamePath() => Application.OpenURL(GamePath);
        public override void Load()
        {
            DCLC_Settings = Config;
            LogWarning = log => Log.LogWarning(log);
            LogError = log => Log.LogError(log);
            LogFatalError = (string log, Action action) => { DCLC_Manager.FatalErrorlog += log + "\n"; LogError(log); DCLC_Manager.FatalErrorAction = action; DCLC_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent!.FullName;
            //DCLC_UpdateChecker.StartAutoUpdate(); TODO
            try
            {
                if (DCLC_Russian_Setting.IsUseRussian.Value)
                {
                    DCLC_Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/RU"));
                    Harmony.PatchAll(typeof(Russian_Font));
                    Harmony.PatchAll(typeof(DCLC_ReadmeManager));
                    Harmony.PatchAll(typeof(DCLC_LoadingManager));
                    Harmony.PatchAll(typeof(DCLC_SpriteUI));
                    Harmony.PatchAll(typeof(DCLC_TextUI));
                }
                Harmony.PatchAll(typeof(DCLC_Manager));
                Harmony.PatchAll(typeof(DCLC_Russian_Setting));
                if(DCLC_Russian_Setting.Lyrics.Value) Harmony.PatchAll(typeof(BVL_lyrics));
                if (!Russian_Font.AddRussianFont(ModPath + "/tmprussianfonts"))
                    LogFatalError("Отсутствует русский шрифт. Пожалуйста, посетите GitHub мода и убедитесь, что у Вас всё установленно согласено инструкции", OpenDCLCURL);
            }
            catch (Exception e)
            {
                LogFatalError("Unknown crit error!!!", () => { CopyLog(); OpenGamePath(); OpenDCLCURL(); });
                LogError(e.ToString());
            }
        }
        public static void CopyLog()
        {
            File.Copy(GamePath + "/BepInEx/LogOutput.log", GamePath + "/Latest.log", true);
            File.Copy(Application.consoleLogPath, GamePath + "/Player.log", true);
        }
    }
}
