using SolarRangers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolarRangers.Managers
{
    public class CombatantManager : AbstractManager<CombatantManager>
    {
        public const float SIGNAL_DETECT_RADIUS = 1000f;

        readonly List<ICombatant> combatants = [];

        readonly Dictionary<ICombatant, AudioSignal> targetSignals = [];
        readonly Queue<AudioSignal> targetSignalPool = [];
        SignalFrequency combatFrequency;

        public static IEnumerable<ICombatant> GetCombatants() => Instance.combatants;
        public static SignalFrequency GetCombatFrequency() => Instance.combatFrequency;

        public static void Track(ICombatant combatant)
        {
            if (!Instance) return;
            Instance.combatants.Add(combatant);
            Instance.GetOrAddSignal(combatant);
        }

        public static void Untrack(ICombatant combatant)
        {
            if (!Instance) return;
            Instance.combatants.Remove(combatant);
            Instance.RemoveSignal(combatant);
        }

        AudioSignal GetOrAddSignal(ICombatant combatant)
        {
            if (!targetSignals.TryGetValue(combatant, out var signal))
            {
                if (!targetSignalPool.TryDequeue(out signal))
                {
                    signal = SolarRangers.NewHorizons.SpawnSignal(SolarRangers.Instance, gameObject, AudioType.None.ToString(), combatant.GetNameKey(), "SignalFrequencyCombat", 1f, 100f, 50f);
                    combatFrequency = signal._frequency;
                }
                targetSignals.Add(combatant, signal);
            }
            return signal;
        }

        void RemoveSignal(ICombatant combatant)
        {
            if (targetSignals.TryGetValue(combatant, out var signal))
            {
                targetSignalPool.Enqueue(signal);
                signal.SetSignalActivation(false);
                targetSignals.Remove(combatant);
            }
        }

        void LateUpdate()
        {
            SignalscopeUI.s_distanceTextThreshold = SolarRangers.CombatModeActive ? float.PositiveInfinity : 0.8f;
            var scope = Locator.GetToolModeSwapper().GetSignalScope();
            foreach (var combatant in GetCombatants())
            {
                var signal = GetOrAddSignal(combatant);
                var canTarget = !combatant.IsPlayer() && combatant.CanTarget();
                var inRange = Vector3.Distance(scope.transform.position, combatant.GetReticlePosition()) < SIGNAL_DETECT_RADIUS;
                signal.SetSignalActivation(canTarget && inRange);
                signal.transform.position = combatant.GetReticlePosition();
            }
        }
    }
}
