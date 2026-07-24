using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Countdown.UI.Common
{
    public enum MatchState
    {
        Neutral,
        Confirmed,
        RuledOut,
        Selected
    }

    // Generic "compare evidence to a reference" row/tile, reused for the Disease Book's
    // per-symptom checklist rows and the Synthesis shelf's reagent items.
    public class MatchableItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image highlightBg;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button button;

        private static readonly Color NeutralColor = new(1f, 1f, 1f, 0.08f);
        private static readonly Color ConfirmedColor = new(0.3f, 0.8f, 0.4f, 0.5f);
        private static readonly Color RuledOutColor = new(0.5f, 0.5f, 0.5f, 0.3f);
        private static readonly Color SelectedColor = new(0.3f, 0.6f, 1f, 0.6f);

        public void Configure(string labelText, Sprite iconSprite, MatchState state, Action onClick = null)
        {
            if (label != null)
                label.text = labelText;

            if (icon != null)
            {
                icon.sprite = iconSprite;
                icon.enabled = iconSprite != null;
            }

            if (highlightBg != null)
            {
                highlightBg.color = state switch
                {
                    MatchState.Confirmed => ConfirmedColor,
                    MatchState.RuledOut => RuledOutColor,
                    MatchState.Selected => SelectedColor,
                    _ => NeutralColor
                };
            }

            if (button != null)
            {
                button.interactable = onClick != null;
                button.onClick.RemoveAllListeners();
                if (onClick != null)
                    button.onClick.AddListener(() => onClick());
            }
        }
    }
}
