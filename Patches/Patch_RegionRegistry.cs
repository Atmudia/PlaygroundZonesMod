using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using SRML.Console;
using UnityEngine;

namespace PlaygroundZonesMod.Patches
{
    
    
    [HarmonyPatch(typeof(RegionRegistry))]
    public static class Patch_RegionRegistry
    {
        private static GameObject Sea;
        [HarmonyPatch(nameof(RegionRegistry.LoadRegion)), HarmonyPostfix]
        public static void LoadRegion(RegionRegistry.RegionSetId setId)
        {
            if (setId != Enums.VRZONE_SETID)
                return;
            Sea ??= GameObject.Find("zoneSEA").transform.Find("Sea").gameObject;
            Sea.SetActive(true);
        }
    }
}