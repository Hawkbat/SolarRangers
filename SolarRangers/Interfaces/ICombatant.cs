
using UnityEngine;

namespace SolarRangers.Interfaces
{
    public interface ICombatant
    {
        string GetNameKey();
        bool IsPlayer();
        bool CanTarget();
        Vector3 GetReticlePosition();
    }
}