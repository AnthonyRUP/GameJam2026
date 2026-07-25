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
    public class InjectorStation : Interactable
    {
        private static readonly int ActivateTrigger = Animator.StringToHash("Activate");

        [SerializeField] private Animator animator;
        [Tooltip("Seconds the player is locked in place per draw/administer action. Defaults to the 22-frame clip's own length at 12fps (~1.83s) - tune freely, it's independent of animation playback speed.")]
        [SerializeField] private float activationSeconds = 22f / 12f;

        private bool _busy;

        protected override void OnInteract()
        {
            if (_busy)
                return;

            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            var inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            var controller = playerGo != null ? playerGo.GetComponent<PlayerController>() : null;
            if (inventory == null)
                return;

            switch (inventory.Held)
            {
                case HeldItemKind.None:
                    int maxDraws = gm.Codex.mechanics.blood_test.draws_needed_for_full_blood_certainty;
                    if (gm.State.BloodDraws.Count >= maxDraws)
                        return;
                    StartCoroutine(RunAction(controller, () => inventory.SetBloodSample()));
                    break;

                case HeldItemKind.Compound:
                    var compound = inventory.HeldCompound;
                    StartCoroutine(RunAction(controller, () =>
                    {
                        inventory.Clear();
                        gm.RecordAdministerAttempt(compound);
                    }));
                    break;

                case HeldItemKind.BloodSample:
                    break;
            }
        }

        private IEnumerator RunAction(PlayerController controller, System.Action onComplete)
        {
            _busy = true;
            if (controller != null)
                controller.SetInputEnabled(false);
            if (animator != null)
                animator.SetTrigger(ActivateTrigger);

            yield return new WaitForSeconds(activationSeconds);

            onComplete();
            if (controller != null)
                controller.SetInputEnabled(true);
            _busy = false;
        }
    }
}
