using System.Collections.Generic;
using System.Linq;
using Countdown.Data;

namespace Countdown.Runtime
{
    // Recomputes the shortlist from the full 14-disease list on every evidence event
    // (symptom reveal, blood draw, administer attempt) - never incrementally mutated.
    // This is deliberately the "boring, safe" choice: it's immune to ordering bugs and
    // automatically satisfies "scoped to current shortlist, not full codex" for
    // post-administer reveals, since all three filters (symptoms, blood, past
    // administer outcomes) are applied together every time - there's no code path
    // where one filter runs without the others.
    public static class ShortlistCalculator
    {
        public static List<DiseaseData> Compute(GameState state, CountdownCodex codex)
        {
            return codex.diseases
                .Where(d => MatchesSymptomEvidence(d, state))
                .Where(d => MatchesBloodEvidence(d, state))
                .Where(d => MatchesAdministerHistory(d, state, codex))
                .ToList();
        }

        private static bool MatchesSymptomEvidence(DiseaseData d, GameState state)
        {
            var trueSymptoms = state.CurrentDisease.symptoms;
            for (int i = 0; i < state.RevealedSymptomCount; i++)
            {
                if (d.symptoms[i] != trueSymptoms[i])
                    return false;
            }
            return true;
        }

        private static bool MatchesBloodEvidence(DiseaseData d, GameState state)
        {
            foreach (var draw in state.BloodDraws)
            {
                string value = draw.Attribute == "concentration" ? d.concentration : d.shape;
                if (value != draw.RevealedValue)
                    return false;
            }
            return true;
        }

        private static bool MatchesAdministerHistory(DiseaseData d, GameState state, CountdownCodex codex)
        {
            foreach (var attempt in state.AdministerHistory)
            {
                var administeredDisease = AdministerRules.FindDiseaseForCompound(attempt.Compound, codex);
                if (AdministerRules.OutcomeFor(d, administeredDisease) != attempt.OutcomeCategory)
                    return false;
            }
            return true;
        }
    }
}
