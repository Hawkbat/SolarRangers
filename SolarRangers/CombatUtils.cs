using SolarRangers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarRangers
{
    public static class CombatUtils
    {
        public static bool ResolveHit(IDamageSource source, IDestructible target)
        {
            var didDamage = target != null && !target.IsDestroyed() && target.OnTakeDamage(source);
            source.GetAttacker()?.OnHitLanded(source, target, didDamage);
            return didDamage;
        }
    }
}
