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
        GameObject beaconObj;

        public override string GetNameKey() => "DestructibleBeacon";

        public static BeaconDestructibleController Spawn()
        {
            var beacon = new GameObject("Beacon").AddComponent<BeaconDestructibleController>();
            beacon.Init();
            return beacon;
        }

        public void Init()
        {
            Init(MAX_HEALTH);

            beaconObj = ObjectUtils.SpawnPrefab("Beacon", transform).gameObject;
        }

        public override bool OnTakeDamage(IDamageSource source)
        {
            var damage = source.GetDamage();
            health = Mathf.Clamp(health - damage, 0f, GetMaxHealth());
            if (health <= 0f)
            {
                foreach (var probe in beaconObj.GetComponentsInChildren<SurveyorProbe>())
                {
                    probe.transform.parent = null;
                    probe.ExternalRetrieve(true);
                }
                beaconObj.SetActive(false);
                ExplosionManager.MediumExplosion(SolarRangers.WorldCombatant, 50f, transform, transform.position);
            }
            return true;
        }
    }
}
