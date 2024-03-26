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
        protected float health;
        protected float maxHealth;

        public abstract string GetNameKey();
        public virtual float GetHealth() => health;
        public virtual float GetMaxHealth() => maxHealth;
        public virtual bool IsDestroyed() => GetHealth() <= 0f;
        public abstract bool OnTakeDamage(IDamageSource source);

        public void Init(float maxHealth)
        {
            this.maxHealth = maxHealth;
            health = maxHealth;
        }
    }
}
