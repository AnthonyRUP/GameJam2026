using System.Collections;
using System.Collections.Generic;
using Countdown.Data;
using Countdown.Runtime;
using Countdown.UI.Common;
using Countdown.UI.Tutorial;
using Countdown.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.Core
{
    public enum GamePhase
    {
        Boot,
        Tutorial,
        Triage,
        BloodTest,
        Monitor,
        Book,
        DyeShelf,
        ShapeShelf,
        Synthesis,
        Administer,
        GameOverWin,
        GameOverLose
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private GameObject triagePanel;
        [SerializeField] private GameObject bloodTestPanel;
        [SerializeField] private GameObject monitorPanel;
        [SerializeField] private GameObject bookPanel;
        [SerializeField] private GameObject dyeShelfPanel;
        [SerializeField] private GameObject shapeShelfPanel;
        [SerializeField] private GameObject synthesisPanel;
        [SerializeField] private GameObject administerPanel;
        [SerializeField] private GameObject gameOverPanel;
        [Tooltip("One-shot boot walkthrough shown before the first patient loads. Optional - if unassigned, the game just starts immediately.")]
        [SerializeField] private TutorialPanel tutorialPanel;

        [Header("Patient transition")]
        [Tooltip("The test tank's doors - closes, patient swaps while hidden, then opens. Left unassigned, patients just swap instantly with no animation.")]
        [SerializeField] private PatientTransitionDoors transitionDoors;
        [Tooltip("Big flashing text shown on a cure (e.g. \"Cured!\"). Optional.")]
        [SerializeField] private CuredFlashText curedFlashText;

        public CountdownCodex Codex { get; private set; }
        public GameState State { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Boot;

        [Header("Difficulty progression")]
        [Tooltip("How many patients must be cured before the next tier (A -> B -> C) becomes possible to draw. Once unlocked, a tier stays in the pool alongside easier ones - it doesn't replace them.")]
        [SerializeField] private int patientsPerTierUnlock = 3;

        [Header("Game over")]
        [Tooltip("Seconds to wait after death (player already frozen, doors already closing) before the Game Over panel actually appears.")]
        [SerializeField] private float gameOverPanelDelaySeconds = 3f;

        private static readonly string[] TierOrder = { "A", "B", "C" };

        public int PatientsCured { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnSymptomRevealed += RecomputeShortlist;
        }

        private void OnDisable()
        {
            GameEvents.OnSymptomRevealed -= RecomputeShortlist;
        }

        private void Start()
        {
            StartCoroutine(CodexLoader.Load(OnCodexLoaded));
        }

        private void OnCodexLoaded(CountdownCodex codex)
        {
            Codex = codex;
            Debug.Log($"Countdown codex loaded: {codex.diseases.Count} diseases.");

            if (tutorialPanel != null)
            {
                CurrentPhase = GamePhase.Tutorial;
                if (player != null)
                    player.SetInputEnabled(false);
                tutorialPanel.Show(StartNewPlaythrough);
            }
            else
            {
                StartNewPlaythrough();
            }
        }

        public void StartNewPlaythrough()
        {
            PatientsCured = 0;
            transitionDoors?.ResetImmediately();
            BeginNextPatient();
        }

        // Sets up a fresh patient: new disease (picked from whatever tiers are
        // currently unlocked), full health, and every per-patient bit of state
        // (blood draws, administer history, revealed symptoms, shortlist) cleared
        // simply by virtue of being a brand new GameState.
        private void BeginNextPatient()
        {
            ClosePanel(); // don't leave a stale panel open across the transition

            var disease = PickDisease();
            State = new GameState
            {
                CurrentDisease = disease,
                Health = Codex.mechanics.health_start
            };
            RecomputeShortlist();
            CurrentPhase = GamePhase.Boot;
            GameEvents.RaiseNewPatient();
            Debug.Log($"New patient. Disease: {disease.id} (tier {disease.tier}). Patients cured so far: {PatientsCured}.");
        }

        // Picks a random disease from every tier unlocked so far. Tier N unlocks
        // once patientsPerTierUnlock * N patients have been cured - unlocked tiers
        // stay in the pool together, so difficulty ramps up gradually rather than
        // jumping straight to "only the hardest tier" the moment it's available.
        private DiseaseData PickDisease()
        {
            int unlockedTierIndex = Mathf.Min(PatientsCured / Mathf.Max(1, patientsPerTierUnlock), TierOrder.Length - 1);

            var pool = new List<DiseaseData>();
            foreach (var disease in Codex.diseases)
            {
                int tierIndex = System.Array.IndexOf(TierOrder, disease.tier);
                if (tierIndex >= 0 && tierIndex <= unlockedTierIndex)
                    pool.Add(disease);
            }

            if (pool.Count == 0)
                pool = Codex.diseases; // safety net - shouldn't normally happen

            return pool[Random.Range(0, pool.Count)];
        }

        public void RecomputeShortlist()
        {
            if (State == null || Codex == null)
                return;
            State.Shortlist = ShortlistCalculator.Compute(State, Codex);
            GameEvents.RaiseShortlistChanged();
        }

        // Called by the (not-yet-built) Blood Test panel once a draw resolves.
        public void RecordBloodDraw(string attribute, string revealedValue)
        {
            State.BloodDraws.Add(new BloodDrawResult { Attribute = attribute, RevealedValue = revealedValue });
            RecomputeShortlist();
        }

        // Called by InjectorStation when the player administers a compound.
        // Returns the resolved outcome category so the caller can drive its own UI feedback.
        public string RecordAdministerAttempt(Compound compound)
        {
            var administeredDisease = AdministerRules.FindDiseaseForCompound(compound, Codex);
            string outcome = AdministerRules.OutcomeFor(State.CurrentDisease, administeredDisease);

            State.AdministerHistory.Add(new AdministerAttempt
            {
                Compound = compound,
                OutcomeCategory = outcome
            });

            State.Health = Mathf.Clamp(State.Health + AdministerRules.HealthDeltaFor(outcome), 0f, Codex.mechanics.health_start);
            RecomputeShortlist();

            string administeredName = administeredDisease != null ? administeredDisease.id : "no matching disease";
            Debug.Log($"Administered [{compound.Color}, {compound.Concentration}, {compound.Shape}] ({administeredName}) vs true disease {State.CurrentDisease.id} -> {outcome.ToUpperInvariant()} (health now {State.Health:F1})");

            if (outcome == AdministerRules.Cure)
            {
                PatientsCured++;
                Debug.Log($"Patient cured! Total cured: {PatientsCured}.");
                GameEvents.RaiseGameWon(); // other systems can still react to this
                StartCoroutine(RunCureTransition());
            }
            else if (State.Health <= 0f)
            {
                HandleLose();
            }

            return outcome;
        }

        // Flashes the "Cured!" text and, if doors are assigned, closes them, swaps
        // the patient while hidden, then opens them again. With no doors assigned,
        // this just swaps the patient immediately (matches old behavior).
        private IEnumerator RunCureTransition()
        {
            if (curedFlashText != null)
                StartCoroutine(curedFlashText.Flash());

            if (transitionDoors != null)
                yield return StartCoroutine(transitionDoors.PlayPatientSwap(BeginNextPatient));
            else
                BeginNextPatient();
        }

        // Single source of truth for "the patient died" - called both when a bad
        // compound finishes them off (here) and when the health countdown simply
        // runs out (HealthController). Guards against double-triggering. Player
        // input freezes and the doors start closing immediately; only the panel
        // itself is delayed, so the moment of death still feels instant.
        public void HandleLose()
        {
            if (State.IsGameOver)
                return;

            State.IsGameOver = true;
            GameEvents.RaiseGameOver();

            if (player != null)
                player.SetInputEnabled(false);

            if (transitionDoors != null)
                StartCoroutine(transitionDoors.PlayFinalClose());

            StartCoroutine(ShowGameOverPanelAfterDelay());
        }

        private IEnumerator ShowGameOverPanelAfterDelay()
        {
            yield return new WaitForSeconds(gameOverPanelDelaySeconds);
            OpenPanel(GamePhase.GameOverLose);
        }

        // Opens a station panel as a modal overlay: shows the matching panel (if built
        // yet) and locks player movement while it's up. Free-roam phases (GamePhase has
        // no dedicated panel yet) still lock input so callers can rely on it uniformly.
        public void OpenPanel(GamePhase phase)
        {
            CurrentPhase = phase;
            SetPanelActive(triagePanel, phase == GamePhase.Triage);
            SetPanelActive(bloodTestPanel, phase == GamePhase.BloodTest);
            SetPanelActive(monitorPanel, phase == GamePhase.Monitor);
            SetPanelActive(bookPanel, phase == GamePhase.Book);
            SetPanelActive(dyeShelfPanel, phase == GamePhase.DyeShelf);
            SetPanelActive(shapeShelfPanel, phase == GamePhase.ShapeShelf);
            SetPanelActive(synthesisPanel, phase == GamePhase.Synthesis);
            SetPanelActive(administerPanel, phase == GamePhase.Administer);
            SetPanelActive(gameOverPanel, phase == GamePhase.GameOverLose);

            if (player != null)
                player.SetInputEnabled(false);
        }

        public void ClosePanel()
        {
            CurrentPhase = GamePhase.Boot;
            SetPanelActive(triagePanel, false);
            SetPanelActive(bloodTestPanel, false);
            SetPanelActive(monitorPanel, false);
            SetPanelActive(bookPanel, false);
            SetPanelActive(dyeShelfPanel, false);
            SetPanelActive(shapeShelfPanel, false);
            SetPanelActive(synthesisPanel, false);
            SetPanelActive(administerPanel, false);
            SetPanelActive(gameOverPanel, false);

            if (player != null)
                player.SetInputEnabled(true);
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        private void Update()
        {
            bool panelOpen = CurrentPhase != GamePhase.Boot && CurrentPhase != GamePhase.Tutorial
                && CurrentPhase != GamePhase.GameOverWin && CurrentPhase != GamePhase.GameOverLose;
            if (panelOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ClosePanel();
        }
    }
}