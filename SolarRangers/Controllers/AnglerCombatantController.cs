using SolarRangers.Interfaces;
using SolarRangers.Managers;
using Steamworks;
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
        const float MECHA_DETECTION_RANGE = 2000f;

        const float INITIAL_ACCELERATION = 40f;
        const float INITIAL_ARRIVAL_DISTANCE = 100f;
        const float INITIAL_CHASE_SPEED = 75f;
        const float INITIAL_ESCAPE_DISTANCE = 400f;
        const float INITIAL_INVESTIGATE_SPEED = 20f;
        const float INITIAL_PURSUE_DISTANCE = 300f;
        static Vector3 INITIAL_MOUTH_OFFSET = new(0f, 2f, 60f);

        const float HEALTH_FACTOR = 500f;
        const float STUN_THRESHOLD = 100f;
        const float STUN_DURATION = 3f;
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
        List<Transform> cybernetics = [];

        public override string GetNameKey() => isMecha ? "CombatantMechaAngler" : "CombatantAngler";
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
            if (!angler) Awake();
            if (scale != 1f)
            {
                var gradualScale = Mathf.Sqrt(scale);
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
                var bodyImplant = ObjectUtils.SpawnPrefab("Angler_Implant_body02", transform, "Beast_Anglerfish/B_angler_root/B_angler_body01/B_angler_body02");
                cybernetics.Add(bodyImplant);
                var engineImplant = ObjectUtils.SpawnPrefab("Angler_Implant_engine", transform, "Beast_Anglerfish/B_angler_root/B_angler_body01");
                cybernetics.Add(engineImplant);
                var jawImplant = ObjectUtils.SpawnPrefab("Angler_Implant_jaw", transform, "Beast_Anglerfish/B_angler_root/B_angler_body01/B_angler_body02/B_angler_jaw");
                cybernetics.Add(jawImplant);
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
            if (damage >= STUN_THRESHOLD)
            {
                Stun(STUN_DURATION);
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
                foreach (var c in cybernetics)
                {
                    foreach (Transform t in c)
                    {
                        foreach (MeshFilter mf in t.GetComponentsInChildren<MeshFilter>())
                        {
                            var mc = mf.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                            mc.convex = true;
                            mf.gameObject.AddComponent<OWCollider>();
                        }
                        ObjectUtils.ConvertToPhysicsProp(t.gameObject, angler._anglerBody);
                    }
                }
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

        void Update()
        {
            if (isMecha && !IsDestroyed() && angler._currentState != AnglerfishController.AnglerState.Stunned)
            {
                var dist = Vector3.Distance(transform.position, Locator.GetPlayerTransform().position);
                if (dist < MECHA_DETECTION_RANGE && angler._currentState != AnglerfishController.AnglerState.Chasing)
                {
                    angler._targetBody = Locator.GetPlayerBody();
                    angler.ChangeState(AnglerfishController.AnglerState.Chasing);
                }
            }
        }
    }
}
