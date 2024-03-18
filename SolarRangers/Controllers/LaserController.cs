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
    public class LaserController : AbstractDamageSourceController
    {
        Vector3 start;
        Vector3 end;
        float speed;
        Vector3 size;

        Transform outerT;
        Transform innerT;
        Renderer outerRenderer;
        Renderer innerRenderer;
        OWAudioSource audioSrc;

        readonly RaycastHit[] hits = new RaycastHit[32];

        bool setUp;
        float dist;
        float t;

        public LaserController Init(ICombatant attacker, float damage, Vector3 start, Vector3 end, float speed, Vector3 size, float thickness, Material outerMat, Material innerMat)
        {
            Init(attacker, damage);
            this.start = start;
            this.end = end;
            this.speed = speed;
            this.size = size;

            SetUp();

            outerT.localScale = size;
            innerT.localScale = size - Vector3.one * thickness;

            outerRenderer.sharedMaterial = outerMat;
            innerRenderer.sharedMaterial = innerMat;
            outerRenderer.enabled = true;
            innerRenderer.enabled = true;

            dist = (end - start).magnitude;
            t = 0f;
            UpdatePosition(t);

            audioSrc.Play();

            enabled = true;

            return this;
        }

        void SetUp()
        {
            if (setUp) return;

            outerT = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            outerT.SetParent(transform, false);
            outerT.GetComponent<Collider>().enabled = false;
            outerRenderer = outerT.GetComponent<MeshRenderer>();

            innerT = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            innerT.SetParent(transform, false);
            innerT.GetComponent<Collider>().enabled = false;
            innerRenderer = innerT.GetComponent<MeshRenderer>();

            var mf = outerT.GetComponent<MeshFilter>();
            var mesh = mf.mesh;
            var tris = mesh.triangles;
            for (var t = 0; t < tris.Length; t += 3)
            {
                var t0 = tris[t + 0];
                var t1 = tris[t + 1];
                var t2 = tris[t + 2];
                tris[t + 0] = t2;
                tris[t + 1] = t1;
                tris[t + 2] = t0;
            }
            mesh.triangles = tris;
            mf.mesh = mesh;

            var soundObj = new GameObject("Sound");
            soundObj.transform.SetParent(transform, false);
            soundObj.SetActive(false);
            soundObj.AddComponent<AudioSource>();
            audioSrc = soundObj.AddComponent<OWAudioSource>();
            audioSrc.SetTrack(OWAudioMixer.TrackName.Environment_Unfiltered);
            soundObj.SetActive(true);
            audioSrc.AssignAudioLibraryClip(AudioType.Ghost_Chase);
            audioSrc.spatialBlend = 1f;
            audioSrc.minDistance = 15f;
            audioSrc.maxDistance = 500f;
            audioSrc.dopplerLevel = 0f;
            audioSrc.pitch = 2.5f;

            setUp = true;
        }

        Vector3 UpdatePosition(float t)
        {
            transform.position = Vector3.Lerp(start, end, t);
            transform.up = (end - start).normalized;
            transform.position += transform.up * size.y * 0.5f;
            return transform.position;
        }

        void Update()
        {
            var previousT = t;

            if (t < 1f)
            {
                var previousPos = UpdatePosition(t);
                t = Mathf.Clamp01(t + Time.deltaTime * speed / dist);
                var nextPos = UpdatePosition(t);
                var diff = nextPos - previousPos;

                int hitCount = Physics.RaycastNonAlloc(previousPos, diff.normalized, hits, diff.magnitude, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
                if (hitCount > 0)
                {
                    var closestHit = hits.Take(hitCount).OrderBy(h => h.distance).First();

                    var target = closestHit.collider.GetComponentInParent<IDestructible>();
                    if (target != null && DealDamage(target))
                    {
                        ExplosionManager.SmallExplosion(null, 0f, closestHit.transform, closestHit.point);
                    }
                    else
                    {
                        ExplosionManager.TinyExplosion(null, 0f, closestHit.transform, closestHit.point);
                    }
                    t = 1f;
                }
            }

            if (t >= 1f && previousT < 1f)
            {
                outerRenderer.enabled = false;
                innerRenderer.enabled = false;
            }

            if (t >= 1f && !audioSrc.isPlaying)
            {
                enabled = false;
                LaserManager.Recycle(this);
            }
        }
    }
}
