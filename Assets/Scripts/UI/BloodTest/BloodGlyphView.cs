using Countdown.Placeholder;
using UnityEngine;
using UnityEngine.UI;

namespace Countdown.UI.BloodTest
{
    // Renders one blood draw's result: only the tested attribute is accurate, the other
    // is a fixed neutral placeholder - isolating exactly one variable per draw so the
    // choice of what to test carries real weight.
    public class BloodGlyphView : MonoBehaviour
    {
        [SerializeField] private Image shapeImage;

        public void Configure(string testedAttribute, string trueSize, string trueShape)
        {
            if (shapeImage == null)
                return;

            var library = ShapeSpriteLibrary.Instance;

            if (testedAttribute == "shape")
            {
                shapeImage.sprite = library.Get(ShapeSpriteLibrary.FromName(trueShape));
                shapeImage.rectTransform.localScale = Vector3.one * SizeScaleTable.NeutralScale;
            }
            else // "size"
            {
                shapeImage.sprite = library.Get(ShapeKind.NeutralUnknown);
                shapeImage.rectTransform.localScale = Vector3.one * SizeScaleTable.Scale(trueSize);
            }
        }
    }
}
