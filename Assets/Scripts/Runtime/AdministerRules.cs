using System;
using Countdown.Data;

namespace Countdown.Runtime
{
    // Deterministic (not random) administer outcome, driven by the "book's" fixed
    // knowledge of disease relationships - NOT by counting how many raw reagent
    // attributes happen to overlap. A cure is only ever the exact match for the true
    // disease; any other recognized disease's cure resolves by symptom overlap with
    // the true disease (near-identical diseases give a real signal, unrelated ones
    // are actively harmful). A compound that isn't any of the 14 diseases' real
    // recipe (decoy ingredient, or a real-but-unassigned combo) simply does nothing.
    public static class AdministerRules
    {
        public const string Cure = "cure";
        public const string Improves = "improves";
        public const string NoEffect = "no_effect";
        public const string Worsens = "worsens";

        public static DiseaseData FindDiseaseForCompound(Compound compound, CountdownCodex codex)
        {
            foreach (var d in codex.diseases)
            {
                if (d.color == compound.Color && d.size == compound.Size && d.shape == compound.Shape)
                    return d;
            }
            return null;
        }

        // administeredDisease is null when the compound doesn't match any of the 14
        // diseases' real recipes.
        public static string OutcomeFor(DiseaseData trueDisease, DiseaseData administeredDisease)
        {
            if (administeredDisease == null)
                return NoEffect;

            if (administeredDisease.id == trueDisease.id)
                return Cure;

            int sharedSymptoms = SymptomOverlap(trueDisease, administeredDisease);
            return sharedSymptoms switch
            {
                3 => Improves,
                2 => NoEffect,
                _ => Worsens
            };
        }

        private static int SymptomOverlap(DiseaseData a, DiseaseData b)
        {
            int count = 0;
            foreach (var symptom in a.symptoms)
            {
                if (Array.IndexOf(b.symptoms, symptom) >= 0)
                    count++;
            }
            return count;
        }

        public static float HealthDeltaFor(string outcome) => outcome switch
        {
            Improves => 15f,
            Worsens => -20f,
            _ => 0f
        };
    }
}
