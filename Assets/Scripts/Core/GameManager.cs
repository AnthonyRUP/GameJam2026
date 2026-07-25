using System.Collections.Generic;
using Countdown.Data;
using Countdown.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.Core
{
    public enum GamePhase
    {
        Boot,
        Triage,
        BloodTest,
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
        [SerializeField] private GameObject synthesisPanel;
        [SerializeField] private GameObject administerPanel;

        public CountdownCodex Codex { get; private set; }
        public GameState State { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Boot;

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
            StartNewPlaythrough();
        }

        public void StartNewPlaythrough()
        {
            var disease = Codex.diseases[Random.Range(0, Codex.diseases.Count)];
            State = new GameState
            {
                CurrentDisease = disease,
                Health = Codex.mechanics.health_start
            };
            RecomputeShortlist();
            CurrentPhase = GamePhase.Boot;
            Debug.Log($"New playthrough started. Disease: {disease.id} (tier {disease.tier}).");
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

        // Called by the (not-yet-built) Administer panel when the player injects a compound.
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

            if (outcome == AdministerRules.Cure)
            {
                State.HasWon = true;
                State.IsGameOver = true;
                GameEvents.RaiseGameWon();
                OpenPanel(GamePhase.GameOverWin);
            }
            else if (State.Health <= 0f)
            {
                State.IsGameOver = true;
                GameEvents.RaiseGameOver();
                OpenPanel(GamePhase.GameOverLose);
            }

            return outcome;
        }

        // Opens a station panel as a modal overlay: shows the matching panel (if built
        // yet) and locks player movement while it's up. Free-roam phases (GamePhase has
        // no dedicated panel yet) still lock input so callers can rely on it uniformly.
        public void OpenPanel(GamePhase phase)
        {
            CurrentPhase = phase;
            SetPanelActive(triagePanel, phase == GamePhase.Triage);
            SetPanelActive(bloodTestPanel, phase == GamePhase.BloodTest);
            SetPanelActive(synthesisPanel, phase == GamePhase.Synthesis);
            SetPanelActive(administerPanel, phase == GamePhase.Administer);

            if (player != null)
                player.SetInputEnabled(false);
        }

        public void ClosePanel()
        {
            CurrentPhase = GamePhase.Boot;
            SetPanelActive(triagePanel, false);
            SetPanelActive(bloodTestPanel, false);
            SetPanelActive(synthesisPanel, false);
            SetPanelActive(administerPanel, false);

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
            bool panelOpen = CurrentPhase != GamePhase.Boot && CurrentPhase != GamePhase.GameOverWin && CurrentPhase != GamePhase.GameOverLose;
            if (panelOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ClosePanel();
        }
    }
}
