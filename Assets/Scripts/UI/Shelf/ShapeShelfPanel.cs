using Countdown.Core;
using Countdown.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Countdown.UI.Shelf
{
    // The ShapeShelf close-up: a 7-frame backdrop sheet where each frame highlights
    // one of the 7 reagent flasks (raster order, matching the shelf's physical
    // top-left-to-bottom-right layout: concentration:low, shape:square, shape:circle,
    // shape:triangle, concentration:medium, shape:diamond, concentration:high). No
    // decoys on this shelf. Same mechanism as DyeShelfPanel: Left/Right cycles the
    // highlighted flask (wraps), Space picks it up into the scientist's hand and
    // closes the panel.
    public class ShapeShelfPanel : MonoBehaviour
    {
        private readonly struct ShelfEntry
        {
            public readonly string Category;
            public readonly string Value;
            public ShelfEntry(string category, string value) { Category = category; Value = value; }
        }

        private static readonly ShelfEntry[] Entries =
        {
            new("concentration", "low"),
            new("shape", "square"),
            new("shape", "circle"),
            new("shape", "triangle"),
            new("concentration", "medium"),
            new("shape", "diamond"),
            new("concentration", "high"),
        };

        [SerializeField] private Image backdropImage;
        [SerializeField] private Sprite[] frames; // 7, in Entries order

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

            var entry = Entries[_selectedIndex];
            if (_inventory.SetReagent(entry.Category, entry.Value))
                GameManager.Instance.ClosePanel();
        }

        private void Refresh()
        {
            if (backdropImage != null && frames != null && _selectedIndex < frames.Length)
                backdropImage.sprite = frames[_selectedIndex];
        }
    }
}
