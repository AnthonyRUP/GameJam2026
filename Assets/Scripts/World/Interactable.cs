using Countdown.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.World
{
    // Physical trigger zone for a station the player walks up to. OnInteract opens the
    // corresponding phase panel via GameManager - concrete per-station differentiation
    // (e.g. Injector's dual draw/administer role) is layered on top as those systems land.
    // Only ever responds to E while it's the single closest candidate to the player, as
    // decided by NearestInteractableSelector - this stops two overlapping interaction
    // zones both being interactable (or highlighted) at once.
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private GamePhase phaseToOpen;
        [SerializeField] private GameObject promptRoot; // shown when player is in range, optional
        [TextArea]
        [SerializeField] private string helpDescription; // shown on ScientistHelpPanel when this is the ActiveInteractable and the player presses H

        protected bool _playerInRange;
        private bool _suppressedUntilReentry; // true right after Interact() - blocks re-triggering and hides the highlight until the player leaves and re-enters range

        private StationHighlight _highlight;

        // Override for anything that shouldn't be selectable yet (e.g. a vial still
        // mid-hop, still settling) - default true, always eligible once in range.
        public virtual bool CanBeActive => true;

        // Read by StationHighlight - even if this is the ActiveInteractable, the
        // highlight stays hidden while suppressed (e.g. so it doesn't sit on top of
        // and block view of the interaction's own animation).
        public bool IsSuppressedUntilReentry => _suppressedUntilReentry;

        // Read by ScientistHelpPanel - the short "what is this station" blurb shown
        // bottom-right of the player when they press H while this is the ActiveInteractable.
        public string HelpDescription => helpDescription;

        private void Awake()
        {
            _highlight = GetComponent<StationHighlight>();
        }

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            _suppressedUntilReentry = false; // fresh entry - clear any prior suppression
            if (promptRoot != null) promptRoot.SetActive(true);
            NearestInteractableSelector.Instance?.Register(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (promptRoot != null) promptRoot.SetActive(false);
            NearestInteractableSelector.Instance?.Unregister(this);
        }

        private void Update()
        {
            if (!_playerInRange || _suppressedUntilReentry)
                return;

            bool isActive = NearestInteractableSelector.Instance == null
                || NearestInteractableSelector.Instance.ActiveInteractable == this;

            if (isActive && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                Interact();
        }

        public void Interact()
        {
            _suppressedUntilReentry = true;
            _highlight?.Hide();
            OnInteract();
        }

        protected virtual void OnInteract()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OpenPanel(phaseToOpen);
        }
    }
}