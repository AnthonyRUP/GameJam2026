using Countdown.Core;
using Countdown.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Countdown.UI.Shelf
{
    // The DyeShelf close-up: a 6-frame backdrop sheet where each frame highlights one
    // of the 6 color reagent bottles (raster order: violet, azure, amber, ash-decoy,
    // crimson, jade - matching the shelf's physical top-left-to-bottom-right layout,
    // not the codex's own array order). Left/Right cycles the highlighted bottle
    // (wraps, since it's a fixed small ring of choices), Space picks it up into the
    // scientist's hand and closes the panel.
    public class DyeShelfPanel : MonoBehaviour
    {
        private static readonly string[] ColorByIndex = { "violet", "azure", "amber", "ash", "crimson", "jade" };

        [SerializeField] private Image backdropImage;
        [SerializeField] private Sprite[] frames; // 6, in ColorByIndex order

        private int _selectedIndex;
        private PlayerInventory _inventory;

        private void OnEnable()
        {
            _selectedIndex = 0;
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            _inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            Refresh();
        }

        private void Update()
        {
            if (Keyboard.current == null || frames == null || frames.Length == 0)
                return;

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = (_selectedIndex - 1 + frames.Length) % frames.Length;
                Refresh();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                _selectedIndex = (_selectedIndex + 1) % frames.Length;
                Refresh();
            }
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TrySelect();
            }
        }

        private void TrySelect()
        {
            if (_inventory == null)
                return;

            string color = ColorByIndex[_selectedIndex];
            if (_inventory.SetReagent("color", color))
                GameManager.Instance.ClosePanel();
        }

        private void Refresh()
        {
            if (backdropImage != null && frames != null && _selectedIndex < frames.Length)
                backdropImage.sprite = frames[_selectedIndex];
        }
    }
}
