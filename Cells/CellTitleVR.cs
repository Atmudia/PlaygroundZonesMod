using System;
using MonomiPark.SlimeRancher.Regions;
using PlaygroundZonesMod.Utils;
using SRML.SR.SaveSystem;
using SRML.SR.Utils;
using UnityEngine;

namespace PlaygroundZonesMod.Cells
{
    public class VRAccessDoor : AccessDoor
    {
        public GameObject realBarrier;
    }
    public static class CellTitleVR
    {
        public static void Initialize()
        {
            var cellTitleVR = EntryPoint.AssetBundle.LoadAsset<GameObject>("zoneVRTITLE").transform.Find("cellTitleVR").gameObject.InstantiateInactive(new Vector3(189.7642f, 7.5488f, -381.8974f), Quaternion.Euler(0f, 90f, 0), GameObject.Find("zoneRANCH").transform, true);
            cellTitleVR.GetComponent<Region>().bounds = new Bounds()
            {
                center = new Vector3(210.0617f, 28.5839f, -386.0507f),
                extents = new Vector3(44.55f, 79.4f, 44.55f)
            };
            cellTitleVR.transform.Find("Sector/objTeleportREEF").CreateTeleport("cellReefVR01", "cellReefVR02", SRObjects.Get<Sprite>("iconZoneReef"), isFormedBefore: true);
            cellTitleVR.transform.Find("Sector/objTeleportMOSS").CreateTeleport("cellMossVR01", "cellMossVR02", SRObjects.Get<Sprite>("iconZoneMoss"), isFormedBefore: true);
            cellTitleVR.transform.Find("Sector/objTeleportRUINS").CreateTeleport("cellRuinsVR01", "cellRuinsVR02", SRObjects.Get<Sprite>("iconZoneRuins"), isFormedBefore: true);
            var techDoorExpansion = GameObject.Find("zoneRANCH/cellRanch_PassageLab/Sector/Ranch Features/techDoorExpansion").Instantiate(new Vector3(202.8642f, 15.0474f, -382.7335f), Quaternion.identity, cellTitleVR.transform.Find("Sector/Ranch Features")); 
            techDoorExpansion.RemoveComponentImmediate<LabAccessDoor>();
            
            var vrAccessDoor = techDoorExpansion.AddComponent<VRAccessDoor>();
            vrAccessDoor.director = IdHandlerUtils.GlobalIdDirector;
            vrAccessDoor.director.persistenceDict.Add(vrAccessDoor, ModdedStringRegistry.ClaimID("door", $"SRPlayground.Door.{cellTitleVR.name}"));
            vrAccessDoor.lockedRegionId = Enums.VRZONE_PEDIAID;
            vrAccessDoor.linkedDoors = Array.Empty<AccessDoor>();
            vrAccessDoor.externalAnimators = Array.Empty<Animator>();
            vrAccessDoor.progress = Array.Empty<ProgressDirector.ProgressType>();
            vrAccessDoor.realBarrier = cellTitleVR.transform.Find("Sector/Cliffs/valley_MochiGate (2)").gameObject;
            vrAccessDoor.doorPurchase = new AccessDoor.DoorPurchaseItem()
            {
                cost = 5000,
                icon = EntryPoint.IconVRZone,
                img = EntryPoint.IconVRZone
            };
            foreach (var transform in cellTitleVR.GetComponentsInChildren<Transform>())
            {
                if (transform.name.StartsWith("nodeSlime"))
                {
                    transform.gameObject.SetActive(Config.EnableDefaultSpawners);
                }
            }
            cellTitleVR.SetActive(true);
        }
    }
}