using HarmonyLib;
using PlaygroundZonesMod.Cells;

namespace PlaygroundZonesMod.Patches
{
    [HarmonyPatch(typeof(AccessDoorUI))]
    public static class Patch_AccessDoorUI
    {
        [HarmonyPatch(nameof(AccessDoorUI.UnlockDoor)),  HarmonyPrefix]
        public static bool Prefix(AccessDoorUI __instance)
        {
            if (__instance.door is not VRAccessDoor)
                return true;
            var progressDirector = SRSingleton<SceneContext>.Instance.ProgressDirector;
            var hasProgress = progressDirector.HasProgress(ProgressDirector.ProgressType.UNLOCK_MOSS) && progressDirector.HasProgress(ProgressDirector.ProgressType.UNLOCK_RUINS);
            if (__instance.playerState.GetCurrency() >= __instance.door.doorPurchase.cost && hasProgress)
            {
                __instance.playerState.SpendCurrency(__instance.door.doorPurchase.cost);
                __instance.door.CurrState = AccessDoor.State.OPEN;
                if (__instance.door.linkedDoors != null)
                {
                    foreach (AccessDoor linkedDoor in __instance.door.linkedDoors)
                    {
                        if (linkedDoor.CurrState == AccessDoor.State.LOCKED)
                            linkedDoor.CurrState = AccessDoor.State.CLOSED;
                    }
                }
                __instance.Play(SRSingleton<GameContext>.Instance.UITemplates.purchaseExpansionCue);
                __instance.Close();
                SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow();
            }
            else
            {
                __instance.PlayErrorCue();
                __instance.Error("e.insuf_coins_zones");
            }

            return false;
        }
    }
}