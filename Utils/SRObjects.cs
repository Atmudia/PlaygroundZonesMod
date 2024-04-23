using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using AssetsLib;
using MonomiPark.SlimeRancher.Serializable.Optional;
using SRML.SR.SaveSystem;
using SRML.SR.Utils;
using UnityEngine;
using UnityEngine.UI;
using static PlaygroundZonesMod.EntryPoint;
using Object = UnityEngine.Object;

namespace PlaygroundZonesMod.Utils
{
    internal static class SRObjects
    {
        internal static readonly Dictionary<Type, Object[]> cache = new Dictionary<Type, Object[]>();
        internal static GameObject TeleportPrefab;
        internal static GameObject GadgetSitePrefab;
        public static T Get<T>(string name) where T : Object
        {
            Type selected = typeof(T);
            if (!cache.ContainsKey(selected))
                cache.Add(selected, Resources.FindObjectsOfTypeAll<T>());
            T found = (T)cache[selected].FirstOrDefault(x => x != null && x.name == name);
            if (found != null) 
                return found;
            cache[selected] = Resources.FindObjectsOfTypeAll<T>();
            found = (T)cache[selected].FirstOrDefault(x => x.name == name);
            if (found == null)
            {
                ConsoleInstance.Log($"Missing {typeof(T).Name} named {name}");
            }
            return found;
        }


        public static void CreateTeleport(this Transform @base, string destinationSetName, string teleportDestinationName, Sprite icon, Vector3 position = default, Quaternion rotation = default,  bool isFormedBefore = false)
        {
            var spawnPosition = !isFormedBefore ? position : @base.position;
            var spawnRotation = !isFormedBefore ? rotation : @base.rotation;
            var parent = isFormedBefore ? @base.transform.parent : @base.transform;
            var teleport = SRObjects.TeleportPrefab.Instantiate(spawnPosition, spawnRotation, parent).transform;
            teleport.GetComponentInChildren<TeleportSource>().destinationSetName = destinationSetName;
            teleport.GetComponentInChildren<TeleportDestination>().teleportDestinationName = teleportDestinationName; 
            teleport.GetComponentInChildren<Image>().sprite = icon;
            if (isFormedBefore)
                @base.gameObject.DestroyImmediate();
        }
        internal static void CreateGadgetSite(Vector3 position, string data, Transform parent)
        {
            var gameObject = GadgetSitePrefab.Instantiate(position, Quaternion.identity, parent, true);
            var gadgetSite = gameObject.GetComponent<GadgetSite>();
            gadgetSite.director = IdHandlerUtils.GlobalIdDirector;
            gadgetSite.director.persistenceDict.Add(gadgetSite, data);
        }
        public static void AddDroneMapping(this CellDirector toAdd, string from)
        {
            Stream manifestResourceStream = ExecAssembly.GetManifestResourceStream(from);
            using StreamReader streamReader = new StreamReader(manifestResourceStream);
            string text = streamReader.ReadToEnd();
            JsonObject jsonObject = (JsonObject)JsonValue.Parse(text);
            DroneNetworkUtils.CreateNodesFromJson(toAdd, jsonObject, true);
            DroneNetwork component = toAdd.GetComponent<DroneNetwork>();
            component.whitelistConnections = new List<Pather.NodePair>();
            component.blacklistConnections = new List<Pather.NodePair>();
            RanchCellFastForwarder ranchCellFastForwarder = toAdd.gameObject.AddComponent<RanchCellFastForwarder>();
            ranchCellFastForwarder.director = toAdd.transform.parent.GetComponent<IdDirector>();
            ranchCellFastForwarder.director.persistenceDict.Add(ranchCellFastForwarder, ModdedStringRegistry.ClaimID("ranch", toAdd.gameObject.name));
        }
        internal static Dictionary<string, Mesh> additionalMeshes = new Dictionary<string, Mesh>();
        internal static Dictionary<string, Material> additionalMaterials = new Dictionary<string, Material>();
    }
}
