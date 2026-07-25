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
    // fresh sample in hand) always redisplays whatever's already been tested.
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
            Refresh();
        }

        private void Update()
        {
            if (!CanOfferNewTest())
                return;
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                RunTest("shape");
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
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
            return gm.State.BloodDraws.Count < maxDraws;
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
            }

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            var gm = GameManager.Instance;
            int maxDraws = gm.Codex.mechanics.blood_test.draws_needed_for_full_blood_certainty;

            if (gm.State.BloodDraws.Count >= maxDraws)
                promptText.text = "Analysis complete - no draws remaining.";
            else if (CanOfferNewTest())
                promptText.text = "Sample inserted.\n[1] Test Shape    [2] Test Concentration";
            else
                promptText.text = "No sample loaded.";
        }
    }
}
