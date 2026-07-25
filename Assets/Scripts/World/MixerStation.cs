using Countdown.Player;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.World
{
    // No panel: interacting while carrying a reagent inserts it directly into the
    // mixer's three slots (color/concentration/shape). A second reagent of a
    // category already filled is rejected outright. Once all three are filled, the
    // mixer immediately combines them into a compound, resets its slots, and hands
    // the compound back into the scientist's grip.
    public class MixerStation : Interactable
    {
        private string _color;
        private string _concentration;
        private string _shape;

        protected override void OnInteract()
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            var inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            if (inventory == null || inventory.Held != HeldItemKind.Reagent)
                return;

            string category = inventory.HeldReagentCategory;
            string value = inventory.HeldReagentValue;

            if (IsSlotFilled(category))
                return; // already holds one of this category - reject the duplicate

            FillSlot(category, value);
            inventory.Clear();

            if (_color != null && _concentration != null && _shape != null)
            {
                var compound = new Compound { Color = _color, Concentration = _concentration, Shape = _shape };
                _color = null;
                _concentration = null;
                _shape = null;
                inventory.SetCompound(compound);
            }
        }

        private bool IsSlotFilled(string category) => category switch
        {
            "color" => _color != null,
            "concentration" => _concentration != null,
            "shape" => _shape != null,
            _ => true // unrecognized category - reject defensively
        };

        private void FillSlot(string category, string value)
        {
            switch (category)
            {
                case "color": _color = value; break;
                case "concentration": _concentration = value; break;
                case "shape": _shape = value; break;
            }
        }
    }
}
