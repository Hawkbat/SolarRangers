using SolarRangers.Interop;
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
        public static GameObject Spawn(Transform parent, string path)
        {
            var planet = parent ? parent.root.gameObject : null;
            var astroObject = planet ? planet.GetComponent<AstroObject>() : null;
            var sector = astroObject ? astroObject.GetRootSector() : null;
            var obj = SolarRangers.NewHorizons.SpawnObject(SolarRangers.Instance, planet, sector, path, parent ? parent.position : Vector3.zero, parent ? parent.eulerAngles : Vector3.zero, 1f, false);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            return obj;
        }

        public static void PlaceOnPlanet(Transform t, string planetName, Vector3 position, Vector3 rotation)
        {
            var planet = SolarRangers.NewHorizons.GetPlanet(planetName);
            var sector = planet.GetComponent<AstroObject>().GetRootSector();
            var pos = planet.transform.TransformPoint(position);
            var rot = planet.transform.rotation * Quaternion.Euler(rotation.x, rotation.y, rotation.z);

            t.SetParent(sector.transform, false);
            t.transform.position = pos;
            t.transform.rotation = rot;
        }

        public static void PlaceOnPlanet(Transform t, AstroObject.Name planetName, Vector3 position, Vector3 rotation)
        {
            var planet = Locator.GetAstroObject(planetName);
            var sector = planet.GetRootSector();
            var pos = planet.transform.TransformPoint(position);
            var rot = planet.transform.rotation * Quaternion.Euler(rotation.x, rotation.y, rotation.z);

            t.SetParent(sector.transform, false);
            t.transform.position = pos;
            t.transform.rotation = rot;
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

        public static void AddReferenceFrame(GameObject obj, float radius, float minTargetRadius, float maxTargetRadius)
        {
            var go = new GameObject("RFVolume");
            go.transform.parent = obj.transform;
            go.transform.localPosition = Vector3.zero;
            go.layer = LayerMask.NameToLayer("ReferenceFrameVolume");
            go.SetActive(false);

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            var rf = new ReferenceFrame(obj.GetAttachedOWRigidbody());
            rf._minSuitTargetDistance = minTargetRadius;
            rf._maxTargetDistance = maxTargetRadius;
            rf._autopilotArrivalDistance = radius;
            rf._autoAlignmentDistance = radius * 0.75f;
            rf._hideLandingModePrompt = false;
            rf._matchAngularVelocity = true;
            rf._minMatchAngularVelocityDistance = 70;
            rf._maxMatchAngularVelocityDistance = 400;
            rf._bracketsRadius = radius * 0.5f;
            
            var rfv = go.AddComponent<ReferenceFrameVolume>();
            rfv._referenceFrame = rf;
            rfv._minColliderRadius = minTargetRadius;
            rfv._maxColliderRadius = radius;
            rfv._isPrimaryVolume = false;
            rfv._isCloseRangeVolume = false;

            rf._useCenterOfMass = false;
            rf._localPosition = Vector3.zero;
            go.transform.localPosition = Vector3.zero;

            go.SetActive(true);
        }
    }
}
