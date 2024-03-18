using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers
{
    public static class AsyncUtils
    {
        public static IEnumerator DoUpdateTimer(float duration, Action<float> callback = null)
        {
            for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                callback?.Invoke(t);
                yield return null;
            }
            callback?.Invoke(1f);
        }

        public static IEnumerator DoObjectLerp(Transform transform, Vector3 targetPos, Quaternion targetRot, float duration, Action<float> callback = null)
        {
            var startPos = transform.position;
            var startRot = transform.rotation;
            yield return DoUpdateTimer(duration, t =>
            {
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                callback?.Invoke(t);
            });
        }
    }
}
