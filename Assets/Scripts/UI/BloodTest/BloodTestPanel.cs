using Countdown.Core;
using Countdown.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.UI.BloodTest
{
    // The blood tester holds up to 2 draws for the whole playthrough. Each draw tests
    // exactly one attribute (concentration or shape) and its result is written into
    // GameState - it persists there, so reopening this panel later (with or without a
    // fresh sample in hand) always redisplays whatever's already been tested. Each
    // attribute can only ever be tested once - testing shape twice would burn both
    // draws and permanently lock the player out of ever learning concentration.
    public class BloodTestPanel : MonoBehaviour
    {
        [SerializeField] private BloodGlyphView[] slotGlyphs;
        [SerializeField] private GameObject[] slotEmptyMarkers;
        [SerializeField] private TextMeshProUGUI promptText;

        private PlayerInventory _inventory;

        private void OnEnable()
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            _inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            GameEvents.OnNewPatient += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnNewPatient -= Refresh;
        }

        private void Update()
        {
            if (!CanOfferNewTest())
                return;
            if (Keyboard.current == null)
                return;

            if ((Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                && !IsAttributeTested("shape"))
                RunTest("shape");
            else if ((Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                && !IsAttributeTested("concentration"))
                RunTest("concentration");
        }

        private bool CanOfferNewTest()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || _inventory == null)
                return false;
            if (_inventory.Held != HeldItemKind.BloodSample)
                return false;

            int maxDraws = gm.Codex.mechanics.blood_test.draws_needed_for_full_blood_certainty;
            if (gm.State.BloodDraws.Count >= maxDraws)
                return false;

            // Even with draws remaining, there's nothing left to offer once both
            // attributes have already been tested.
            return !IsAttributeTested("shape") || !IsAttributeTested("concentration");
        }

        private bool IsAttributeTested(string attribute)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return false;

            var draws = gm.State.BloodDraws;
            for (int i = 0; i < draws.Count; i++)
            {
                if (draws[i].Attribute == attribute)
                    return true;
            }
            return false;
        }

        private void RunTest(string attribute)
        {
            var gm = GameManager.Instance;
            var disease = gm.State.CurrentDisease;
            string revealedValue = attribute == "shape" ? disease.shape : disease.concentration;

            _inventory.Clear();
            gm.RecordBloodDraw(attribute, revealedValue);
            Refresh();
        }

        private void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null)
                return;

            var draws = gm.State.BloodDraws;
            var disease = gm.State.CurrentDisease;

            for (int i = 0; i < slotGlyphs.Length; i++)
            {
                bool filled = i < draws.Count;
                slotGlyphs[i].gameObject.SetActive(filled);
                if (slotEmptyMarkers[i] != null)
                    slotEmptyMarkers[i].SetActive(!filled);

                if (filled)
                {
                    var draw = draws[i];
                    slotGlyphs[i].Configure(draw.Attribute, disease.concentration, disease.shape);
                }
                else
                {
                    slotGlyphs[i].Hide();
                }
            }

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            var gm = GameManager.Instance;
            int maxDraws = gm.Codex.mechanics.blood_test.draws_needed_for_full_blood_certainty;

            if (gm.State.BloodDraws.Count >= maxDraws)
            {
                promptText.text = "Analysis complete - no draws remaining.";
                return;
            }

            if (_inventory == null || _inventory.Held != HeldItemKind.BloodSample)
            {
                promptText.text = "No sample loaded.";
                return;
            }

            bool shapeTested = IsAttributeTested("shape");
            bool concentrationTested = IsAttributeTested("concentration");

            if (shapeTested && concentrationTested)
            {
                promptText.text = "Both attributes already tested.";
                return;
            }

            string prompt = "Sample inserted.\n";
            if (!shapeTested)
                prompt += "[1] Test Shape    ";
            if (!concentrationTested)
                prompt += "[2] Test Concentration";
            promptText.text = prompt;
        }
    }
}