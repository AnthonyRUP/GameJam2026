using System.Collections;
using Countdown.Core;
using UnityEngine;

namespace Countdown.World
{
    // Cough has no sprite overlay of its own (codex technique: discrete_event) - a
    // periodic sound is its entire visual/audio language. Lives on the world Tank
    // object rather than the Triage close-up view, so it's audible the whole time
    // cough is revealed, not just while the player happens to have the patient panel
    // open - closing the same feedback gap that made fever/chills/cough symptoms
    // invisible for however long it took to next open the Triage view.
    [RequireComponent(typeof(AudioSource))]
    public class CoughSymptomAudio : MonoBehaviour
    {
        private const float MinGap = 5f;
        private const float MaxGap = 12f;
        private const float FitCoughSpacingMin = 0.4f;
        private const float FitCoughSpacingMax = 0.9f;

        [SerializeField] private AudioClip coughClip;

        private AudioSource _source;
        private Coroutine _routine;
        private bool _coughActive;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            bool coughRevealed = false;
            var symptoms = gm.State.CurrentDisease.symptoms;
            for (int i = 0; i < gm.State.RevealedSymptomCount && i < symptoms.Length; i++)
            {
                if (symptoms[i] == "cough")
                {
                    coughRevealed = true;
                    break;
                }
            }

            if (coughRevealed == _coughActive)
                return;

            _coughActive = coughRevealed;
            if (_coughActive)
            {
                _routine = StartCoroutine(CoughLoop());
            }
            else if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        // Like tremor's on/off bursts: mostly silence, punctuated by a short irregular
        // episode - here, 1-2 coughs close together - rather than a metronome-even
        // single hit every gap, which read as too mechanical.
        private IEnumerator CoughLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(MinGap, MaxGap));

                int coughsInEpisode = Random.Range(1, 3);
                for (int i = 0; i < coughsInEpisode; i++)
                {
                    if (coughClip == null)
                        continue;

                    _source.PlayOneShot(coughClip);
                    // Never start the next cough while this one is still audible -
                    // wait out its full length before even considering the gap.
                    yield return new WaitUntil(() => !_source.isPlaying);

                    if (i < coughsInEpisode - 1)
                        yield return new WaitForSeconds(Random.Range(FitCoughSpacingMin, FitCoughSpacingMax));
                }
            }
        }
    }
}
