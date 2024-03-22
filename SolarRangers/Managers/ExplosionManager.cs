using SolarRangers.Controllers;
using SolarRangers.Interfaces;
using SolarRangers.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class ExplosionManager : AbstractManager<ExplosionManager>
    {
        GameObject shipExplosionPrefab;
        readonly Queue<ExplosionController> explosionPool = [];

        public void Awake()
        {
            shipExplosionPrefab = Locator.GetShipBody().GetComponent<ShipDamageController>()._explosion.gameObject;
        }

        public static void TinyExplosion(ICombatant attacker, float damage, Transform parent, Vector3 position)
        {
            Explode(attacker, damage, parent, position, 0.2f, AudioType.ImpactLowSpeed);
        }

        public static void SmallExplosion(ICombatant attacker, float damage, Transform parent, Vector3 position)
        {
            Explode(attacker, damage, parent, position, 0.25f, AudioType.ShipDamageShipExplosion);
        }

        public static void MediumExplosion(ICombatant attacker, float damage, Transform parent, Vector3 position)
        {
            Explode(attacker, damage, parent, position, 1f, AudioType.ShipDamageShipExplosion);
        }

        public static void LargeExplosion(ICombatant attacker, float damage, Transform parent, Vector3 position)
        {
            Explode(attacker, damage, parent, position, 5f, AudioType.BH_MeteorImpact);
        }

        static void Explode(ICombatant attacker, float damage, Transform parent, Vector3 position, float size, AudioType sound)
        {
            if (!Instance.explosionPool.TryDequeue(out var explosion))
            {
                explosion = Instantiate(Instance.shipExplosionPrefab).GetComponent<ExplosionController>();
            }

            explosion.transform.SetParent(null);
            explosion.transform.localScale = Vector3.one * 20f * size;
            explosion.transform.parent = parent;
            explosion.transform.position = position;
            explosion.enabled = true;
            SolarRangers.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                //explosion._forceVolume.SetVolumeActivation(true);
                if (Vector3.Distance(explosion.transform.position, Locator.GetPlayerTransform().position) < explosion.transform.localScale.x * explosion.GetComponent<SphereCollider>().radius)
                {
                    RumbleManager.PulseShipExplode();
                }
                explosion._timer = 0f;
                explosion._renderer.enabled = true;
                explosion._light.enabled = true;

                var soundT = explosion.transform.Find("Sound");
                var soundObj = soundT ? soundT.gameObject : null;
                if (!soundObj)
                {
                    soundObj = new GameObject("Sound");
                    soundObj.transform.SetParent(explosion.transform, false);
                }
                soundObj.SetActive(false);
                var audioSource = soundObj.GetAddComponent<AudioSource>();
                var owAudioSource = soundObj.GetAddComponent<OWAudioSource>();
                owAudioSource._audioSource = audioSource;
                owAudioSource.SetTrack(OWAudioMixer.TrackName.Environment_Unfiltered);
                soundObj.SetActive(true);
                owAudioSource.maxDistance = 500f;
                owAudioSource.minDistance = 15f;
                owAudioSource.dopplerLevel = 0f;
                owAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                owAudioSource.spatialBlend = 1f;
                owAudioSource.AssignAudioLibraryClip(sound);
                owAudioSource.Play();
                if (owAudioSource.clip.length > explosion._length)
                {
                    explosion._length = owAudioSource.clip.length;
                }
                explosion._playing = true;
                explosion.enabled = true;

                if (damage > 0f)
                {
                    var damageSource = new TransientDamageSource(attacker, position);
                    var colliders = Physics.OverlapSphere(position, size * 10f, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
                    var targets = colliders.Select(c => c.GetComponentInParent<IDestructible>()).Distinct().Where(t => t != null);
                    foreach (var target in targets)
                    {
                        target.TakeDamage(damageSource, damage);
                    }
                }
            });
        }

        public static void Recycle(ExplosionController explosion)
        {
            explosion.enabled = false;
            explosion.transform.SetParent(null);
            Instance.explosionPool.Enqueue(explosion);
        }
    }
}
