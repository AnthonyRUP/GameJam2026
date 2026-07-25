using System.Collections.Generic;

namespace Countdown.Data
{
    public enum SymptomTechnique
    {
        InstrumentReadout,
        SpriteOverlay,
        DiscreteEvent,
        SpriteJitter,
        IconTicker
    }

    public readonly struct SymptomVisual
    {
        public readonly SymptomTechnique Technique;
        public readonly string AssetName; // null for code-driven techniques (e.g. tremor)

        public SymptomVisual(SymptomTechnique technique, string assetName)
        {
            Technique = technique;
            AssetName = assetName;
        }
    }

    // Presentation metadata for the 10 symptoms, hand-ported from the brief's table.
    public static class SymptomVisualCatalog
    {
        public static readonly Dictionary<string, SymptomVisual> Visuals = new()
        {
            ["fever"] = new SymptomVisual(SymptomTechnique.InstrumentReadout, "temp_monitor_high"),
            ["chills"] = new SymptomVisual(SymptomTechnique.InstrumentReadout, "temp_monitor_low"),
            ["rapid_pulse"] = new SymptomVisual(SymptomTechnique.InstrumentReadout, "heart_monitor_spike"),
            ["rash"] = new SymptomVisual(SymptomTechnique.SpriteOverlay, "overlay_rash"),
            ["swelling"] = new SymptomVisual(SymptomTechnique.SpriteOverlay, "overlay_swelling"),
            ["bloodshot_eyes"] = new SymptomVisual(SymptomTechnique.SpriteOverlay, "overlay_eyes"),
            ["pale_skin"] = new SymptomVisual(SymptomTechnique.SpriteOverlay, "overlay_pale_skin"),
            ["cough"] = new SymptomVisual(SymptomTechnique.DiscreteEvent, "event_cough_fog"),
            ["tremor"] = new SymptomVisual(SymptomTechnique.SpriteJitter, null),
            ["nausea"] = new SymptomVisual(SymptomTechnique.IconTicker, "icon_nausea"),
        };
    }
}
