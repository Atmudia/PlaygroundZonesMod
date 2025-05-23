using HarmonyLib;
using PlaygroundZonesMod.Cells;
using UnityEngine;

namespace PlaygroundZonesMod.Patches
{
    [HarmonyPatch(typeof(BarrierController))]
    public static class Patch_BarrierController
    {
        [HarmonyPatch(nameof(BarrierController.SetIsOpen)), HarmonyPrefix]
        public static void SetIsOpen(BarrierController __instance, bool isOpen)
        {
            var vrAccessDoor = __instance.GetComponentInParent<VRAccessDoor>();
            if (!vrAccessDoor)
                return;
            vrAccessDoor.realBarrier.GetComponent<BoxCollider>().enabled = !isOpen;

        }
    }
}