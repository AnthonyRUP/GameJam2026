using System.Collections;
using Countdown.Core;
using Countdown.Player;
using UnityEngine;

namespace Countdown.World
{
    // The Injector is dual-purpose and shares one physical animation for both roles:
    // with empty hands it draws a blood sample (up to the codex's per-playthrough draw
    // cap); holding a synthesized compound, the same interaction administers it instead.
    // Already holding an unused blood sample blocks both - it has to be tested first.
    // Drawn samples don't go straight into the player's hands - they pop out of the
    // machine at sampleSpawnPoint as a BloodSamplePickup, which the player then
    // interacts with separately to actually pick up.
    public class InjectorStation : Interactable
    {
        private static readonly int ActivateTrigger = Animator.StringToHash("Activate");

        [SerializeField] private Animator animator;
        [Tooltip("Seconds the player is locked in place per draw/administer action. Defaults to the 22-frame clip's own length at 12fps (~1.83s) - tune freely, it's independent of animation playback speed.")]
        [SerializeField] private float activationSeconds = 22f / 12f;
        [SerializeField] private Transform sampleSpawnPoint;
        [SerializeField] private GameObject bloodSamplePrefab;

        private bool _busy;
        private GameObject _pendingSample; // uncollected vial currently sitting in the world, if any

        protected override void OnInteract()
        {
            if (_busy)
                return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            var inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            if (inventory == null)
                return;

            switch (inventory.Held)
            {
                case HeldItemKind.None:
                    if (_pendingSample != null)
                        return; // a drawn sample is still sitting uncollected - test it first

                    int maxDraws = gm.Codex.mechanics.blood_test.draws_needed_for_full_blood_certainty;
                    if (gm.State.BloodDraws.Count >= maxDraws)
                        return;
                    StartCoroutine(RunAction(SpawnBloodSample));
                    break;

                case HeldItemKind.Compound:
                    var compound = inventory.HeldCompound;
                    StartCoroutine(RunAction(() =>
                    {
                        inventory.Clear();
                        gm.RecordAdministerAttempt(compound);
                    }));
                    break;

                case HeldItemKind.BloodSample:
                    break;
            }
        }

        private void SpawnBloodSample()
        {
            if (bloodSamplePrefab == null || sampleSpawnPoint == null)
                return;

            _pendingSample = Instantiate(bloodSamplePrefab, sampleSpawnPoint.position, sampleSpawnPoint.rotation);
        }

        private IEnumerator RunAction(System.Action onComplete)
        {
            _busy = true;
            if (animator != null)
                animator.SetTrigger(ActivateTrigger);

            yield return new WaitForSeconds(activationSeconds);

            onComplete();
            _busy = false;
        }
    }
}