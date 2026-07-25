using System.Collections;
using System.Collections.Generic;
using Countdown.Core;
using UnityEngine;

namespace Countdown.UI.Triage
{
    // Drives the Close-Up Character's base animation plus all symptom overlay layers,
    // keeping every active layer's Animator in lockstep with the base (Normal/Tremor/
    // Vomiting), and toggling each overlay's visibility based on which symptoms are
    // currently revealed for the disease.
    //
    // BlueSkin shows whenever pale_skin is revealed. Swelling alone shows plain Lumps;
    // swelling combined with pale_skin swaps Lumps for BlueLumps (a recolored variant
    // matching the tinted skin) layered on top of BlueSkin, rather than stacking the
    // mismatched plain-color Lumps on a blue body. BloodEyes, Rash and Vomit are
    // independent add-on layers that combine freely with all of the above.
    public class PatientCloseUpAnimator : MonoBehaviour
    {
        private const string NormalState = "Normal";
        private const string TremorState = "Tremor";
        private const string VomitingState = "Vomiting";

        // Tremor plays in short random bursts rather than continuously, to read as an
        // involuntary shake rather than constant shaking.
        private const float TremorBurstDuration = 5f / 8f; // matches the 5-frame clip @ 8fps
        private const float TremorMinGap = 2f;
        private const float TremorMaxGap = 5f;

        [SerializeField] private Animator characterAnimator;

        [Header("Swelling / Discoloration group")]
        [SerializeField] private GameObject lumpsRoot;       // swelling only (plain color)
        [SerializeField] private Animator lumpsAnimator;
        [SerializeField] private GameObject blueLumpsRoot;   // swelling + pale_skin (recolored, stacks on BlueSkin)
        [SerializeField] private Animator blueLumpsAnimator;
        [SerializeField] private GameObject blueSkinRoot;    // pale_skin (any time it's present)
        [SerializeField] private Animator blueSkinAnimator;

        [Header("Independent add-on layers")]
        [SerializeField] private GameObject bloodEyesRoot;
        [SerializeField] private Animator bloodEyesAnimator;
        [SerializeField] private GameObject rashRoot;
        [SerializeField] private Animator rashAnimator;
        [SerializeField] private GameObject vomitRoot;
        [SerializeField] private Animator vomitAnimator;

        private string _targetMode = "";
        private Coroutine _tremorRoutine;
        private readonly List<Animator> _activeAnimators = new();

        private void OnEnable()
        {
            _targetMode = "";
        }

        private void OnDisable()
        {
            StopTremorRoutine();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || characterAnimator == null)
                return;

            var state = gm.State;
            var symptoms = state.CurrentDisease.symptoms;
            bool nausea = false, tremor = false, swelling = false, paleSkin = false, bloodshotEyes = false, rash = false;
            for (int i = 0; i < state.RevealedSymptomCount && i < symptoms.Length; i++)
            {
                switch (symptoms[i])
                {
                    case "nausea": nausea = true; break;
                    case "tremor": tremor = true; break;
                    case "swelling": swelling = true; break;
                    case "pale_skin": paleSkin = true; break;
                    case "bloodshot_eyes": bloodshotEyes = true; break;
                    case "rash": rash = true; break;
                }
            }

            string target = nausea ? VomitingState : tremor ? TremorState : NormalState;

            // A layer that just became visible must join whatever's actually playing
            // right now, mid-clip - not restart at frame 0 of a guessed state. The
            // base character Animator is always active and never toggled, so it's the
            // source of truth for both the current logical state and exact playback
            // position (this matters most during a Tremor burst: a layer revealed
            // mid-burst must pick up at the same frame as everyone else, not jump back
            // to Normal or restart Tremor from its beginning).
            var (liveState, liveTime) = GetCharacterPlayback();

            SetActiveAndSync(lumpsRoot, lumpsAnimator, swelling && !paleSkin, liveState, liveTime);
            SetActiveAndSync(blueLumpsRoot, blueLumpsAnimator, swelling && paleSkin, liveState, liveTime);
            SetActiveAndSync(blueSkinRoot, blueSkinAnimator, paleSkin, liveState, liveTime);
            SetActiveAndSync(bloodEyesRoot, bloodEyesAnimator, bloodshotEyes, liveState, liveTime);
            SetActiveAndSync(rashRoot, rashAnimator, rash, liveState, liveTime);
            SetActiveAndSync(vomitRoot, vomitAnimator, nausea, liveState, liveTime);

            RebuildActiveAnimatorList(swelling, paleSkin, bloodshotEyes, rash, nausea);

            if (target == _targetMode)
                return;

            _targetMode = target;
            StopTremorRoutine();

            if (target == TremorState)
                _tremorRoutine = StartCoroutine(TremorLoop());
            else
                PlayAll(target);
        }

        // Every animator driven by PlayAll (whole-list transitions and the tremor
        // burst loop) starts together in the same Update() call, at normalizedTime 0,
        // so they never drift out of phase with each other on their own - only a
        // layer joining an already-running clip needs the explicit resync above.
        private (string state, float normalizedTime) GetCharacterPlayback()
        {
            var info = characterAnimator.GetCurrentAnimatorStateInfo(0);
            string state = info.IsName(TremorState) ? TremorState
                : info.IsName(VomitingState) ? VomitingState
                : NormalState;
            return (state, info.normalizedTime % 1f);
        }

        private void RebuildActiveAnimatorList(bool swelling, bool paleSkin, bool bloodshotEyes, bool rash, bool nausea)
        {
            _activeAnimators.Clear();
            _activeAnimators.Add(characterAnimator);
            if (swelling && !paleSkin) _activeAnimators.Add(lumpsAnimator);
            if (swelling && paleSkin) _activeAnimators.Add(blueLumpsAnimator);
            if (paleSkin) _activeAnimators.Add(blueSkinAnimator);
            if (bloodshotEyes) _activeAnimators.Add(bloodEyesAnimator);
            if (rash) _activeAnimators.Add(rashAnimator);
            if (nausea) _activeAnimators.Add(vomitAnimator);
        }

        private static void SetActiveAndSync(GameObject go, Animator animator, bool active, string state, float normalizedTime)
        {
            if (go == null)
                return;
            if (go.activeSelf == active)
                return;

            go.SetActive(active);
            if (active && animator != null)
                animator.Play(state, 0, normalizedTime);
        }

        private void PlayAll(string stateName)
        {
            foreach (var a in _activeAnimators)
                if (a != null)
                    a.Play(stateName);
        }

        private void StopTremorRoutine()
        {
            if (_tremorRoutine != null)
            {
                StopCoroutine(_tremorRoutine);
                _tremorRoutine = null;
            }
        }

        private IEnumerator TremorLoop()
        {
            while (true)
            {
                PlayAll(NormalState);
                yield return new WaitForSeconds(Random.Range(TremorMinGap, TremorMaxGap));
                PlayAll(TremorState);
                yield return new WaitForSeconds(TremorBurstDuration);
            }
        }
    }
}
