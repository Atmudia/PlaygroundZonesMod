using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SRML.Utils;
using UnityEngine;
using Console = SRML.Console.Console;

namespace PlaygroundZonesMod.Patches
{
    [HarmonyPatch(typeof(DynamicObjectContainer))]
    public static class Patch_DynamicObjectContainer
    {
        [HarmonyPatch(nameof(DynamicObjectContainer.Awake)), HarmonyPrefix]
        public static void Awake(DynamicObjectContainer __instance)
        {
            if (!Levels.IsLevel("worldGenerated"))
                return;
            foreach (Transform transform in new[]
                     {
                         EntryPoint.AssetBundle.LoadAsset<GameObject>("Dynamic Objects of zoneREEFVR").transform,
                         EntryPoint.AssetBundle.LoadAsset<GameObject>("Dynamic Objects of zoneRUINSVR").transform,
                         EntryPoint.AssetBundle.LoadAsset<GameObject>("Dynamic Objects of zoneRuinsVR").transform,
                         EntryPoint.AssetBundle.LoadAsset<GameObject>("Dynamic Objects of zoneVRTitle").transform
                     }.SelectMany( x => x.Cast<Transform>()))
            {
                foreach (Transform child in transform)
                {
                    var id = EnumUtils.Parse<Identifiable.Id>(child.name);
                    id.GetPrefab().Instantiate(transform.position, transform.rotation, __instance.transform, true);
                }
               
            }
        }
    }
}