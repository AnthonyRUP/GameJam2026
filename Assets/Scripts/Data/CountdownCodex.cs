using System;
using System.Collections.Generic;

namespace Countdown.Data
{
    [Serializable]
    public class DiseaseData
    {
        public string id;
        public string name;
        public string tier; // "A" | "B" | "C"
        public string[] symptoms; // length 3, index = T1/T2/T3 reveal order
        public string color;
        public string concentration;
        public string shape;
        public string[] reagents_required; // [color, size, shape]
    }

    [Serializable]
    public class ReagentCategoryData
    {
        public string[] used;
        public string[] decoys;
    }

    [Serializable]
    public class ReagentShelfData
    {
        public ReagentCategoryData colors;
        public ReagentCategoryData concentrations;
        public ReagentCategoryData shapes;
    }

    [Serializable]
    public class SymptomThresholds
    {
        public float T1;
        public float T2;
        public float T3;
    }

    [Serializable]
    public class BloodTestConfig
    {
        public string[] testable_attributes;
        public int draws_needed_for_full_blood_certainty;
        public float cost_seconds_per_draw;
    }

    [Serializable]
    public class MechanicsData
    {
        public int health_start;
        public float base_decay_per_second;
        public SymptomThresholds symptom_reveal_health_thresholds;
        public BloodTestConfig blood_test;
    }

    [Serializable]
    public class CountdownCodex
    {
        public List<DiseaseData> diseases;
        public string[] symptom_pool;
        public ReagentShelfData reagent_shelf;
        public MechanicsData mechanics;
    }
}
