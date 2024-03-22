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
    public class AnglerCombatantController : AbstractCombatantController, IDestructible
    {
        const float INITIAL_ACCELERATION = 40f;
        const float INITIAL_ARRIVAL_DISTANCE = 100f;
        const float INITIAL_CHASE_SPEED = 75f;
        const float INITIAL_ESCAPE_DISTANCE = 400f;
        const float INITIAL_INVESTIGATE_SPEED = 20f;
        const float INITIAL_PURSUE_DISTANCE = 300f;
        static Vector3 INITIAL_MOUTH_OFFSET = new(0f, 2f, 60f);

        const float HEALTH_FACTOR = 500f;
        const float STUN_CHANCE_FACTOR = 1f / 200f;
        const float STUN_DURATION_FACTOR = 1f / 25f;
        const float DEATH_EXPLOSION_LARGE_THRESHOLD = 0.75f;
        const float DEATH_EXPLOSION_MEDIUM_THRESHOLD = 0.25f;
        const float EAT_PLAYER_SCALE_THRESHOLD = 0.5f;

        float scale;
        bool isMecha;
        float health;
        float maxHealth;
        AnglerfishController angler;
        AnglerfishAnimController anglerAnim;
        AnglerfishAudioController anglerAudio;

        public override string GetNameKey() => "CombatantAngler";
        public override bool CanTarget() => !IsDestroyed();
        public float GetHealth() => health;
        public float GetMaxHealth() => maxHealth;
        public bool IsDestroyed() => health <= 0f;
        public bool CanEatPlayer() => scale >= EAT_PLAYER_SCALE_THRESHOLD;

        public static AnglerCombatantController Spawn(Transform parent, float scale, bool isMecha)
        {
            var anglerObj = ObjectUtils.Spawn(parent, "Anglerfish_Body");
            var angler = anglerObj.GetAddComponent<AnglerCombatantController>();
            angler.Init(scale, isMecha);
            return angler;
        }

        public static AnglerCombatantController Merge(Component c)
        {
            var angler = c.gameObject.AddComponent<AnglerCombatantController>();
            angler.Init(angler.transform.localScale.x, false);
            return angler;
        }

        public void Init(float scale, bool isMecha)
        {
            this.scale = scale;
            this.isMecha = isMecha;
            health = maxHealth = HEALTH_FACTOR * scale;
            transform.localScale = Vector3.one * scale;
            var gradualScale = Mathf.Sqrt(scale);
            if (!angler) Awake();
            if (scale != 1f)
            {
                angler._acceleration = INITIAL_ACCELERATION * gradualScale;
                angler._arrivalDistance = INITIAL_ARRIVAL_DISTANCE * gradualScale;
                angler._chaseSpeed = INITIAL_CHASE_SPEED * gradualScale;
                angler._escapeDistance = INITIAL_ESCAPE_DISTANCE * gradualScale;
                angler._investigateSpeed = INITIAL_INVESTIGATE_SPEED * gradualScale;
                angler._pursueDistance = INITIAL_PURSUE_DISTANCE * gradualScale;
                angler._mouthOffset = INITIAL_MOUTH_OFFSET * scale;
            }

            ReferenceFrameManager.Register(angler._anglerBody._referenceFrame, GetNameKey());

            if (isMecha)
            {
                // spawn cyborg attachments
            }
        }

        public void Stun(float duration)
        {
            angler._stunTimer = Mathf.Max(angler._stunTimer, duration);
            angler._anglerBody.AddAngularVelocityChange(UnityEngine.Random.onUnitSphere * 0.02f);
            if (angler.GetAnglerState() != AnglerfishController.AnglerState.Stunned)
            {
                anglerAudio._oneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);
                angler.ChangeState(AnglerfishController.AnglerState.Stunned);
            }
        }

        public bool TakeDamage(IDamageSource source, float damage)
        {
            if (IsDestroyed()) return false;
            health = Mathf.Max(health - damage, 0f);
            var stunChance = damage * STUN_CHANCE_FACTOR;
            if (UnityEngine.Random.value < stunChance)
            {
                Stun(damage * STUN_DURATION_FACTOR);
            }
            if (health <= 0f)
            {
                StartCoroutine(OnDie());
            }
            return true;
        }

        IEnumerator OnDie()
        {
            if (isMecha)
            {
                if (scale > DEATH_EXPLOSION_LARGE_THRESHOLD)
                {
                    ExplosionManager.LargeExplosion(this, 50f, transform, transform.position);
                }
                else if (scale > DEATH_EXPLOSION_MEDIUM_THRESHOLD)
                {
                    ExplosionManager.MediumExplosion(this, 25f, transform, transform.position);
                }
                else
                {
                    ExplosionManager.SmallExplosion(this, 15f, transform, transform.position);
                }
            }
            angler.ChangeState(AnglerfishController.AnglerState.Stunned);
            angler.enabled = false;
            anglerAudio._oneShotSource.PlayOneShot(AudioType.DBAnglerfishOpeningMouth);
            yield return new WaitForSeconds(0.5f);
            anglerAnim.enabled = false;
            anglerAnim._animator.enabled = false;
            angler._anglerBody.AddAngularVelocityChange(UnityEngine.Random.onUnitSphere * 0.25f);
        }

        void Awake()
        {
            angler = GetComponent<AnglerfishController>();
            anglerAnim = GetComponentInChildren<AnglerfishAnimController>();
            anglerAudio = GetComponentInChildren<AnglerfishAudioController>();
        }

    }
}
