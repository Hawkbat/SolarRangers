using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarRangers.Interfaces;
using SolarRangers.Managers;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class MeteorProjectileController : AbstractDamageSourceController, IDestructible
    {
        const float MAX_LIFETIME = 15f;

        float size;
        float lifeTime;
        bool isCustom;
        MeteorController meteor;

        public string GetNameKey() => "DestructibleMeteor";
        public float GetHealth() => (1f - meteor.heat) * 100f;
        public float GetMaxHealth() => 100f;
        public bool IsDestroyed() => meteor.hasImpacted;

        public void Init(ICombatant attacker, float damage, float size, float speed, Transform launcher)
        {
            Init(attacker, damage);
            this.size = size;

            lifeTime = 0f;
            isCustom = true;

            transform.localScale = Vector3.one * size;

            var parentBody = launcher.GetAttachedOWRigidbody();
            var pointVelocity = parentBody ? parentBody.GetPointVelocity(launcher.position) : Vector3.zero;

            meteor.Initialize(launcher, null, null);
            var meteorRadius = 16f * size;
            var position = launcher.position + launcher.forward * (meteorRadius + 0.5f);
            var rotation = launcher.rotation;
            var velocity = pointVelocity + launcher.forward * speed;
            var angularVelocity = launcher.forward * 2f;
            meteor.Launch(null, position, rotation, velocity, angularVelocity);
            meteor._minDamage = meteor._maxDamage = damage;
        }

        public void OnImpact()
        {
            if (damage > 0f)
            {
                var colliders = Physics.OverlapSphere(transform.position, (16f + 4f) * size, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
                var targets = colliders.Select(c => c.GetComponentInParent<IDestructible>()).Distinct().Where(t => t != null);
                foreach (var target in targets)
                {
                    CombatUtils.ResolveHit(this, target);
                }
            }
        }

        public void OnSuspend()
        {
            MeteorManager.Recycle(this);
        }

        public bool OnTakeDamage(IDamageSource source)
        {
            if (!meteor.hasLaunched || meteor.hasImpacted) return false;
            var damage = source.GetDamage();
            meteor._heat += damage / 100f;
            if (meteor._heat > 1f)
            {
                meteor.Impact(gameObject, transform.position, Vector3.zero);
            }
            return true;
        }

        void Awake()
        {
            meteor = GetComponent<MeteorController>();
        }

        void Update()
        {
            if (!isCustom || !meteor.hasLaunched || meteor.hasImpacted) return;
            lifeTime += Time.deltaTime;
            if (lifeTime > MAX_LIFETIME - 5f)
            {
                meteor._heat += Time.deltaTime / 5f;
                if (meteor._heat > 1f)
                {
                    meteor.Impact(gameObject, transform.position, Vector3.zero);
                }
            }
        }
    }
}
