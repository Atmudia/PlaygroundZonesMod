using HarmonyLib;

namespace PlaygroundZonesMod.Patches
{
    [HarmonyPatch(typeof(CellDirector))]
    public static class Patch_CellDirector
    {
        [HarmonyPatch(nameof(CellDirector.Despawn)), HarmonyPrefix]
        public static bool Despawn(CellDirector __instance)
        {
            return __instance.GetZoneId() != Enums.VRZONE;
        }

    }
}