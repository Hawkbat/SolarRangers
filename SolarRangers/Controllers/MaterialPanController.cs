using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Controllers
{
    internal class MaterialPanController : MonoBehaviour
    {
        Material materialRef;
        public int materialIndex;
        public Vector2 scrollSpeed;
        void Awake() {
            materialRef = GetComponent<MeshRenderer>().sharedMaterials[materialIndex];
        }
        public void Update()
        {
            materialRef.mainTextureOffset += scrollSpeed * Time.deltaTime;
        }
    }
}
