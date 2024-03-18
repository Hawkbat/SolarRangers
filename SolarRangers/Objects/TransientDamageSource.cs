using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Objects
{
    public class TransientDamageSource(
        ICombatant attacker,
        Vector3 damagePosition,
        InstantDamageType damageType = InstantDamageType.Impact,
        HazardVolume.HazardType hazardType = HazardVolume.HazardType.FIRE
    ) : IDamageSource
    {
        public ICombatant GetAttacker() => attacker;
        public Vector3 GetDamagePosition() => damagePosition;
        public InstantDamageType GetDamageType() => damageType;
        public HazardVolume.HazardType GetHazardType() => hazardType;
    }
}
