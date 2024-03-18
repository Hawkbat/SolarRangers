using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public abstract class AbstractTurretController : MonoBehaviour, ITurret
    {
        protected ICombatant combatant;
        protected bool firing;
        protected float fireRate;
        protected float fireDelay;
        protected float damage;
        protected float fireTimer;

        public void Init(ICombatant combatant, float fireRate, float fireDelay, float damage)
        {
            this.combatant = combatant;
            this.fireRate = fireRate;
            this.fireDelay = fireDelay;
            this.damage = damage;
        }

        public bool GetFiringState() => firing;

        public void SetFiringState(bool firing)
        {
            var wasFiring = this.firing;
            this.firing = firing;
            if (firing && !wasFiring)
            {
                fireTimer = Mathf.Max(fireTimer, fireDelay);
                OnStartFiring();
            }
            if (!firing && wasFiring)
            {
                OnStopFiring();
            }
        }

        protected virtual void OnStartFiring() { }
        protected virtual void OnStopFiring() { }
        protected virtual void OnFire() { }

        protected virtual void Update()
        {
            if (fireTimer > 0f)
            {
                fireTimer -= Time.deltaTime;
            }
            if (firing && fireTimer <= 0f)
            {
                fireTimer += fireRate;
                OnFire();
            }
        }
    }
}
