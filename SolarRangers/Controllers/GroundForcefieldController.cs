using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    public class GroundForcefieldController : MonoBehaviour
    {
        bool active;
        BeaconDestructibleController beacon;
        GameObject fieldObj;

        public static GroundForcefieldController Spawn(BeaconDestructibleController beacon)
        {
            var field = new GameObject("GroundForcefield").AddComponent<GroundForcefieldController>();
            field.Init(beacon);
            return field;
        }

        public void Init(BeaconDestructibleController beacon)
        {
            this.beacon = beacon;

            fieldObj = ObjectUtils.SpawnPrefab("GroundForcefield", transform).gameObject;
            active = true;
        }

        void Update()
        {
            if (active && beacon.IsDestroyed())
            {
                active = false;
                foreach (var probe in fieldObj.GetComponentsInChildren<SurveyorProbe>())
                {
                    probe.transform.parent = null;
                    probe.ExternalRetrieve(true);
                }
                fieldObj.SetActive(false);
            }
        }
    }
}
