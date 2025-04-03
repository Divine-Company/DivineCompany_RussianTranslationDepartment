using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalizeDCLC
{
    public static class DCLC_UpdateChecker
    {
        public static ConfigEntry<bool> AutoUpdate = DCLCMod.DCLC_Settings.Bind("DCLC Settings", "AutoUpdate", false, "Автоматически проверять и загружать обновления (true|false)");
        public static ConfigEntry<URI> UpdateURI = DCLCMod.DCLC_Settings.Bind("DCLC Settings", "UpdateURI", URI.GitHub, "Автоматическое обновление URI ( GitHub:по умолчанию | Mirror_OneDrive:Обновления могут появляться чуть позже)");
        public static void StartAutoUpdate()
        {
            if (AutoUpdate.Value)
            {
                DCLCMod.LogWarning($"Check Mod Update From {UpdateURI.Value}");
                Action ModUpdate = CheckModUpdate;
                new Thread(ModUpdate).Start();
            }
        }
        static void CheckModUpdate()
        {
            string release_uri = "https://api.github.com/repos/Divine-Company/DivineCompany_RussianTranslationDepartment/releases/latest";
            UnityWebRequest www = UnityWebRequest.Get(release_uri);
            www.timeout = 4;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            if (www.result != UnityWebRequest.Result.Success)
                DCLCMod.LogWarning($"Не удается подключиться к {UpdateURI.Value}!!! " + www.error);
            else
            {
                var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
                string latestReleaseTag = latest["tag_name"].Value.Remove(latest["tag_name"].Value.Length - 1, 1);
                DCLCMod.LogWarning(latestReleaseTag);
                if (Version.Parse(DCLCMod.VERSION) <= Version.Parse(latestReleaseTag.Remove(0, 1)))
                {
                    string download_uri = $"https://github.com/Divine-Company/DivineCompany_RussianTranslationDepartment/releases/download/{latest["tag_name"].Value}/Divine.Company.rar";
                    var dirs = download_uri.Split('/');
                    string filename = DCLCMod.GamePath + "/" + dirs[^1];
                    if (!File.Exists(filename))
                        DownloadFileAsync(download_uri, filename);
                    UpdateCall = UpdateDel;
                }
            }
        }
        static void UpdateDel()
        {
            DCLCMod.OpenGamePath();
            Application.Quit();
        }
        static void DownloadFileAsync(string uri, string filePath)
        {
            try
            {
                DCLCMod.LogWarning("Download " + uri + " To " + filePath);
                using HttpClient client = new();
                using HttpResponseMessage response = client.GetAsync(uri).GetAwaiter().GetResult();
                using HttpContent content = response.Content;
                using FileStream fileStream = new(filePath, FileMode.Create);
                content.CopyToAsync(fileStream).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException httpException && httpException.StatusCode == HttpStatusCode.NotFound)
                    DCLCMod.LogWarning($"{uri} 404 NotFound, No Resource");
                else
                    DCLCMod.LogWarning($"{uri} Error!!!" + ex.ToString());
            }
        }
        public static string Updatelog;
        public static Action UpdateCall;
        public enum URI
        {
            GitHub,
            Mirror_OneDrive
        }
    }
}