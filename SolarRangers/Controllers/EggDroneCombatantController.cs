using SolarRangers.Interfaces;
using SolarRangers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class EggDroneCombatantController : AbstractCombatantController, IDestructible
    {
        const float DETECTION_DISTANCE = 1500f;
        const float MAX_HEALTH = 50f;

        float health;
        bool chasing;
        bool initialized;
        LaserTurretController turret;
        OWRigidbody rb;
        OWRigidbody planetBody;

        public override string GetNameKey() => "CombatantEggDrone";

        public float GetHealth() => health;
        public float GetMaxHealth() => MAX_HEALTH;
        public bool IsDestroyed() => health <= 0f;

        public static EggDroneCombatantController Spawn(GameObject planet)
        {
            var drone = ObjectUtils.SpawnPrefab("Drone", null, null).GetComponent<EggDroneCombatantController>();
            drone.gameObject.SetActive(true);
            drone.Init(planet);
            return drone;
        }

        public void Init(GameObject planet)
        {
            health = MAX_HEALTH;
            transform.parent = null;

            ReferenceFrameManager.Register(rb._referenceFrame, GetNameKey());

            planetBody = planet.GetAttachedOWRigidbody();

            turret = new GameObject("Turret").AddComponent<LaserTurretController>();
            turret.transform.SetParent(transform);
            turret.transform.localPosition = new Vector3(0f, 20f, 0f);
            turret.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);

            var fireRate = 2f;
            var fireDelay = 0f;
            var damage = 10f;
            var spread = 0.05f;
            var laserSpeed = 200f;
            var laserRange = 1000f;
            var laserSize = new Vector3(0.5f, 4f, 0.5f);
            var laserColor = Color.green;
            turret.Init(this, fireRate, fireDelay, damage, spread, laserSpeed, laserRange, laserSize, laserColor);

            initialized = true;
        }

        public bool TakeDamage(IDamageSource source, float damage)
        {
            if (health <= 0f) return false;
            health = Mathf.Clamp(health - damage, 0f, GetMaxHealth());
            if (health <= 0f)
            {
                ExplosionManager.LargeExplosion(this, 25f, transform, rb.GetWorldCenterOfMass());
                foreach (Transform child in transform)
                {
                    ObjectUtils.ConvertToPhysicsProp(child.gameObject, rb);
                }
            }
            return true;
        }

        void Awake()
        {
            rb = GetComponent<OWRigidbody>();
        }

        void Update()
        {
            if (!initialized) return;
            var isDead = IsDestroyed();
            var inRange = Vector3.Distance(transform.position, Locator.GetPlayerTransform().position) < DETECTION_DISTANCE;
            if (inRange && !chasing)
            {
                chasing = true;
            }
            turret.SetFiringState(!isDead && inRange);
        }

        void FixedUpdate()
        {
            if (!initialized) return;

            var relativeVelocity = planetBody.GetRelativeVelocity(rb);

            if (chasing && !IsDestroyed())
            {
                var moveAccel = 75f;
                var turnSpeed = 30f;
                var targetPos = Locator.GetPlayerBody().GetPosition();
                var diff = targetPos - rb.GetPosition();
                var cross = Vector3.Cross(transform.up, diff.normalized).normalized;
                rb.SetAngularVelocity(cross * turnSpeed * Mathf.Deg2Rad);
                rb.AddLocalAcceleration(Vector3.up * moveAccel);
            }

            var dragFactor = 0.5f;
            var acceleration = -0.5f * relativeVelocity * dragFactor;
            rb.AddAcceleration(acceleration);
            var angularVelocity = rb.GetAngularVelocity();
            var angularAcceleration = -0.5f * angularVelocity * dragFactor;
            rb.AddAngularAcceleration(angularAcceleration);
        }
    }
}
