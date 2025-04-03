using BattleUI;
using HarmonyLib;

namespace LimbusLocalizeDCLC
{
    public static class DCLC_SpriteUI
    {
        [HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.SetParryingTypoData))]
        [HarmonyPrefix]
        private static void ParryingTypoUI_SetParryingTypoData(ParryingTypoUI __instance)
          => __instance.img_parryingTypo.sprite = DCLC_ReadmeManager.ReadmeSprites["DCLC_Combo"];
    }
}
