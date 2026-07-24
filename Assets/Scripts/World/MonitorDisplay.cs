using Countdown.Core;
using Countdown.Runtime;
using TMPro;
using UnityEngine;

namespace Countdown.World
{
    // Ambient vitals readout - not interactive. This is the rapid_pulse symptom's
    // "heart_monitor" visual technique from the brief: steady baseline heart rate by
    // default, spikes only when rapid_pulse is among the CURRENT disease's revealed
    // symptoms (a discrete, per-disease signal - not a continuous Health gauge), and
    // flatlines at death.
    public class MonitorDisplay : MonoBehaviour
    {
        private const int BaselineBpm = 72;
        private const int SpikeBpm = 150;

        [SerializeField] private TextMeshPro readout;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || readout == null)
                return;

            var state = gm.State;
            if (state.IsGameOver && state.Health <= 0f)
            {
                readout.text = "FLATLINE";
                readout.color = Color.red;
                return;
            }

            if (IsRapidPulseRevealed(state))
            {
                readout.text = $"HR {SpikeBpm} !";
                readout.color = Color.red;
            }
            else
            {
                readout.text = $"HR {BaselineBpm}";
                readout.color = Color.green;
            }
        }

        private static bool IsRapidPulseRevealed(GameState state)
        {
            var symptoms = state.CurrentDisease?.symptoms;
            if (symptoms == null)
                return false;

            for (int i = 0; i < state.RevealedSymptomCount && i < symptoms.Length; i++)
            {
                if (symptoms[i] == "rapid_pulse")
                    return true;
            }
            return false;
        }
    }
}
