using System.Linq;
using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;

namespace PlaygroundZonesMod.Patches
{
    [HarmonyPatch(typeof(MusicDirector), nameof(MusicDirector.GetRegionMusic))]
    public static class Patch_MusicDirector
    {
        public static bool Prefix(MusicDirector __instance, RegionMember member, ref MusicDirector.Music __result)
        {
            if (!member.IsInZone(Enums.VRZONE))
                return true;
            __result = member.regions.Aggregate(__result, (current, memberRegion) => memberRegion.name switch
            {
                "cellRuinsVR" => __instance.ruinsMusic,
                "cellMossVR" => __instance.mossMusic,
                "cellReefVR" => __instance.reefMusic,
                _ => current
            });
            return __result == null;
        }
    }
}