using System.Collections.Generic;
using Countdown.Data;

namespace Countdown.Runtime
{
    public struct BloodDrawResult
    {
        public string Attribute; // "size" | "shape"
        public string RevealedValue;
    }

    public struct Compound
    {
        public string Color;
        public string Size;
        public string Shape;
    }

    public struct AdministerAttempt
    {
        public Compound Compound;
        public int MatchCount;
        public string OutcomeCategory; // "cure" | "improves" | "no_effect" | "worsens"
    }

    public class GameState
    {
        public DiseaseData CurrentDisease;
        public float Health;
        public bool IsGameOver;
        public bool HasWon;

        public int RevealedSymptomCount; // 0..3
        public List<BloodDrawResult> BloodDraws = new();
        public List<AdministerAttempt> AdministerHistory = new();
        public List<DiseaseData> Shortlist = new();
    }
}
