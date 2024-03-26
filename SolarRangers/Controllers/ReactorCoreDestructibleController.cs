using SolarRangers.Interfaces;
using SolarRangers.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class ReactorCoreDestructibleController : AbstractDestructibleController
    {
        const float MAX_HEALTH = 1000f;

        ReactorCombatantController reactor;
        GameObject coreSphere;
        Material coreMaterial;

        public override string GetNameKey() => "DestructibleReactorCore";
        public override float GetHealth() => health;
        public override float GetMaxHealth() => MAX_HEALTH;

        public void Init(ReactorCombatantController reactor)
        {
            Init(MAX_HEALTH);
            this.reactor = reactor;
        }

        public override bool OnTakeDamage(IDamageSource source)
        {
            if (!reactor.IsCoreExposed()) return false;
            if (IsDestroyed()) return false;
            var damage = source.GetDamage();
            health = Mathf.Max(health - damage, 0f);
            if (health <= 0f)
            {
                coreSphere.SetActive(false);
                ExplosionManager.LargeExplosion(reactor, 25f, transform, transform.position);
            }
            return true;
        }

        void Awake()
        {
            coreSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreSphere.transform.SetParent(transform, false);
            coreSphere.transform.localPosition = Vector3.zero;
            coreSphere.transform.localScale = Vector3.one;
            coreMaterial = new Material(Shader.Find("Unlit/Color"))
            {
                color = Color.cyan
            };
            coreSphere.GetComponent<MeshRenderer>().sharedMaterial = coreMaterial;
        }

        void Update()
        {
            coreMaterial.color = GetCoreColor();
            coreSphere.transform.localScale = Vector3.one * 80f * Mathf.Lerp(0.9f, 1.1f, Mathf.Abs(Mathf.Sin(Time.time)));
        }

        Color GetCoreColor()
        {
            var healthFraction = GetHealth() / GetMaxHealth();
            return Color.Lerp(Color.red, Color.cyan, healthFraction);
        }
    }
}
