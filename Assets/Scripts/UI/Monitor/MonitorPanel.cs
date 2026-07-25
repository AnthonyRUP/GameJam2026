using Countdown.Core;
using Countdown.Runtime;
using Countdown.World;
using TMPro;
using UnityEngine;

namespace Countdown.UI.Monitor
{
    // Two data points: heart rate (scrolling ECG trace + BPM label) and temperature
    // (fever/chills' "instrument_readout" technique from the brief - previously
    // unbuilt, since only the sprite-overlay symptoms had any visual at all).
    // Heartbeat audio only plays while this panel is open (see MonitorDisplay).
    public class MonitorPanel : MonoBehaviour
    {
        private const float BaselineBpm = 72f;
        private const float SpikeBpm = 150f;
        private const string NormalTemp = "98.6";
        private const string HighTemp = "104.2";
        private const string LowTemp = "92.4";

        [SerializeField] private ECGWaveform ecg;
        [SerializeField] private TextMeshProUGUI bpmLabel;
        [SerializeField] private TextMeshProUGUI tempLabel;

        private MonitorDisplay _monitorDisplay;

        private void OnEnable()
        {
            if (_monitorDisplay == null)
                _monitorDisplay = Object.FindAnyObjectByType<MonitorDisplay>();
            _monitorDisplay?.SetListening(true);
        }

        private void OnDisable()
        {
            _monitorDisplay?.SetListening(false);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            var state = gm.State;
            bool rapid = MonitorDisplay.IsRapidPulseRevealed(state);
            float bpm = rapid ? SpikeBpm : BaselineBpm;

            if (ecg != null)
                ecg.Bpm = bpm;

            if (bpmLabel != null)
            {
                bpmLabel.text = rapid ? $"HR {Mathf.RoundToInt(bpm)} !" : $"HR {Mathf.RoundToInt(bpm)}";
                bpmLabel.color = rapid ? Color.red : Color.green;
            }

            if (tempLabel != null)
            {
                string tempState = TemperatureState(state);
                tempLabel.text = tempState switch
                {
                    "high" => $"TEMP {HighTemp} !",
                    "low" => $"TEMP {LowTemp} !",
                    _ => $"TEMP {NormalTemp}"
                };
                tempLabel.color = tempState == "normal" ? Color.green : Color.red;
            }
        }

        // Scans from the most recently revealed symptom backward, so if a disease
        // (rarely) has both fever and chills, whichever was revealed later wins -
        // it reflects the patient's current state, not their history.
        private static string TemperatureState(GameState state)
        {
            var symptoms = state.CurrentDisease?.symptoms;
            if (symptoms == null)
                return "normal";

            int count = Mathf.Min(state.RevealedSymptomCount, symptoms.Length);
            for (int i = count - 1; i >= 0; i--)
            {
                if (symptoms[i] == "fever")
                    return "high";
                if (symptoms[i] == "chills")
                    return "low";
            }
            return "normal";
        }
    }
}
