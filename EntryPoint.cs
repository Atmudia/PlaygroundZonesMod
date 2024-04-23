using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using Assets.Script.Util.Extensions;
using AssetsLib;
// using BetterBuild.Persistance;
using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Regions;
using PlaygroundZonesMod.Cells;
using PlaygroundZonesMod.Utils;
// using PlaygroundZonesMod.Persistence;
using SRML;
using SRML.Console;
using SRML.Console.Commands;
using SRML.SR;
using SRML.SR.Patches;
using SRML.SR.SaveSystem;
using SRML.SR.Utils;
using SRML.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace PlaygroundZonesMod
{
    
    public class EntryPoint : ModEntryPoint
    {
        public new static Console.ConsoleInstance ConsoleInstance;
        public static AssetBundle AssetBundle;
        public static Dictionary<string, List<Vector3>> GadgetSiteMappings;
        public static Assembly ExecAssembly;
        public static Sprite IconVRZone;

        public static Shader[] beforeAssetBundleShaders;

        public override void PreLoad()
        {
            if (!SRModLoader.IsModPresent("assetslib"))
                throw new MissingReferenceException("This mod requires assetslib. Please install it");
            ExecAssembly = Assembly.GetExecutingAssembly();
            TranslationPatcher.AddUITranslation("e.insuf_coins_zones", "Not enough newbucks or don't have required\n areas like The Moss Blanket or The Ruins");
            TranslationPatcher.AddPediaTranslation("t.vrzone_pediaid", "VR Playground");
            TranslationPatcher.AddPediaTranslation("m.intro.vrzone_pediaid", "The ranch's very own distant areas");
            TranslationPatcher.AddPediaTranslation("m.desc.vrzone_pediaid", "The VR playground area is actually just a nickname that ended up becoming the name of a real area on the Far, Far Range, which was used as a reference by its creator, Viktor Humphries, to develop a VR playground simulation. This simulation, based on a refined version of Viktor's slimeulation, allows people on Earth to experience life on the Far, Far Range. It serves both as entertainment for fun-seekers and as a training ground for aspiring ranchers who want to get to know the environment of the Far, Far Range.");
            PediaRegistry.RegisterInitialPediaEntry(Enums.VRZONE_PEDIAID);
            PediaRegistry.SetPediaCategory(Enums.VRZONE_PEDIAID, PediaRegistry.PediaCategory.RANCH);
            using (Stream bundleStream = ExecAssembly.GetManifestResourceStream("PlaygroundZonesMod.gadgetmappings.json"))
            {
                using StreamReader streamReader = new (bundleStream ?? throw new InvalidOperationException());
                GadgetSiteMappings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<Vector3>>>(streamReader.ReadToEnd());
            }
            ConsoleInstance = base.ConsoleInstance;
            HarmonyInstance.PatchAll();
            SRCallbacks.PreSaveGameLoaded += OnSrCallbacksOnPreSaveGameLoaded;
            SRCallbacks.OnSaveGameLoaded += SRCallbacksOnOnSaveGameLoaded;
            Stream manifestResourceStream = ExecAssembly.GetManifestResourceStream("PlaygroundZonesMod.playgroundzones"); 
            AssetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
            PediaRegistry.RegisterIdEntry(Enums.VRZONE_PEDIAID, IconVRZone = AssetBundle.LoadAsset<Sprite>("iconVRZone"));
            RegionSetRegistry.RegisterRegion(Enums.VRZONE_SETID, new BoundsQuadtree<Region>(2000,new Vector3(0,0,0),250,1.2f ));
            RegionSetRegistry.RegisterZoneIntoRegion(Enums.VRZONE, Enums.VRZONE_SETID);
        }
        public override void Load()
        {
            beforeAssetBundleShaders = Resources.FindObjectsOfTypeAll<Shader>();
            SRObjects.additionalMeshes = AssetBundle.LoadAllAssets<Mesh>().ToDictionary(x => x.name, x => x);
            SRObjects.additionalMaterials = new Dictionary<string, Material>();
            foreach (var material in SRObjects.additionalMaterials)
            {
                material.Value.shader = beforeAssetBundleShaders.FirstOrDefault(x => x.name == material.Value.name);
            }   
        }

      

        private static void OnSrCallbacksOnPreSaveGameLoaded(SceneContext menu)
        {
            SRObjects.TeleportPrefab = GameObject.Find("zoneSEA/cellSea_MiniIsland/Sector/Constructs/objTeleportTwoWay");
            SRObjects.GadgetSitePrefab = GameObject.Find("zoneRANCH/cellRanch_Home/Sector/Build Sites/siteGadget (2)");
            var empty = beforeAssetBundleShaders.FirstOrDefault();
            if (SRObjects.additionalMaterials.Count != 0)
            {
                foreach (var keyValuePair in SRObjects.additionalMaterials.Where(keyValuePair => !keyValuePair.Key.Equals("envMossRocky01")))
                {
                    Object.DestroyImmediate(keyValuePair.Value);
                }

                SRObjects.additionalMaterials.Clear();
            }
            var material1 = new Material(empty)
            {
                name = "VRDefaultUI",
            };
            SRObjects.additionalMaterials.Add(material1.name, material1);

            material1 = new Material(empty)
            {
                name = "RanchTech 1",
            };
            SRObjects.additionalMaterials.Add(material1.name, material1);

            material1 = SRObjects.Get<Material>("objMtnMoss01");
            SRObjects.additionalMaterials.Add("envMossRocky01", material1);
            material1 = new Material(empty)
            {
                name = "objRuinBarrierBlocksLow"
            };
            SRObjects.additionalMaterials.Add(material1.name, material1);
            material1 = new Material(empty)
            {
                name = "objMossRockB01"
            };
            SRObjects.additionalMaterials.Add(material1.name, material1);
            material1 = new Material(empty)
            {
                name = "envRuinsBlocks02_sub"
            };
            SRObjects.additionalMaterials.Add(material1.name, material1);
            SRObjects.cache.Clear();
            CellTitleVR.Initialize();
            var zoneVR = AssetBundle.LoadAsset<GameObject>("zoneVR").InstantiateInactive(true);
            zoneVR.transform.position = new Vector3(0, 600, 0);
            zoneVR.AddComponent<IdDirector>();
            zoneVR.GetComponent<ZoneDirector>().zone = Enums.VRZONE;
            CellMossVR.Initialize(zoneVR);
            CellReefVR.Initialize(zoneVR);
            CellRuinsVR.Initialize(zoneVR);
            SceneManager.MoveGameObjectToScene(zoneVR, SRObjects.GadgetSitePrefab.scene);
            zoneVR.SetActive(true);
        }
        private void SRCallbacksOnOnSaveGameLoaded(SceneContext t)
        {
            var transparentCutout = beforeAssetBundleShaders.FirstOrDefault( x=> x.name.Contains("Unlit/Transparent Cutout"));
            var recolorx8 = beforeAssetBundleShaders.FirstOrDefault(x => x.name.Equals("SR/Actor/Recolor x8"));
            var triplanarTopper = beforeAssetBundleShaders.FirstOrDefault(x => x.name.Equals("SR/Paintlight/Triplanar Topper"));

            var additionalMaterial = SRObjects.additionalMaterials["VRDefaultUI"];
            additionalMaterial.shader = transparentCutout;
            additionalMaterial.mainTexture = AssetBundle.LoadAsset<Texture2D>("monomipark_logo");
            
            additionalMaterial = SRObjects.additionalMaterials["RanchTech 1"];
            additionalMaterial.shader = recolorx8;
            additionalMaterial.SetTexture("_ColorMask", SRObjects.Get<Texture2D>("mask_ranchtech"));
            additionalMaterial.SetTexture("_AmbientOcclusion", SRObjects.Get<Texture2D>("dpt_paintmask"));
            additionalMaterial.SetFloat("_AOUV1", 1);
            additionalMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
            additionalMaterial.SetColor("_Color00", new Color(0.83921576f, 0.5764706f, 0.19607845f, 0f));
            additionalMaterial.SetColor("_Color01", new Color(0.67058825f, 0.5882353f, 0.227451f, 0.134f));
            additionalMaterial.SetColor("_Color10", new Color(0.16480237f, 0.14208478f, 0.2647059f, 0f));
            additionalMaterial.SetColor("_Color11", new Color(0.31194952f, 0.26967993f, 0.31617647f, 0.616f));
            additionalMaterial.SetColor("_Color20", new Color(0.41176468f, 0.24966726f, 0.1483564f, 0f));
            additionalMaterial.SetColor("_Color21", new Color(0.27450982f, 0.23137257f, 0.21568629f, 0.103f));
            additionalMaterial.SetColor("_Color30", new Color(0.5647059f, 0.5411765f, 0.5019608f, 0f));
            additionalMaterial.SetColor("_Color31", new Color(0.6392157f, 0.6039216f, 0.4901961f, 0.266f));
            additionalMaterial.SetColor("_Color40", new Color(0.7686275f, 0.13333334f, 0.2509804f, 0f));
            additionalMaterial.SetColor("_Color41", new Color(0.9686275f, 0.21568629f, 0.50980395f, 0f));
            additionalMaterial.SetColor("_Color50", new Color(0.4039216f, 0.427451f, 0.4156863f, 0f));
            additionalMaterial.SetColor("_Color51", new Color(0.5294118f, 0.5294118f, 0.54509807f, 1f));
            additionalMaterial.SetColor("_Color60", new Color(0f, 0f, 0f, 0f));
            additionalMaterial.SetColor("_Color61", new Color(0f, 0f, 0f, 0f));
            additionalMaterial.SetColor("_Color70", new Color(0f, 0f, 0f, 0f));
            additionalMaterial.SetColor("_Color71", new Color(0f, 0f, 0f, 0f));
            additionalMaterial.SetFloat("_AOUV1", 1f);
            additionalMaterial.SetFloat("_NRMUV1", 0f);
            additionalMaterial.SetFloat("_NRMTriplanar", 1f);
            additionalMaterial.SetFloat("_PaintMaskTriplanar", 1f);
            additionalMaterial.SetFloat("_OverrideUV1", 1f);
            additionalMaterial.SetFloat("_GlowMultiplier", 1f);
            additionalMaterial.SetFloat("_AssembleOffset", 0.67f);
            additionalMaterial.SetFloat("_AssembleDif", 0.25f);
            additionalMaterial.SetFloat("_AssembleDistance", 15f);
            additionalMaterial.SetFloat("_AssembleFalloff", 1f);
            
            additionalMaterial = SRObjects.additionalMaterials["objRuinBarrierBlocksLow"];
            additionalMaterial.shader = triplanarTopper;
            additionalMaterial.SetTexture("_PrimaryTex", SRObjects.Get<Texture2D>("envCliffMoss02"));
            additionalMaterial.SetTexture("_Depth", SRObjects.Get<Texture2D>("dptCliffMoss03"));
            additionalMaterial.SetTexture("_Topper_MainTex", SRObjects.Get<Texture2D>("envDirtMoss03"));
            additionalMaterial.SetTexture("_TopperDetailTex", SRObjects.Get<Texture2D>("envDirtMoss03"));
            additionalMaterial.SetTexture("_DetailNoiseMask", SRObjects.Get<Texture2D>("dptCracked02"));
            additionalMaterial.SetTexture("_VerticalRamp", SRObjects.Get<Texture2D>("strat_banding"));
            additionalMaterial.SetFloat("_UseMeshUVs", 1);
            additionalMaterial.SetFloat("_RampOffset", 2f);
            additionalMaterial.SetFloat("_RampScale", 1f);
            additionalMaterial.SetFloat("_SeaLevelRampOffset", -3f);
            additionalMaterial.SetFloat("_SeaLevelRampScale", 1f);
            additionalMaterial.SetFloat("_SeaLevelRampObjectPos", 0f);
            additionalMaterial.SetFloat("_BlurOffset", 0.0075f);
            additionalMaterial.SetVector("_RampUpper", new Vector4(0.5f, 0.6f, 0.7f, 1.0f));
            additionalMaterial.SetVector("_SeaLevelRampLower", new Vector4(0.3f, 0.4f, 0.4f, 1.0f));
            additionalMaterial.SetVector("_SpecularColor", new Vector4(0.4f, 0.8f, 0.8f, 1.0f));
            additionalMaterial.SetVector("_GlassColor", new Vector4(0.4f, 0.5f, 0.5f, 1.0f));
            
            additionalMaterial = SRObjects.additionalMaterials["objMossRockB01"];
            additionalMaterial.shader = triplanarTopper;
            additionalMaterial.SetTexture("_PrimaryTex", AssetBundle.LoadAsset<Texture2D>("objRockCaveB01"));
            additionalMaterial.SetTexture("_Depth", SRObjects.Get<Texture2D>("dptBrushed01"));
            additionalMaterial.SetTexture("_Topper_MainTex", SRObjects.Get<Texture2D>("envGrass01"));
            additionalMaterial.SetTexture("_TopperDepth", SRObjects.Get<Texture2D>("dptGrass"));
            additionalMaterial.SetTexture("_DetailTex", SRObjects.Get<Texture2D>("envMossCliff01"));
            additionalMaterial.SetTexture("_TopperDetailTex", SRObjects.Get<Texture2D>("envGrass01"));
            additionalMaterial.SetTexture("_DetailNoiseMask", SRObjects.Get<Texture2D>("dptNoiseClouds"));
            additionalMaterial.SetTexture("_TopperSpecular", SRObjects.Get<Texture2D>("spcGlitter"));
            additionalMaterial.SetFloat("_UseMeshUVs", 1);
            additionalMaterial.SetFloat("_EnableDetailTex", 0);
            additionalMaterial.SetFloat("_TopperEnableDetailTex", 1);
            additionalMaterial.SetFloat("_TopperDepthStrength", 1f);
            additionalMaterial.SetFloat("_RampOffset", 2f);
            additionalMaterial.SetFloat("_RampScale", 1f);
            additionalMaterial.SetFloat("_SeaLevelRampOffset", -3f);
            additionalMaterial.SetFloat("_SeaLevelRampScale", 1f);
            additionalMaterial.SetFloat("_TopRampOffset", 30f);
            additionalMaterial.SetFloat("_TopRampScale", 1f);
            additionalMaterial.SetFloat("_PrimeLowerToggle", 0f);
            additionalMaterial.SetFloat("_PrimeLowerOffset", 30f);
            additionalMaterial.SetFloat("_PrimeLowerScale", 1f);
            additionalMaterial.SetVector("_RampUpper", new Vector4(0.4f, 0.4f, 0.6f, 1.0f));
            additionalMaterial.SetVector("_SeaLevelRampLower", new Vector4(0.3f, 0.4f, 0.4f, 1.0f));
            additionalMaterial.SetVector("_SpecularColor", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            additionalMaterial.SetVector("_EmissiveColor", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            additionalMaterial.SetVector("_DifOverlay", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            additionalMaterial.SetVector("_RampTop", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

            
            additionalMaterial = SRObjects.additionalMaterials["envRuinsBlocks02_sub"];
            additionalMaterial.shader = triplanarTopper;
            additionalMaterial.SetFloat("_UseMeshUVs", 0);
            additionalMaterial.SetFloat("_EnableDetailTex", 1);
            additionalMaterial.SetFloat("_TopperEnableDetailTex", 1);
            additionalMaterial.SetFloat("_TopperDepthStrength", 0.5f);
            additionalMaterial.SetFloat("_RampOffset", 2f);
            additionalMaterial.SetFloat("_RampScale", 0.75f);
            additionalMaterial.SetFloat("_SeaLevelRampOffset", -0.35f);
            additionalMaterial.SetFloat("_SeaLevelRampScale", 0.91f);
            additionalMaterial.SetFloat("_TopRampOffset", 10f);
            additionalMaterial.SetFloat("_TopRampScale", 10f);
            additionalMaterial.SetFloat("_PrimeLowerToggle", 0f);
            additionalMaterial.SetFloat("_PrimeLowerOffset", 30f);
            additionalMaterial.SetFloat("_PrimeLowerScale", 1f);
            additionalMaterial.SetTexture("_PrimaryTex", SRObjects.Get<Texture2D>("envRuinBlocks"));
            additionalMaterial.SetTexture("_Depth", SRObjects.Get<Texture2D>("dpt_envRuinBlocks"));
            additionalMaterial.SetTexture("_Topper_MainTex", AssetBundle.LoadAsset<Texture2D>("envRuinBlocks03"));
            additionalMaterial.SetTexture("_TopperDepth", SRObjects.Get<Texture2D>("dpt_envRuinBlocks"));
            additionalMaterial.SetTexture("_DetailTex", SRObjects.Get<Texture2D>("envRocky08"));
            additionalMaterial.SetTexture("_TopperDetailTex", SRObjects.Get<Texture2D>("envSeaTop02"));
            additionalMaterial.SetTexture("_DetailNoiseMask", SRObjects.Get<Texture2D>("dptNoiseClouds_contrast"));
            additionalMaterial.SetTexture("_TopperSpecular", SRObjects.Get<Texture2D>("spec_envRuinBlocks"));
            additionalMaterial.SetVector("_RampUpper", new Vector4(0.1f, 0.5f, 0.7f, 1.0f));
            additionalMaterial.SetVector("_SeaLevelRampLower", new Vector4(0.2f, 0.4f, 0.4f, 1.0f));
            additionalMaterial.SetVector("_SpecularColor", new Vector4(0.7f, 0.6f, 1.0f, 1.0f));
            additionalMaterial.SetVector("_EmissiveColor", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            additionalMaterial.SetVector("_DifOverlay", new Vector4(0.3f, 0.3f, 0.4f, 1.0f));
            additionalMaterial.SetVector("_RampTop", new Vector4(0.8f, 0.6f, 0.6f, 0.9f));
            


        }
        
        
        
    }
   
}
