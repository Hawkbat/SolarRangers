using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public abstract class AbstractDestructibleController : MonoBehaviour, IDestructible
    {
        public abstract string GetNameKey();
        public abstract float GetHealth();
        public abstract float GetMaxHealth();
        public virtual bool IsDestroyed() => GetHealth() <= 0f;
        public abstract bool TakeDamage(IDamageSource source, float damage);
    }
}
