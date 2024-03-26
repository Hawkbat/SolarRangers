using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using SolarRangers.Managers;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public abstract class AbstractCombatantController : MonoBehaviour, ICombatant
    {
        public abstract string GetNameKey();
        public virtual bool IsPlayer() => false;
        public virtual bool CanTarget() => true;
        public virtual Vector3 GetReticlePosition() => transform.position;

        protected virtual void OnEnable()
        {
            CombatantManager.Track(this);
        }

        protected virtual void OnDisable()
        {
            CombatantManager.Untrack(this);
        }

        public virtual void OnHitLanded(IDamageSource source, IDestructible target, bool didDamage)
        {

        }
    }
}
