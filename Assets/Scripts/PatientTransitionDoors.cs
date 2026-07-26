using System;
using System.Collections;
using UnityEngine;

namespace Countdown.World
{
    // Doors on the test tank used for two different transitions:
    //  - PlayPatientSwap: a SINGLE close-hold-open clip (one trigger). The patient
    //    swap fires from an Animation Event (NotifyFullyClosed) placed at the exact
    //    frame the doors are fully shut - not on a guessed timer - so it works no
    //    matter how long the "hold closed" portion of the clip is. Deactivates once
    //    the whole clip finishes.
    //  - PlayFinalClose: a separate, harsher close animation that plays once and
    //    stays shut/active forever - used when the patient dies.
    // GameManager owns calling these; this object never decides on its own to run.
    public class PatientTransitionDoors : MonoBehaviour
    {
        private static readonly int PlayTrigger = Animator.StringToHash("Play");
        private static readonly int CloseFinalTrigger = Animator.StringToHash("CloseFinal");

        [SerializeField] private Animator animator;
        [Tooltip("Total seconds the whole close-hold-open clip takes, start to finish. Used only as a safety fallback in case the animation event is ever missing/misplaced.")]
        [SerializeField] private float totalTransitionSeconds = 3f;
        [Tooltip("Seconds the permanent death-close animation takes.")]
        [SerializeField] private float closeFinalAnimationSeconds = 1f;

        private Action _pendingOnClosed;
        private bool _closedCallbackFired;

        private void Awake()
        {
            // Not mid-transition by default - stays out of the way otherwise.
            gameObject.SetActive(false);
        }

        // Starts the single close-hold-open clip and waits for it to finish. The
        // patient swap itself happens via NotifyFullyClosed (an Animation Event on
        // the clip), invoked automatically whenever playback reaches that frame.
        public IEnumerator PlayPatientSwap(Action onClosed)
        {
            gameObject.SetActive(true);
            _pendingOnClosed = onClosed;
            _closedCallbackFired = false;

            if (animator != null)
                animator.SetTrigger(PlayTrigger);

            // Wait for the animation event to fire the swap - with a timeout safety
            // net in case the event is ever missing, so this can't softlock forever.
            float waited = 0f;
            while (!_closedCallbackFired && waited < totalTransitionSeconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            if (!_closedCallbackFired)
                NotifyFullyClosed();

            // Let the rest of the clip (the opening portion) finish playing before
            // deactivating the doors.
            float remaining = totalTransitionSeconds - waited;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            gameObject.SetActive(false);
        }

        // Hook this up as an Animation Event on the clip, placed at the exact frame
        // the doors are fully shut (see the Animation window - right-click the
        // timeline at that frame -> Add Animation Event -> pick this function).
        public void NotifyFullyClosed()
        {
            if (_closedCallbackFired)
                return;

            _closedCallbackFired = true;
            _pendingOnClosed?.Invoke();
            _pendingOnClosed = null;
        }

        // Plays the permanent close animation and leaves the doors shut - this
        // object stays active/closed forever afterward, unlike PlayPatientSwap.
        public IEnumerator PlayFinalClose()
        {
            gameObject.SetActive(true);

            if (animator != null)
                animator.SetTrigger(CloseFinalTrigger);
            yield return new WaitForSeconds(closeFinalAnimationSeconds);
            // Intentionally stays active and closed - no further action.
        }

        // Instantly hides the doors with no animation - used when restarting after
        // a death, since PlayFinalClose otherwise leaves them permanently active
        // and closed. Not used for the normal patient-swap transition, which
        // already deactivates itself at the right moment on its own.
        public void ResetImmediately()
        {
            gameObject.SetActive(false);
        }
    }
}