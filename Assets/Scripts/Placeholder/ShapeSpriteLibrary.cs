using System.Collections.Generic;
using UnityEngine;

namespace Countdown.Placeholder
{
    // Caches procedurally generated shape sprites and prefers a real art override once one
    // is assigned, so dropping real sprites in later requires no code changes.
    public class ShapeSpriteLibrary : MonoBehaviour
    {
        public static ShapeSpriteLibrary Instance { get; private set; }

        [Tooltip("Optional real-art overrides, indexed by ShapeKind. Leave elements empty to keep the procedural placeholder.")]
        [SerializeField] private Sprite[] realOverrides = new Sprite[System.Enum.GetValues(typeof(ShapeKind)).Length];

        private readonly Dictionary<ShapeKind, Sprite> _cache = new();

        private void Awake()
        {
            Instance = this;
        }

        public Sprite Get(ShapeKind kind)
        {
            int index = (int)kind;
            if (realOverrides != null && index < realOverrides.Length && realOverrides[index] != null)
                return realOverrides[index];

            if (!_cache.TryGetValue(kind, out var sprite))
            {
                sprite = ProceduralShapeFactory.Create(kind);
                _cache[kind] = sprite;
            }
            return sprite;
        }

        // Maps codex shape/reagent names ("triangle","square","circle","diamond","star")
        // to ShapeKind. Falls back to NeutralUnknown for unrecognized names.
        public static ShapeKind FromName(string name) => name switch
        {
            "triangle" => ShapeKind.Triangle,
            "square" => ShapeKind.Square,
            "circle" => ShapeKind.Circle,
            "diamond" => ShapeKind.Diamond,
            "star" => ShapeKind.Star,
            _ => ShapeKind.NeutralUnknown
        };
    }
}
