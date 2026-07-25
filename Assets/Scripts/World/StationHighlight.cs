using UnityEngine;

namespace Countdown.World
{
    // Shows a highlighted overlay sprite while the player is within the station's
    // existing interaction-range trigger - a purely visual "you can interact with
    // this" affordance, independent of Interactable's own E-key/panel-opening logic
    // (a separate component listening on the same collider, not a replacement for it).
    public class StationHighlight : MonoBehaviour
    {
        [SerializeField] private GameObject highlightRoot;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && highlightRoot != null)
                highlightRoot.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && highlightRoot != null)
                highlightRoot.SetActive(false);
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
