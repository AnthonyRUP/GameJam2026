using Countdown.Placeholder;
using UnityEngine;
using UnityEngine.UI;

namespace Countdown.UI.BloodTest
{
    // Renders one blood draw's result: only the tested attribute is accurate, the
    // other isn't shown at all - isolating exactly one variable per draw so the
    // choice of what to test carries real weight. A shape reading shows the real
    // shape icon; a concentration reading instead shows a petri-dish glyph (a dish
    // rim with a scattered cluster of cells) - dot count (2/4/6 = low/medium/high)
    // reads as an absolute count, no reference frame needed to judge it.
    public class BloodGlyphView : MonoBehaviour
    {
        [SerializeField] private Image shapeImage;
        [SerializeField] private GameObject petriDishRoot;
        [SerializeField] private Image[] cells; // 6 slots, toggled on/off by count

        public void Configure(string testedAttribute, string trueConcentration, string trueShape)
        {
            bool isConcentration = testedAttribute == "concentration";

            if (shapeImage != null)
            {
                shapeImage.gameObject.SetActive(!isConcentration);
                if (!isConcentration)
                    shapeImage.sprite = ShapeSpriteLibrary.Instance.Get(ShapeSpriteLibrary.FromName(trueShape));
            }

            if (petriDishRoot != null)
                petriDishRoot.SetActive(isConcentration);

            int count = isConcentration ? ConcentrationDotTable.DotCount(trueConcentration) : 0;
            if (cells == null)
                return;
            for (int i = 0; i < cells.Length; i++)
                if (cells[i] != null)
                    cells[i].gameObject.SetActive(i < count);
        }

        // Explicitly turns everything off - unlike relying on the parent GameObject
        // being deactivated (which only hides children that are actually nested
        // under it), this guarantees petriDishRoot/shapeImage/cells all turn off
        // regardless of hierarchy structure.
        public void Hide()
        {
            if (shapeImage != null)
                shapeImage.gameObject.SetActive(false);

            if (petriDishRoot != null)
                petriDishRoot.SetActive(false);

            if (cells == null)
                return;
            for (int i = 0; i < cells.Length; i++)
                if (cells[i] != null)
                    cells[i].gameObject.SetActive(false);
        }
    }
}