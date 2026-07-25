using Countdown.Core;
using TMPro;
using UnityEngine;

namespace Countdown.World
{
    // The tank's built-in countdown readout: time remaining before the patient dies
    // at the current health decay rate (Health / base_decay_per_second) - not a
    // separate timer, just Health reframed as a clock, since decay is a fixed rate.
    public class CountdownTimerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || gm.Codex == null || label == null)
                return;

            if (gm.State.IsGameOver && gm.State.Health <= 0f)
            {
                label.text = "0:00";
                return;
            }

            float secondsRemaining = gm.State.Health / gm.Codex.mechanics.base_decay_per_second;
            int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
            int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
            label.text = $"{minutes}:{seconds:00}";
        }
    }
}
