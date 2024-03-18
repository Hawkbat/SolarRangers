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
    public class MeteorTurretController : AbstractTurretController
    {
        float meteorSize;
        float meteorSpeed;

        public void Init(ICombatant combatant, float fireRate, float fireDelay, float damage, float meteorSize, float meteorSpeed)
        {
            Init(combatant, fireRate, fireDelay, damage);
            this.meteorSize = meteorSize;
            this.meteorSpeed = meteorSpeed;
        }

        protected override void OnFire()
        {
            MeteorManager.Launch(combatant, damage, meteorSize, meteorSpeed, transform);
        }
    }
}
