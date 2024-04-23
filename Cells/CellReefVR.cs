using System.Linq;
using MonomiPark.SlimeRancher.Regions;
using PlaygroundZonesMod.Utils;
using SRML.SR.SaveSystem;
using UnityEngine;

namespace PlaygroundZonesMod.Cells
{
    public class CellReefVR
    {
        public static void Initialize(GameObject zoneVR)
        {
            var cellReefVR = EntryPoint.AssetBundle.LoadAsset<GameObject>("zoneREEFVR").transform.Find("cellReefVR").gameObject.Instantiate(new Vector3(-390.4853f, -10.14195f, -726.7545f), Quaternion.identity, zoneVR.transform, true);
            foreach (Transform collider in cellReefVR.transform.Find("Sector/Colliders"))
            {
                if (collider.name.StartsWith("playerBlocker")) 
                    collider.gameObject.SetActive(false);
            }
            cellReefVR.GetComponent<CellDirector>().AddDroneMapping("PlaygroundZonesMod.cellReefVR.json");
            cellReefVR.GetComponent<Region>().bounds = new Bounds()
            {
                extents = new Vector3(178.2f, 119.1f, 178.2f),
                center = new Vector3(-394.4865f, -47.0877f, -711.9658f)
            };
            var siteGadgets = new GameObject("Gadget Sites")
            {
                transform = { parent = cellReefVR.transform.Find("Sector").transform}
            };
            for (var index = 0; index < EntryPoint.GadgetSiteMappings["cellReefVR"].Count; index++)
            {
                var vector3Save = EntryPoint.GadgetSiteMappings["cellReefVR"][index];
                SRObjects.CreateGadgetSite(new Vector3(vector3Save.x, vector3Save.y, vector3Save.z), ModdedStringRegistry.ClaimID("Site", $"SRPlayground.Site.{cellReefVR.name}.{index}"), siteGadgets.transform);
            }
            cellReefVR.transform.Find("Sector/Ranch Features/gadgetSlimeHoop").gameObject.DestroyImmediate();
            cellReefVR.transform.Find("Sector/Ranch Features/gadgetSpringPad").gameObject.DestroyImmediate();

            foreach (var spawnResource in cellReefVR.GetComponentsInChildren<SpawnResource>().SelectMany(x => x.SpawnJoints))
            {
                spawnResource.GetComponent<Replacer>().DestroyImmediate();
            }
            foreach (var transform in cellReefVR.GetComponentsInChildren<Transform>())
            {
                if (transform.name.StartsWith("nodeSlime"))
                {
                    transform.gameObject.SetActive(Config.EnableDefaultSpawners);
                }
            }
            cellReefVR.transform.Find("Sector").CreateTeleport("cellReefVR02", "cellReefVR01", SRObjects.Get<Sprite>("iconZoneLab"), new Vector3(-385.8301f, -2.3895f, -686.2509f), Quaternion.Euler(0f, 167.5227f, 0));
            cellReefVR.SetActive(true);
        }
    }
}