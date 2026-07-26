using Countdown.Core;
using Countdown.World;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.UI
{
    // Screen-centered "what is this station" popup. H toggles it on/off; while on,
    // it shows the HelpDescription of whichever station is currently the
    // NearestInteractableSelector's ActiveInteractable - i.e. the same single
    // station E would interact with and StationHighlight would highlight. Only
    // shown during free-roam (no phase panel open) and hidden with nothing to say.
    public class ScientistHelpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root; // backdrop + label, a centered Canvas child
        [SerializeField] private TextMeshProUGUI label;

        private bool _toggledOn;
        private Interactable _shownFor;
        private PlayerController _player;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                _toggledOn = !_toggledOn;

            if (_player == null)
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                _player = playerGo != null ? playerGo.GetComponent<PlayerController>() : null;
            }

            // Walking away closes the popup outright (same as Escape) rather than
            // just hiding it while out of range - otherwise it'd silently reappear
            // for whatever station the player next walks up to.
            if (_toggledOn && _player != null && _player.IsMoving)
                _toggledOn = false;

            var gm = GameManager.Instance;
            bool freeRoam = gm != null && gm.CurrentPhase == GamePhase.Boot;
            var active = freeRoam ? NearestInteractableSelector.Instance?.ActiveInteractable : null;

            bool shouldShow = _toggledOn && active != null && !string.IsNullOrEmpty(active.HelpDescription);
            if (root != null)
                root.SetActive(shouldShow);

            if (!shouldShow)
                return;

            if (active != _shownFor)
            {
                _shownFor = active;
                if (label != null)
                    label.text = active.HelpDescription;
            }
        }
    }
}
