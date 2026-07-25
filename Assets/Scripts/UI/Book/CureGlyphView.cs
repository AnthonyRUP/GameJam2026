using UnityEngine;
using UnityEngine.UI;
using Countdown.Placeholder;

namespace Countdown.UI.Book
{
    // The cure's full recipe as one glyph. Unlike BloodGlyphView (which deliberately
    // hides one attribute per draw, since the blood test is a partial reveal), this
    // is a fully-known recipe from the reference book - shape, color and
    // concentration are all shown together: the real shape tinted the real color,
    // with a scattered dot cluster (2/4/8 = low/medium/high) for concentration.
    public class CureGlyphView : MonoBehaviour
    {
        private const float MaxRadius = 0.42f; // matches ProceduralShapeFactory's margin-inset extent
        private const float RadiusStep = 0.02f;
        private const float InsetFactor = 0.75f; // sit a bit inside the boundary, not touching it

        private static readonly Color DotColor = Color.black;

        [SerializeField] private Image shapeImage;
        [SerializeField] private Image[] dots;

        public void Configure(string color, string concentration, string shape)
        {
            var shapeKind = ShapeSpriteLibrary.FromName(shape);

            if (shapeImage != null)
            {
                shapeImage.enabled = true;
                shapeImage.sprite = ShapeSpriteLibrary.Instance.Get(shapeKind);
                shapeImage.color = ColorPalette.Get(color);
            }

            int count = ConcentrationDotTable.DotCount(concentration);
            if (dots == null)
                return;

            var iconSize = ((RectTransform)transform).sizeDelta;
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] == null)
                    continue;

                dots[i].color = DotColor;
                dots[i].gameObject.SetActive(i < count);
                PositionDot(dots[i], i, dots.Length, shapeKind, iconSize);
            }
        }

        // Walks inward from the shape's outer extent along a fixed angle until the
        // point actually tests inside the shape's own fill - guarantees containment
        // for any of the 4 real shapes (a triangle or diamond leaves far less usable
        // area near the bounding box's edges than a circle does), rather than reusing
        // one hardcoded scatter pattern that only happens to fit a circle.
        private static void PositionDot(Image dot, int index, int total, ShapeKind shapeKind, Vector2 iconSize)
        {
            float angle = index * (360f / total) * Mathf.Deg2Rad;
            float dirX = Mathf.Cos(angle);
            float dirY = Mathf.Sin(angle);

            float safeRadius = 0f;
            for (float r = MaxRadius; r >= 0f; r -= RadiusStep)
            {
                if (ProceduralShapeFactory.Contains(shapeKind, dirX * r, dirY * r))
                {
                    safeRadius = r;
                    break;
                }
            }

            float finalRadius = safeRadius * InsetFactor;
            var rt = (RectTransform)dot.transform;
            rt.anchoredPosition = new Vector2(dirX * finalRadius * iconSize.x, dirY * finalRadius * iconSize.y);
        }

        public void Clear()
        {
            if (shapeImage != null)
                shapeImage.enabled = false;

            if (dots == null)
                return;
            foreach (var dot in dots)
                if (dot != null)
                    dot.gameObject.SetActive(false);
        }
    }
}
