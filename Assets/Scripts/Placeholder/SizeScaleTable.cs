namespace Countdown.Placeholder
{
    // Modest scale deltas by design - reading the blood glyph's size should take genuine
    // attention, not be an obvious give-away at a glance.
    public static class SizeScaleTable
    {
        public const float NeutralScale = 1.0f;

        public static float Scale(string size) => size switch
        {
            "tiny" => 0.45f,
            "small" => 0.7f,
            "medium" => 1.0f,
            "large" => 1.3f,
            _ => NeutralScale
        };
    }
}
