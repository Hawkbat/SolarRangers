using SolarRangers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Objects
{
    public class TransientCombatant(
        string nameKey,
        bool isPlayer = false,
        bool canTarget = false,
        Vector3 position = default
    ) : ICombatant
    {
        public string GetNameKey() => nameKey;
        public bool IsPlayer() => isPlayer;
        public bool CanTarget() => canTarget;
        public Vector3 GetReticlePosition() => position;
    }
}
