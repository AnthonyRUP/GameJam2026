using UnityEngine;

namespace Countdown.World
{
    // Shows a highlighted overlay sprite while the player is within the station's
    // existing interaction-range trigger - a purely visual "you can interact with
    // this" affordance, independent of Interactable's own E-key/panel-opening logic
    // (a separate component listening on the same collider, not a replacement for it).
    // If the object also implements ISettleGate (e.g. a vial still mid-hop), the
    // highlight waits until it reports settled before appearing - otherwise it looks
    // like you can interact with something that's still animating into place.
    public class StationHighlight : MonoBehaviour
    {
        [SerializeField] private GameObject highlightRoot;

        private ISettleGate _settleGate;
        private bool _playerInRange;

        private void Awake()
        {
            _settleGate = GetComponent<ISettleGate>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            RefreshVisibility();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            RefreshVisibility();
        }

        private void Update()
        {
            // Only need to keep polling while the player's actually in range and
            // something might still settle later - avoids per-frame work otherwise.
            if (_playerInRange && _settleGate != null && !_settleGate.HasSettled)
                RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            if (highlightRoot == null) return;
            bool settled = _settleGate == null || _settleGate.HasSettled;
            highlightRoot.SetActive(_playerInRange && settled);
        }

        // Called by Interactable the instant E is pressed, before any interaction
        // logic runs - once you're actually interacting, "you can interact with
        // this" is no longer useful information. Re-shows automatically next time
        // OnTriggerEnter2D fires (e.g. after leaving and re-entering range).
        public void Hide()
        {
            if (highlightRoot != null)
                highlightRoot.SetActive(false);
        }
    }
}