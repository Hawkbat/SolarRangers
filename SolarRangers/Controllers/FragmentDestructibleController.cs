using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class FragmentDestructibleController : MonoBehaviour, IDestructible
    {
        FragmentIntegrity fragmentIntegrity;
        public string GetNameKey() => "DestructibleFragment";
        public float GetHealth() => fragmentIntegrity._integrity;
        public float GetMaxHealth() => fragmentIntegrity._origIntegrity;
        public bool IsDestroyed() => fragmentIntegrity._integrity <= 0f;

        public static FragmentDestructibleController Merge(Component c)
        {
            var fragment = c.gameObject.AddComponent<FragmentDestructibleController>();
            fragment.Init();
            return fragment;
        }

        public void Init()
        {

        }

        public bool OnTakeDamage(IDamageSource source)
        {
            if (source is MeteorProjectileController) return false; // Meteor handles damage itself
            var damage = source.GetDamage();
            fragmentIntegrity.AddDamage(damage);
            return true;
        }

        void Awake()
        {
            fragmentIntegrity = GetComponent<FragmentIntegrity>();
        }
    }
}
