using System.Collections.Generic;
using System.Text;
using Countdown.Core;
using Countdown.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.UI.Book
{
    // Reference book: a two-page spread showing two diseases at once (left = an even
    // index, right = the next odd index), Left/Right flips a whole spread at a time -
    // 1&2, then 3&4, then 5&6, etc. Shows the full 14-disease codex regardless of the
    // current playthrough - this is static reference material, not scoped to the
    // shortlist. Per page: name up top (centered), symptoms below it, then the cure
    // as plain text just below that - a visual glyph (real shape/color/dots) made the
    // cure trivially pattern-matchable at a glance while flipping pages, undercutting
    // the symptom-based diagnosis loop, so this is deliberately just words.
    public class BookPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI leftDiseaseNameLabel;
        [SerializeField] private TextMeshProUGUI leftSymptomsLabel;
        [SerializeField] private TextMeshProUGUI leftReagentsLabel;
        [SerializeField] private TextMeshProUGUI leftPageIndicatorLabel;

        [SerializeField] private TextMeshProUGUI rightDiseaseNameLabel;
        [SerializeField] private TextMeshProUGUI rightSymptomsLabel;
        [SerializeField] private TextMeshProUGUI rightReagentsLabel;
        [SerializeField] private TextMeshProUGUI rightPageIndicatorLabel;

        private int _spreadIndex;

        private void OnEnable()
        {
            _spreadIndex = 0;
            Refresh();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Codex == null || Keyboard.current == null)
                return;

            int spreadCount = Mathf.CeilToInt(gm.Codex.diseases.Count / 2f);
            if (spreadCount == 0)
                return;

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame && _spreadIndex > 0)
            {
                _spreadIndex--;
                Refresh();
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame && _spreadIndex < spreadCount - 1)
            {
                _spreadIndex++;
                Refresh();
            }
        }

        private void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Codex == null)
                return;

            var diseases = gm.Codex.diseases;
            int leftIndex = _spreadIndex * 2;
            int rightIndex = leftIndex + 1;

            SetPage(leftDiseaseNameLabel, leftSymptomsLabel, leftReagentsLabel, leftPageIndicatorLabel, diseases, leftIndex);
            SetPage(rightDiseaseNameLabel, rightSymptomsLabel, rightReagentsLabel, rightPageIndicatorLabel, diseases, rightIndex);
        }

        private static void SetPage(TextMeshProUGUI nameLabel, TextMeshProUGUI symptomsLabel, TextMeshProUGUI reagentsLabel,
            TextMeshProUGUI pageLabel, List<DiseaseData> diseases, int index)
        {
            bool valid = index >= 0 && index < diseases.Count;
            var disease = valid ? diseases[index] : null;

            if (nameLabel != null)
                nameLabel.text = valid ? disease.name : "";

            if (symptomsLabel != null)
                symptomsLabel.text = valid ? FormatSymptoms(disease.symptoms) : "";

            if (reagentsLabel != null)
                reagentsLabel.text = valid ? FormatReagents(disease.color, disease.concentration, disease.shape) : "";

            if (pageLabel != null)
                pageLabel.text = valid ? $"{index + 1} / {diseases.Count}" : "";
        }

        private static string FormatReagents(string color, string concentration, string shape) =>
            $"Cure: {Prettify(color)}, {Prettify(concentration)}, {Prettify(shape)}";

        private static string FormatSymptoms(string[] symptoms)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < symptoms.Length; i++)
            {
                if (i > 0)
                    sb.Append('\n');
                sb.Append("- ").Append(Prettify(symptoms[i]));
            }
            return sb.ToString();
        }

        private static string Prettify(string symptomId)
        {
            var words = symptomId.Split('_');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
            return string.Join(' ', words);
        }
    }
}
