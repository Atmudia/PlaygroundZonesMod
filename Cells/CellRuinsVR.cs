using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using PlaygroundZonesMod.Utils;
using SRML.SR.SaveSystem;
using UnityEngine;

namespace PlaygroundZonesMod.Cells
{
    public class CellRuinsVR
    {
        public static void Initialize(GameObject zoneVR)
        {
            var cellRuinsVR = EntryPoint.AssetBundle.LoadAsset<GameObject>("zoneRUINSVR").transform.Find("cellRuinsVR").gameObject.Instantiate(new Vector3(-1764.179f, 2.36894f, -464), Quaternion.identity, zoneVR.transform, true);
            foreach (Transform colliders in cellRuinsVR.transform.Find("Sector/Colliders"))
            {
                colliders.gameObject.SetActive(false);
            }
            cellRuinsVR.GetComponent<CellDirector>().AddDroneMapping("PlaygroundZonesMod.cellRuinsVR.json");
            cellRuinsVR.GetComponent<Region>().bounds = new Bounds()
            {
                extents = new Vector3(178.2f, 198.5f, 148.5f),
                center = new Vector3(-1734.7f,71.4578f, -417.3472f)
            };
            var siteGadgets = new GameObject("Gadget Sites")
            {
                transform = { parent = cellRuinsVR.transform.Find("Sector").transform}
            };
            for (var index = 0; index < EntryPoint.GadgetSiteMappings["cellRuinsVR"].Count; index++)
            {
                var vector3Save = EntryPoint.GadgetSiteMappings["cellRuinsVR"][index];
                SRObjects.CreateGadgetSite(new Vector3(vector3Save.x, vector3Save.y, vector3Save.z), ModdedStringRegistry.ClaimID("Site", $"SRPlayground.Site.{cellRuinsVR.name}.{index}"), siteGadgets.transform);
            }
            foreach (var spawnResource in cellRuinsVR.GetComponentsInChildren<SpawnResource>().SelectMany(x => x.SpawnJoints))
            {
                spawnResource.GetComponent<Replacer>().DestroyImmediate();
            }
            foreach (var transform in cellRuinsVR.GetComponentsInChildren<Transform>())
            {
                if (transform.name.StartsWith("nodeSlime"))
                {
                    transform.gameObject.SetActive(Config.EnableDefaultSpawners);
                }
            }
            cellRuinsVR.transform.Find("Sector").CreateTeleport("cellRuinsVR02", "cellRuinsVR01", SRObjects.Get<Sprite>("iconZoneLab"), new Vector3(-1758.484f, 9.9f, -475.4825f), Quaternion.Euler(0, 0, 0));
        }
    }
}