using Countdown.Placeholder;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.Player
{
    public enum HeldItemKind
    {
        None,
        BloodSample,
        Compound
    }

    // Tracks what the scientist is currently carrying - empty hands, a drawn blood
    // sample, or a synthesized compound - and renders a small placeholder icon for it
    // near the character. The Injector reads this to decide whether an interaction
    // should draw blood or administer a serum; the eventual BloodResearchStation/Mixer
    // will be what actually calls SetBloodSample/SetCompound/Clear.
    public class PlayerInventory : MonoBehaviour
    {
        private const float IconBaseScale = 0.3f;
        private static readonly Color BloodSampleColor = new(0.75f, 0.05f, 0.05f);

        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer; // read for flipX, to mirror the icon's side
        [SerializeField] private Vector3 iconLocalOffset = new(0.28f, 0.05f, 0f);

        public HeldItemKind Held { get; private set; } = HeldItemKind.None;
        public Compound HeldCompound { get; private set; }

        private void LateUpdate()
        {
            if (iconRenderer == null || bodyRenderer == null)
                return;

            var offset = iconLocalOffset;
            if (bodyRenderer.flipX)
                offset.x = -offset.x;
            iconRenderer.transform.localPosition = offset;
        }

        // The scientist only ever has two hands and one job: these return false and
        // change nothing if he's already carrying something, so every station that
        // hands him an item (Injector, and later Mixer/BloodResearchStation) enforces
        // "one item at a time" for free rather than each needing to remember the check.
        public bool SetBloodSample()
        {
            if (Held != HeldItemKind.None)
                return false;

            Held = HeldItemKind.BloodSample;
            if (iconRenderer == null)
                return true;

            iconRenderer.sprite = ShapeSpriteLibrary.Instance.Get(ShapeKind.Circle);
            iconRenderer.color = BloodSampleColor;
            iconRenderer.transform.localScale = Vector3.one * (IconBaseScale * SizeScaleTable.NeutralScale);
            iconRenderer.enabled = true;
            return true;
        }

        public bool SetCompound(Compound compound)
        {
            if (Held != HeldItemKind.None)
                return false;

            Held = HeldItemKind.Compound;
            HeldCompound = compound;
            if (iconRenderer == null)
                return true;

            iconRenderer.sprite = ShapeSpriteLibrary.Instance.Get(ShapeSpriteLibrary.FromName(compound.Shape));
            iconRenderer.color = ColorPalette.Get(compound.Color);
            iconRenderer.transform.localScale = Vector3.one * (IconBaseScale * SizeScaleTable.Scale(compound.Size));
            iconRenderer.enabled = true;
            return true;
        }

        public void Clear()
        {
            Held = HeldItemKind.None;
            if (iconRenderer != null)
                iconRenderer.enabled = false;
        }
    }
}
