using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class ShipWingController : MonoBehaviour
    {
        ShipCombatantController ship;
        bool left;
        bool lower;

        Quaternion openRotation;
        Quaternion closedRotation;

        LaserTurretController turretController;

        bool setUp;

        public ShipWingController Init(ShipCombatantController ship, bool left, bool lower)
        {
            this.ship = ship;
            this.left = left;
            this.lower = lower;

            gameObject.name = $"Wing_{(lower ? "L" : "U")}{(left ? "L" : "R")}";

            SetUp();

            transform.SetParent(Locator.GetShipTransform(), false);
            transform.localPosition = new Vector3(left ? -2f : 2f, lower ? -0.25f : 4.75f, 0f);

            closedRotation = Quaternion.Euler(0f, 180f + 25f * (left ? -1f : 1f), (left ? 90f : 270f) + 20f * (lower == left ? 1f : -1f));
            openRotation = closedRotation * Quaternion.Euler(0f, 0f, 45f * (left == lower ? -1f : 1f));

            UpdateRotation(0f);

            return this;
        }

        public void UpdateRotation(float t)
        {
            transform.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
        }

        public void SetFiringState(bool firing)
        {
            var shipT = Locator.GetShipTransform();
            var convergeAt = 500f;
            var planetRuleset = Locator.GetShipDetector().GetComponent<RulesetDetector>().GetPlanetoidRuleset();
            if (planetRuleset != null)
            {
                var shipP = shipT.position;
                var planetP = planetRuleset.transform.root.position;
                var dot = Mathf.Max(0f, Vector3.Dot(shipT.forward, (planetP - shipP).normalized));
                var dist = Vector3.Distance(shipP, planetP);
                var altitude = Mathf.Max(100f, Mathf.Abs(planetRuleset.GetAltitude(dist)));
                convergeAt = Mathf.Lerp(500f, altitude, dot);
            }
            var target = shipT.position + shipT.forward * convergeAt;
            turretController.transform.forward = (target - turretController.transform.position).normalized;
            turretController.SetFiringState(firing);
        }

        void SetUp()
        {
            if (setUp) return;

            var shipObj = Locator.GetShipBody().gameObject;
            var wingPath = "Prefab_IP_AlarmBell/Structure_IP_AlarmBell/Arm_L_pivot/arm_L";
            var wingScale = 3f;

            var wingObj = ObjectUtils.Spawn(transform, wingPath);
            wingObj.transform.localPosition = new Vector3(0f, -4f, -1.6f);
            wingObj.transform.localEulerAngles = new Vector3(0f, 330f, 0f);
            wingObj.transform.localScale = Vector3.one * wingScale;

            var collider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            collider.transform.SetParent(transform, false);
            collider.transform.localPosition = new Vector3(0f, -3.5f, -1.5f);
            collider.transform.localEulerAngles = new Vector3(25.5f, 0f, 0f);
            collider.transform.localScale = new Vector3(0.5f, 7f, 0.5f);
            collider.GetComponent<MeshRenderer>().enabled = false;

            var turret = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            turret.transform.SetParent(transform, false);
            turret.transform.localPosition = new Vector3(0f, -6.25f, -3.5f);
            turret.transform.localScale = Vector3.one * 0.5f;
            turret.GetComponent<MeshRenderer>().enabled = false;
            turret.transform.forward = shipObj.transform.forward;

            var fireRate = 0.5f;
            var fireDelay = 0f + (!left ? 0.25f : 0f) + (!lower ? 0.125f : 0f);
            var damage = 10f;
            var spread = 0f;
            var laserSpeed = 1000f;
            var laserRange = 1000f;
            var laserSize = new Vector3(0.5f, 4f, 0.5f);
            var laserColor = Color.red;

            turretController = turret.AddComponent<LaserTurretController>();
            turretController.Init(ship, fireRate, fireDelay, damage, spread, laserSpeed, laserRange, laserSize, laserColor);

            setUp = true;
        }
    }
}
