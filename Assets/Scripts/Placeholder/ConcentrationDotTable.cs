namespace Countdown.Placeholder
{
    // Pathogen concentration reads as a dot count rather than a scaled shape - a
    // discrete count needs no reference frame to be legible, unlike a continuous
    // scale (which a lone glyph can't communicate "big or small" for on its own).
    public static class ConcentrationDotTable
    {
        public static int DotCount(string concentration) => concentration switch
        {
            "low" => 2,
            "medium" => 4,
            "high" => 8,
            _ => 1 // decoy ("trace") or unrecognized - deliberately off-pattern, never a real reading
        };
    }
}
