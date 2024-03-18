using SolarRangers.Interfaces;
using SolarRangers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class LaserTurretController : AbstractTurretController
    {
        float spread;
        float laserSpeed;
        float laserRange;
        Vector3 laserSize;
        Color laserColor;

        public void Init(ICombatant combatant, float fireRate, float fireDelay, float damage, float spread, float laserSpeed, float laserRange, Vector3 laserSize, Color laserColor)
        {
            Init(combatant, fireRate, fireDelay, damage);
            this.spread = spread;
            this.laserSpeed = laserSpeed;
            this.laserRange = laserRange;
            this.laserSize = laserSize;
            this.laserColor = laserColor;
        }

        protected override void OnFire()
        {
            var dir = transform.forward;
            
            if (spread > 0f)
            {
                var perpDir = Vector3.Cross(UnityEngine.Random.insideUnitSphere, dir).normalized;
                dir = Vector3.Slerp(dir, perpDir, UnityEngine.Random.value * spread);
            }

            var start = transform.position;
            var end = transform.position + dir * laserRange;
            LaserManager.Fire(combatant, damage, start, end, laserSpeed, laserSize, laserColor);
        }
    }
}
