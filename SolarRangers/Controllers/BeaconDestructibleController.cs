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
    public class BeaconDestructibleController : AbstractDestructibleController
    {
        const float MAX_HEALTH = 100f;

        float health = MAX_HEALTH;
        GameObject beaconObj;

        public override float GetHealth() => health;
        public override float GetMaxHealth() => MAX_HEALTH;

        public override string GetNameKey() => "DestructibleBeacon";

        public static BeaconDestructibleController Spawn()
        {
            var beacon = new GameObject("Beacon").AddComponent<BeaconDestructibleController>();
            beacon.Init();
            return beacon;
        }

        public void Init()
        {
            health = MAX_HEALTH;

            beaconObj = ObjectUtils.SpawnPrefab("Beacon", transform).gameObject;
        }

        public override bool TakeDamage(IDamageSource source, float damage)
        {
            if (health <= 0f) return false;
            health = Mathf.Clamp(health - damage, 0f, GetMaxHealth());
            if (health <= 0f)
            {
                beaconObj.SetActive(false);
                ExplosionManager.MediumExplosion(SolarRangers.WorldCombatant, 50f, transform, transform.position);
            }
            return true;
        }
    }
}
