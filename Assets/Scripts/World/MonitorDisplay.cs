using Countdown.Core;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.World
{
    // Owns the heartbeat audio only - the Monitor's Interactable now opens
    // MonitorPanel (a real UI panel, like BloodTestPanel) rather than toggling audio
    // directly in the world. The panel calls SetListening while it's open, so the
    // heartbeat is only audible while the player is actually looking at the readout.
    // This is the rapid_pulse symptom's "heart_monitor" audio technique from the
    // brief: normal pitch by default, sped up only when rapid_pulse is among the
    // CURRENT disease's revealed symptoms - a discrete, per-disease signal, not a
    // continuous Health gauge - and silent at flatline.
    [RequireComponent(typeof(AudioSource))]
    public class MonitorDisplay : MonoBehaviour
    {
        private const float BaselinePitch = 1f;
        private const float SpikePitch = 1.7f;

        [SerializeField] private AudioClip heartbeatClip;

        private AudioSource _audio;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.clip = heartbeatClip;
            _audio.loop = true;
            _audio.playOnAwake = false;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            var state = gm.State;

            if (state.IsGameOver && state.Health <= 0f)
            {
                if (_audio.isPlaying)
                    _audio.Stop();
                return;
            }

            _audio.pitch = IsRapidPulseRevealed(state) ? SpikePitch : BaselinePitch;
        }

        public void SetListening(bool listening)
        {
            if (heartbeatClip == null)
                return;

            if (listening && !_audio.isPlaying)
                _audio.Play();
            else if (!listening && _audio.isPlaying)
                _audio.Stop();
        }

        public static bool IsRapidPulseRevealed(GameState state)
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
