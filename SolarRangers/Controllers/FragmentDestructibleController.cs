using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class FragmentDestructibleController : AbstractDestructibleController
    {
        FragmentIntegrity fragmentIntegrity;
        public override string GetNameKey() => "DestructibleFragment";
        public override float GetHealth() => fragmentIntegrity._integrity;
        public override float GetMaxHealth() => fragmentIntegrity._origIntegrity;

        public static FragmentDestructibleController Merge(Component c)
        {
            var fragment = c.gameObject.AddComponent<FragmentDestructibleController>();
            fragment.Init();
            return fragment;
        }

        public void Init()
        {

        }

        public override bool TakeDamage(IDamageSource source, float damage)
        {
            if (source is MeteorProjectileController) return false; // Meteor handles damage itself
            if (fragmentIntegrity._integrity <= 0f) return false;
            fragmentIntegrity.AddDamage(damage);
            return true;
        }

        void Awake()
        {
            fragmentIntegrity = GetComponent<FragmentIntegrity>();
        }
    }
}
