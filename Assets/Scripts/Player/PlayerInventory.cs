using Countdown.Placeholder;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.Player
{
    public enum HeldItemKind
    {
        None,
        BloodSample,
        Compound,
        Reagent
    }

    // Tracks what the scientist is currently carrying and renders the matching icon
    // from Items-Sheet near the character. Blood samples use their own dedicated
    // sprite (bloodSampleSprite) if one is assigned, otherwise they fall back to a
    // tinted procedural circle. The Injector reads Held
    // to decide whether an interaction should draw blood or administer a serum;
    // BloodResearchStation/Mixer are what actually call SetBloodSample/SetCompound/
    // SetReagent/Clear.
    public class PlayerInventory : MonoBehaviour
    {
        private const float IconBaseScale = 1f;
        private static readonly Color BloodSampleColor = new(0.75f, 0.05f, 0.05f);

        // Items-Sheet frame layout (16x16 each): 0=concentration (any level),
        // 1-4=shapes (colored to match their ShapeShelf bottle), 5-10=dyes (colored to
        // match DyeShelf), 11=finished serum (generic - carrying it never reveals the
        // recipe you mixed).
        private const int ConcentrationFrame = 0;
        private const int SerumFrame = 11;

        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer; // read for flipX, to mirror the icon's side
        [SerializeField] private Vector3 iconLocalOffset = new(0.28f, 0.05f, 0f);
        [SerializeField] private Sprite[] itemSprites; // 12 frames from Items-Sheet, in sheet order
        [SerializeField] private Sprite bloodSampleSprite; // optional dedicated art; falls back to tinted circle if unset
        [Tooltip("The scientist's bare-hands visual layer (own SpriteRenderer + Animator, playing e.g. Idle_Hands/Walk_Hands). Shown only while Held == None - once carrying something, the item icon replaces the hands entirely.")]
        [SerializeField] private GameObject handsRoot;

        public HeldItemKind Held { get; private set; } = HeldItemKind.None;
        public Compound HeldCompound { get; private set; }
        public string HeldReagentCategory { get; private set; }
        public string HeldReagentValue { get; private set; }

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
        // hands him an item enforces "one item at a time" for free rather than each
        // needing to remember the check.
        public bool SetBloodSample()
        {
            if (Held != HeldItemKind.None)
                return false;

            Held = HeldItemKind.BloodSample;
            UpdateHandsVisibility();
            if (iconRenderer == null)
                return true;

            if (bloodSampleSprite != null)
            {
                iconRenderer.sprite = bloodSampleSprite;
                iconRenderer.color = Color.white;
            }
            else
            {
                iconRenderer.sprite = ShapeSpriteLibrary.Instance.Get(ShapeKind.Circle);
                iconRenderer.color = BloodSampleColor;
            }
            iconRenderer.transform.localScale = Vector3.one * IconBaseScale;
            iconRenderer.enabled = true;
            return true;
        }

        public bool SetCompound(Compound compound)
        {
            if (Held != HeldItemKind.None)
                return false;

            Held = HeldItemKind.Compound;
            HeldCompound = compound;
            UpdateHandsVisibility();
            ShowItemSprite(SerumFrame);
            return true;
        }

        // A single reagent picked up from a shelf, carried to the Mixer to be
        // combined into a full Compound.
        public bool SetReagent(string category, string value)
        {
            if (Held != HeldItemKind.None)
                return false;

            Held = HeldItemKind.Reagent;
            HeldReagentCategory = category;
            HeldReagentValue = value;
            UpdateHandsVisibility();
            ShowItemSprite(FrameFor(category, value));
            return true;
        }

        private void ShowItemSprite(int frameIndex)
        {
            if (iconRenderer == null)
                return;

            if (itemSprites != null && frameIndex >= 0 && frameIndex < itemSprites.Length)
                iconRenderer.sprite = itemSprites[frameIndex];
            iconRenderer.color = Color.white;
            iconRenderer.transform.localScale = Vector3.one * IconBaseScale;
            iconRenderer.enabled = true;
        }

        private static int FrameFor(string category, string value) => category switch
        {
            "concentration" => ConcentrationFrame,
            "shape" => value switch
            {
                "circle" => 1,
                "square" => 2,
                "triangle" => 3,
                "diamond" => 4,
                _ => ConcentrationFrame
            },
            "color" => value switch
            {
                "violet" => 5,
                "azure" => 6,
                "jade" => 7,
                "amber" => 8,
                "ash" => 9,
                "crimson" => 10,
                _ => ConcentrationFrame
            },
            _ => ConcentrationFrame
        };

        private void UpdateHandsVisibility()
        {
            if (handsRoot != null)
                handsRoot.SetActive(Held == HeldItemKind.None);
        }

        public void Clear()
        {
            Held = HeldItemKind.None;
            HeldReagentCategory = null;
            HeldReagentValue = null;
            UpdateHandsVisibility();
            if (iconRenderer != null)
                iconRenderer.enabled = false;
        }
    }
}