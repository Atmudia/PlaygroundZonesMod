using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using PlaygroundZonesMod.Utils;
using SRML.SR.SaveSystem;
using UnityEngine;

namespace PlaygroundZonesMod.Cells
{
    public static class CellMossVR
    {
        public static void Initialize(GameObject zoneVR)
        {
            var cellMossVR = EntryPoint.AssetBundle.LoadAsset<GameObject>("zoneMOSSVR").transform.Find("cellMossVR").gameObject.Instantiate(new Vector3(-732.9204f, 4.7f, 65f), Quaternion.identity, zoneVR.transform, true);
            cellMossVR.GetComponent<CellDirector>().AddDroneMapping("PlaygroundZonesMod.cellMossVR.json");
            
            cellMossVR.transform.Find("Sector/Ranch Features/gadgetSlimeStage").gameObject.DestroyImmediate();
            foreach (var spawnResource in cellMossVR.GetComponentsInChildren<SpawnResource>().SelectMany(x => x.SpawnJoints))
            {
                spawnResource.GetComponent<Replacer>().DestroyImmediate();
            }
            foreach (var transform in cellMossVR.GetComponentsInChildren<Transform>())
            {
                if (transform.name.StartsWith("nodeSlime"))
                {
                    transform.gameObject.SetActive(Config.EnableDefaultSpawners);
                }
            }
            cellMossVR.GetComponent<Region>().bounds = new Bounds()
            {
                center = new Vector3(-754.6288f, 39.9781f, 75.895f),
                extents = new Vector3(133.65f, 119.1f, 133.65f)
            };
            var siteGadgets = new GameObject("Gadget Sites")
            {
                transform = { parent = cellMossVR.transform.Find("Sector").transform}
            };
            for (var index = 0; index < EntryPoint.GadgetSiteMappings["cellMossVR"].Count; index++)
            {
                var vector3Save = EntryPoint.GadgetSiteMappings["cellMossVR"][index];
                SRObjects.CreateGadgetSite(new Vector3(vector3Save.x, vector3Save.y, vector3Save.z), ModdedStringRegistry.ClaimID("Site", $"SRPlayground.Site.{cellMossVR.name}.{index}"), siteGadgets.transform);
            }

            cellMossVR.transform.Find("Sector").CreateTeleport("cellMossVR02", "cellMossVR01", SRObjects.Get<Sprite>("iconZoneLab"), new Vector3(-777.3044f, 12.99859f, 95.26738f), Quaternion.Euler(0, 84, 0));
            cellMossVR.SetActive(true);
        }
    }
}