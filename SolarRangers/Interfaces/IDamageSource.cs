using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Interfaces
{
    public interface IDamageSource
    {
        public ICombatant GetAttacker();
        public float GetDamage();
        public Vector3 GetDamagePosition();
        public InstantDamageType GetDamageType();
        public HazardVolume.HazardType GetHazardType();
    }
}
