using UnityEngine;

namespace Countdown.Placeholder
{
    public enum ShapeKind
    {
        Triangle,
        Square,
        Circle,
        Diamond,
        Star,
        NeutralUnknown
    }

    // Generates simple filled-shape sprites at runtime so gameplay (especially the blood
    // glyph reading) can be built and played before real disease art arrives. Swap real
    // sprites in later via ShapeSpriteLibrary's realOverrides slots - no code changes needed.
    public static class ProceduralShapeFactory
    {
        public static Sprite Create(ShapeKind kind, int px = 64)
        {
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[px * px];
            for (int y = 0; y < px; y++)
            {
                for (int x = 0; x < px; x++)
                {
                    bool inside = InShape(kind, x, y, px);
                    pixels[y * px + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, px, px), new Vector2(0.5f, 0.5f), px);
        }

        private static bool InShape(ShapeKind kind, int x, int y, int n)
        {
            float fx = (x + 0.5f) / n;
            // Texture2D's pixel array is bottom-origin (row 0 = bottom), but the shape math
            // below is authored assuming fy=0 is the visual top - invert so orientation-
            // sensitive shapes (Triangle) come out pointing the intended way on screen.
            float fy = 1f - (y + 0.5f) / n;
            float cx = fx - 0.5f;
            float cy = fy - 0.5f;
            const float margin = 0.08f; // keep shapes inset from the sprite edge

            switch (kind)
            {
                case ShapeKind.Circle:
                    return (cx * cx + cy * cy) <= (0.5f - margin) * (0.5f - margin);

                case ShapeKind.Square:
                    return Mathf.Abs(cx) <= 0.5f - margin && Mathf.Abs(cy) <= 0.5f - margin;

                case ShapeKind.Diamond:
                    return (Mathf.Abs(cx) + Mathf.Abs(cy)) <= 0.5f - margin;

                case ShapeKind.Triangle:
                {
                    // Equilateral-ish triangle pointing up, inset by margin.
                    float top = margin;
                    float bottom = 1f - margin;
                    if (fy < top || fy > bottom) return false;
                    float t = (fy - top) / (bottom - top); // 0 at apex, 1 at base
                    float halfWidth = t * (0.5f - margin);
                    return Mathf.Abs(cx) <= halfWidth;
                }

                case ShapeKind.Star:
                {
                    // 4-point star via intersection of a diamond and a rotated (45deg) diamond,
                    // giving a distinct silhouette that reads as "not a real reading" at a glance.
                    float diamond = Mathf.Abs(cx) + Mathf.Abs(cy);
                    float rotated = Mathf.Abs(cx + cy) + Mathf.Abs(cx - cy);
                    bool inDiamond = diamond <= 0.5f - margin;
                    bool inRotated = rotated <= (0.5f - margin) * 1.05f;
                    return inDiamond || (inRotated && diamond <= 0.5f - margin + 0.12f);
                }

                case ShapeKind.NeutralUnknown:
                {
                    // Dashed/hashed ring - visually distinct from every real shape so it never
                    // reads as an actual measurement.
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);
                    bool inRing = dist <= 0.5f - margin && dist >= 0.5f - margin - 0.12f;
                    float angle = Mathf.Atan2(cy, cx);
                    bool dash = Mathf.Repeat(angle * Mathf.Rad2Deg, 45f) < 22f;
                    return inRing && dash;
                }

                default:
                    return false;
            }
        }
    }
}
