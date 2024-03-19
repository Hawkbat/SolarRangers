using SolarRangers.Interop;
using SolarRangers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers
{
    public static class ObjectUtils
    {
        public static T Spawn<T>(Transform parent, string path) where T : Component
        {
            var planet = parent ? parent.root.gameObject : null;
            var astroObject = planet ? planet.GetComponent<AstroObject>() : null;
            var sector = astroObject ? astroObject.GetRootSector() : null;
            var obj = SolarRangers.NewHorizons.SpawnObject(SolarRangers.Instance, planet, sector, path, parent ? parent.position : Vector3.zero, parent ? parent.eulerAngles : Vector3.zero, 1f, false);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            return obj.GetComponent<T>();
        }

        public static GameObject Spawn(Transform parent, string path)
            => Spawn<Transform>(parent, path).gameObject;

        public static T PlaceOnPlanet<T>(T c, string planetName, Vector3 position, Vector3 rotation) where T : Component
        {
            var planet = SolarRangers.NewHorizons.GetPlanet(planetName);
            var sector = planet.GetComponent<AstroObject>().GetRootSector();
            var pos = planet.transform.TransformPoint(position);
            var rot = planet.transform.rotation * Quaternion.Euler(rotation.x, rotation.y, rotation.z);

            var t = c.transform;
            t.SetParent(sector.transform, false);
            t.transform.position = pos;
            t.transform.rotation = rot;

            return c;
        }

        public static T PlaceOnPlanet<T>(T c, AstroObject.Name planetName, Vector3 position, Vector3 rotation) where T : Component
        {
            var planet = Locator.GetAstroObject(planetName);
            var sector = planet.GetRootSector();
            var pos = planet.transform.TransformPoint(position);
            var rot = planet.transform.rotation * Quaternion.Euler(rotation.x, rotation.y, rotation.z);

            var t = c.transform;
            t.SetParent(sector.transform, false);
            t.transform.position = pos;
            t.transform.rotation = rot;

            return c;
        }

        public static OWRigidbody ConvertToPhysicsProp(GameObject obj, OWRigidbody parentBody)
        {
            obj.AddComponent<Rigidbody>();
            var owBody = obj.AddComponent<OWRigidbody>();
            owBody.SetVelocity(parentBody.GetPointVelocity(obj.transform.position));
            owBody.SetMass(0.001f);
            owBody.SetAngularVelocity(parentBody.GetAngularVelocity());
            obj.layer = LayerMask.NameToLayer("PhysicalDetector");
            obj.tag = "DynamicPropDetector";
            var shape = obj.AddComponent<SphereShape>();
            shape._collisionMode = Shape.CollisionMode.Detector;
            shape._layerMask = (int)(Shape.Layer.Default | Shape.Layer.Gravity);
            shape._radius = 1f;
            var forceDetector = obj.AddComponent<DynamicForceDetector>();
            var fluidDetector = obj.AddComponent<DynamicFluidDetector>();
            fluidDetector._buoyancy = Locator.GetProbe().GetOWRigidbody()._attachedFluidDetector._buoyancy;
            fluidDetector._splashEffects = Locator.GetProbe().GetOWRigidbody()._attachedFluidDetector._splashEffects;

            var gravityVolume = parentBody.GetAttachedGravityVolume();
            if (gravityVolume)
            {
                forceDetector.AddVolume(gravityVolume);
            }
            return owBody;
        }

        public static void AddReferenceFrame(GameObject obj, string nameKey, float radius, float minTargetRadius, float maxTargetRadius)
        {
            var go = new GameObject("RFVolume");
            go.transform.parent = obj.transform;
            go.transform.localPosition = Vector3.zero;
            go.layer = LayerMask.NameToLayer("ReferenceFrameVolume");
            go.SetActive(false);

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;
            var rf = new ReferenceFrame(obj.GetAttachedOWRigidbody())
            {
                _minSuitTargetDistance = minTargetRadius,
                _maxTargetDistance = maxTargetRadius,
                _autopilotArrivalDistance = radius,
                _autoAlignmentDistance = radius * 0.75f,
                _hideLandingModePrompt = false,
                _matchAngularVelocity = true,
                _minMatchAngularVelocityDistance = 70,
                _maxMatchAngularVelocityDistance = 400,
                _bracketsRadius = radius * 0.5f
            };

            var rfv = go.AddComponent<ReferenceFrameVolume>();
            rfv._referenceFrame = rf;
            rfv._minColliderRadius = minTargetRadius;
            rfv._maxColliderRadius = radius;
            rfv._isPrimaryVolume = false;
            rfv._isCloseRangeVolume = false;

            rf._useCenterOfMass = false;
            rf._localPosition = Vector3.zero;
            go.transform.localPosition = Vector3.zero;

            ReferenceFrameManager.Register(rf, nameKey);

            go.SetActive(true);
        }

        public static void ReplaceMaterials(GameObject root, Dictionary<string, string> materialMapping)
        {
            var bundle = AssetBundle.GetAllLoadedAssetBundles().First(b => b.name == "solarrangers");
            Dictionary<string, Material> bundleMats = [];
            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                for (var i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    var material = renderer.sharedMaterials[i];
                    if (materialMapping.TryGetValue(material.name, out var newMatName) || materialMapping.TryGetValue(material.name.Replace(" (Instance)", ""), out newMatName))
                    {
                        if (!bundleMats.TryGetValue(newMatName, out var newMat))
                        {
                            newMat = bundle.LoadAsset<Material>($"Assets/SolarRangers/Materials/{newMatName}.mat");
                        }
                        renderer.sharedMaterials[i] = newMat;
                    }
                }
            }
        }
    }
}
