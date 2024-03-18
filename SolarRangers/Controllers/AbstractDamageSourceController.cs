using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class AbstractDamageSourceController : MonoBehaviour, IDamageSource
    {
        protected ICombatant attacker;
        protected float damage;

        public ICombatant GetAttacker() => attacker;
        public Vector3 GetDamagePosition() => transform.position;
        public virtual InstantDamageType GetDamageType() => InstantDamageType.Impact;
        public HazardVolume.HazardType GetHazardType() => HazardVolume.HazardType.FIRE;

        public void Init(ICombatant attacker, float damage)
        {
            this.attacker = attacker;
            this.damage = damage;
        }

        public bool DealDamage(IDestructible target)
        {
            return target.TakeDamage(this, damage);
        }
    }
}
