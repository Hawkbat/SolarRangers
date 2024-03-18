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
        const float MAX_HEALTH = 25f;

        float health;

        public override string GetNameKey() => "CombatantEggDrone";

        public float GetHealth() => health;
        public float GetMaxHealth() => MAX_HEALTH;
        public bool IsDestroyed() => health <= 0f;

        public static EggDroneCombatantController Spawn()
        {
            var drone = new GameObject("EggDrone").AddComponent<EggDroneCombatantController>();
            drone.Init();
            return drone;
        }

        public void Init()
        {
            health = MAX_HEALTH;
        }

        public bool TakeDamage(IDamageSource source, float damage)
        {
            if (health <= 0f) return false;
            health = Mathf.Clamp(health - damage, 0f, GetMaxHealth());
            if (health <= 0f)
            {
                ExplosionManager.LargeExplosion(this, 25f, transform, Vector3.zero);
            }
            return true;
        }
    }
}
