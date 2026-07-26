using UnityEngine;

namespace Countdown.World
{
    // Shows a highlighted overlay sprite only while this GameObject's Interactable is
    // the current ActiveInteractable per NearestInteractableSelector - i.e. only the
    // single closest in-range station gets highlighted, never two at once, and it's
    // always the same one you're actually able to interact with. Purely visual;
    // independent of Interactable's own E-key/panel-opening logic.
    public class StationHighlight : MonoBehaviour
    {
        [SerializeField] private GameObject highlightRoot;

        private Interactable _interactable;

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
        }

        private void Update()
        {
            bool shouldShow = _interactable != null
                && !_interactable.IsSuppressedUntilReentry
                && NearestInteractableSelector.Instance != null
                && NearestInteractableSelector.Instance.ActiveInteractable == _interactable;

            if (highlightRoot != null)
                highlightRoot.SetActive(shouldShow);
        }

        // Called by Interactable the instant E is pressed, before any interaction
        // logic runs - once you're actually interacting, "you can interact with
        // this" is no longer useful information. Re-shows automatically once this
        // becomes the ActiveInteractable again (e.g. after leaving and re-entering
        // range, or the previous active station stops being eligible).
        public void Hide()
        {
            if (highlightRoot != null)
                highlightRoot.SetActive(false);
        }
    }
}