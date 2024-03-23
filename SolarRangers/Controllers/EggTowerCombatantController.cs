using SolarRangers.Interfaces;
using SolarRangers.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class EggTowerCombatantController : AbstractCombatantController, IDestructible
    {
        const float DETECTION_DISTANCE = 1500f;
        const float TURRET_ROTATE_SPEED = 30f;
        const float MAX_HEALTH = 100f;

        float health = MAX_HEALTH;
        bool hasDied = false;

        MeteorTurretController turret;
        GameObject eggObj;
        GameObject towerObj;
        GameObject towerCollisionObj;
        GameObject turretObj;

        public override string GetNameKey() => "CombatantEgg";
        public override bool CanTarget() => !IsDestroyed();
        public override Vector3 GetReticlePosition() => eggObj.transform.position;
        public float GetHealth() => health;
        public float GetMaxHealth() => MAX_HEALTH;
        public bool IsDestroyed() => hasDied;

        public static EggTowerCombatantController Spawn()
        {
            var egg = new GameObject("Egg").AddComponent<EggTowerCombatantController>();
            egg.Init();
            return egg;
        }

        public void Init()
        {

        }

        public bool TakeDamage(IDamageSource source, float damage)
        {
            if (hasDied) return false;
            health = Mathf.Max(health - damage, 0f);
            if (health <= 0f)
            {
                hasDied = true;
                StartCoroutine(OnDie(source));
            }
            return true;
        }

        IEnumerator OnDie(IDamageSource source)
        {
            var parentBody = transform.GetAttachedOWRigidbody();
            ExplosionManager.SmallExplosion(this, 50f, transform, transform.position);
            yield return new WaitForSeconds(0.25f);
            ExplosionManager.SmallExplosion(this, 25f, transform, Vector3.Lerp(transform.position, eggObj.transform.position, 0.5f));
            yield return new WaitForSeconds(0.25f);
            ExplosionManager.MediumExplosion(this, 25f, transform, eggObj.transform.position);
            yield return new WaitForSeconds(0.15f);
            towerObj.SetActive(false);
            towerCollisionObj.SetActive(false);
            turretObj.SetActive(false);
            eggObj.GetComponentInChildren<Animator>().speed = 0f;
            var owBody = ObjectUtils.ConvertToPhysicsProp(eggObj, parentBody);
            ReferenceFrameManager.Register(owBody._referenceFrame, GetNameKey());
            var launchDir = ((eggObj.transform.position - source.GetDamagePosition()).normalized + transform.up).normalized;
            owBody.AddVelocityChange(launchDir * 10f);
        }

        void Awake()
        {
            var towerOffset = 53f;
            
            var eggPath = "CaveTwin_Body/Sector_CaveTwin/Sector_NorthHemisphere/Sector_NorthSurface/Sector_Lakebed/Interactables_Lakebed/Traveller_HEA_Chert";
            eggObj = ObjectUtils.Spawn(transform, eggPath);
            eggObj.transform.localPosition = new Vector3(0f, towerOffset, 0f);

            var drum = eggObj.transform.Find("Traveller_HEA_Chert_ANIM_Chatter_Chipper/NewDrum:pCylinder1");
            drum.gameObject.SetActive(false);

            var convoZone = eggObj.transform.Find("ConversationZone_Chert");
            Destroy(convoZone.gameObject);

            var towerPath = "TowerTwin_Body/Sector_TowerTwin/Geometry_TowerTwin/OtherComponentsGroup/ControlledByProxy_Arch/Structures/OtherComponentsGroup/Structure_NOM_TimeLoopTower_Ext_NORTH";
            towerObj = ObjectUtils.Spawn(transform, towerPath);
            towerObj.transform.localPosition = new Vector3(0f, -15.34f, 0f);
            towerObj.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
            towerObj.transform.localScale = new Vector3(0.4f, 1f, 1f);
            towerObj.transform.Find("higherPlatform").gameObject.SetActive(false);
            towerObj.transform.Find("lowerPlatform").gameObject.SetActive(false);
            towerObj.transform.Find("SolarPanel_Structure_rebent_Panels").gameObject.SetActive(false);
            towerObj.transform.Find("staircaseOnTheBottom").gameObject.SetActive(false);

            towerCollisionObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            towerCollisionObj.transform.SetParent(transform, false);
            towerCollisionObj.transform.localScale = new Vector3(7.5f, towerOffset * 0.5f, 7.5f);
            towerCollisionObj.transform.localPosition = new Vector3(0f, towerOffset * 0.5f, 0f);
            towerCollisionObj.GetComponent<MeshRenderer>().enabled = false;

            turretObj = new GameObject("Turret");
            turretObj.transform.SetParent(transform, false);
            turretObj.transform.localPosition = new Vector3(0f, towerOffset + 0.5f, 0f);

            var turretDisplayPath = "CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_GravityCannon/Geometry_GravityCannon/ControlledByProxy_Arch/Structure_NOM_GravityCannon_HT";
            var turretDisplayObj = ObjectUtils.Spawn(turretObj.transform, turretDisplayPath);
            turretDisplayObj.transform.localPosition = Vector3.zero;
            turretDisplayObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            turretDisplayObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            turret = new GameObject("Turret").AddComponent<MeteorTurretController>();
            turret.transform.SetParent(turretObj.transform, false);
            turret.transform.localPosition = new Vector3(0f, 0f, 200f);
            turret.transform.localEulerAngles = Vector3.zero;

            var fireRate = 10f;
            var fireDelay = 1f;
            var damage = 15f;
            /*
            var spread = 0.05f;
            var laserSpeed = 200f;
            var laserRange = 1000f;
            var laserSize = new Vector3(0.5f, 4f, 0.5f);
            var laserColor = Color.green;
            turret.Init(this, fireRate, fireDelay, damage, spread, laserSpeed, laserRange, laserSize, laserColor);
            */
            var meteorSize = 0.5f;
            var meteorSpeed = 250f;
            turret.Init(this, fireRate, fireDelay, damage, meteorSize, meteorSpeed);
        }

        void Update()
        {
            if (hasDied)
            {
                turret.SetFiringState(false);
                return;
            }

            var playerT = Locator.GetPlayerTransform();
            var turretT = turretObj.transform;
            var inRange = Vector3.Distance(playerT.position, turretT.position) <= DETECTION_DISTANCE;
            
            if (inRange)
            {
                var lookRot = Quaternion.LookRotation(playerT.position - turretT.position, transform.up);
                turretT.rotation = Quaternion.RotateTowards(turretT.rotation, lookRot, Time.deltaTime * TURRET_ROTATE_SPEED);

                /*var lookPlane = new Plane(transform.up, transform.position);
                var lookPoint = lookPlane.ClosestPointOnPlane(playerT.position);
                var lookRot = Quaternion.LookRotation(lookPoint - transform.position, transform.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, Time.deltaTime * TURRET_ROTATE_SPEED);

                var turretPlane = new Plane(transform.right, turretT.position);
                var turretPoint = turretPlane.ClosestPointOnPlane(playerT.position);
                var turretRot = Quaternion.LookRotation(turretPoint - turretT.position, transform.up);
                turretT.rotation = Quaternion.RotateTowards(turretT.rotation, turretRot, Time.deltaTime * TURRET_ROTATE_SPEED);*/
            }

            turret.SetFiringState(inRange);
        }
    }
}
