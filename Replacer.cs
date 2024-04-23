using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PlaygroundZonesMod.Utils;
using SRML.SR.SaveSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace PlaygroundZonesMod
{
	public class Replacer : MonoBehaviour
	{
		private static List<string> MissingMeshNames = new List<string>();
		private static List<string> MissingMaterialNames = new List<string>();
		private static Mesh GetAnyMesh(string meshName)
		{
			bool flag = SRObjects.additionalMeshes.TryGetValue(meshName, out var mesh);
			if (flag)
				return mesh;
			mesh = SRObjects.Get<Mesh>(meshName);
			if (mesh != null) return mesh;
			if (MissingMeshNames.Contains(meshName)) return null;
			EntryPoint.ConsoleInstance.Log($"Missing Mesh: {meshName}");
			MissingMeshNames.Add(meshName);
			return null;
		}
		private static Material GetAnyMaterial(string materialName)
		{
			bool flag = SRObjects.additionalMaterials.TryGetValue(materialName, out var material);
			if (flag)
				return material;
			material = SRObjects.Get<Material>(materialName);
			if (material != null) return material;
			if (MissingMaterialNames.Contains(materialName)) return null;
			MissingMaterialNames.Add(materialName);
			EntryPoint.ConsoleInstance.Log($"Missing Material: {materialName}");
			return null;
			//SRObjects.additionalMaterials.Add(materialName, material);
		}
		
		public void Awake()
		{
			try
			{
				if (replaceEmptyGameObjects)
				{
					foreach (var component in GetComponentsInChildren<Component>())
					{
						var type = component.GetType();
						foreach (var fieldInfo in type.GetFields(AccessTools.all).Where(x => x.FieldType == typeof(GameObject)))
						{
							GameObject value = fieldInfo.GetValue(component) as GameObject;
							if (value != null && value.name.EndsWith("_empty"))
							{
								fieldInfo.SetValue(component, SRObjects.Get<GameObject>(value.name.Replace("_empty", string.Empty)));
							}
						}
						
						if (component is SpawnResource spawnResource)
						{
							spawnResource.ObjectsToSpawn = spawnResource.ObjectsToSpawn.Select(x => SRObjects.Get<GameObject>(x.name.Replace("_empty", string.Empty))).ToArray();
							spawnResource.BonusObjectsToSpawn = spawnResource.BonusObjectsToSpawn.Select(x => SRObjects.Get<GameObject>(x.name.Replace("_empty", string.Empty))).ToArray();
						}

						if (component is not DirectedActorSpawner directedSpawner) 
							continue;
						foreach (var constraint in directedSpawner.constraints.SelectMany(x => x.slimeset.members))
						{
							constraint.prefab = SRObjects.Get<GameObject>(constraint.prefab.name.Replace("_empty", string.Empty));
						}
					}
				}
				if (this.GetComponent<DirectedAnimalSpawner>())
				{
					var child = transform.GetChild(0);
					if (child != null)
					{
						Object.DestroyImmediate(child.gameObject);
					}
				}
				
				if (this.name.StartsWith("nodeSlime"))
				{
					Object.DestroyImmediate(this);
					return;
				}
				if (!this.materials.IsNullOrEmpty())
				{
					base.gameObject.AddComponent<MeshRenderer>().sharedMaterials = this.materials.Select(GetAnyMaterial).ToArray();
				}
				if (!this.mesh.IsNullOrEmpty())
				{
					base.gameObject.AddComponent<MeshFilter>().sharedMesh = Replacer.GetAnyMesh(this.mesh);
				}
				if (whatIsThisCollider != -1)
				{
					int text = this.whatIsThisCollider;
					switch (text)
					{
						case 0:
						{
							MeshCollider addComponent = base.gameObject.AddComponent<MeshCollider>();
							addComponent.sharedMesh = Replacer.GetAnyMesh(this.meshOfCollider);
							addComponent.isTrigger = this.isTrigger;
							break;
						}
						case 1:
						{
							SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
							sphereCollider.center = this.center;
							sphereCollider.radius = this.radius;
							sphereCollider.isTrigger = this.isTrigger;
							break;
						}
						case 2:
						{
							BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
							boxCollider.center = this.center;
							boxCollider.size = this.size;
							boxCollider.isTrigger = this.isTrigger;
							break;
						}
						case 3:
						{
							CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
							capsuleCollider.center = this.center;
							capsuleCollider.radius = this.radius;
							capsuleCollider.height = this.height;
							capsuleCollider.isTrigger = this.isTrigger;
							break;
						}
					}
				}
				if (TryGetComponent(out LiquidSource liquidSource))
				{
					liquidSource.director = this.GetComponentInParent<IdDirector>();
					if (liquidSource.director != null)
						liquidSource.director.persistenceDict.Add(liquidSource, ModdedStringRegistry.ClaimID(liquidSource.IdPrefix(), idHandler));
					
				}
				if (landPlotId != -1)
				{ 
					GameObject origLandPlot = GameObject.Find("zoneRANCH/cellRanch_Home/Sector/Ranch Features/").transform.GetChild(7).gameObject;
					var landPlot = origLandPlot.InstantiateInactive(this.transform.position, this.transform.rotation, transform.parent, true);
					var landPlotLocation = landPlot.GetComponent<LandPlotLocation>();
					
					landPlotLocation.director = GetComponentInParent<IdDirector>();
					if (landPlotLocation.director != null)
						landPlotLocation.director.persistenceDict.Add(landPlotLocation, ModdedStringRegistry.ClaimID(landPlotLocation.IdPrefix(), idHandler));
					landPlot.SetActive(true);
				}
				Object.DestroyImmediate(this);
			}
			catch (Exception e)
			{
				EntryPoint.ConsoleInstance.Log($"This replacer is causing problems: {e}");
				throw;
			}
		}
		public bool isTrigger;
		public List<string> materials;
		public string mesh;
		public string meshOfCollider;
		public Vector3 center;
		public Vector3 size;
		public float radius;
		public float height;
		public int whatIsThisCollider = -1;
		
		public bool replaceEmptyGameObjects;

		public int landPlotId = -1;
		public string idHandler;


	}
}
