using System.Collections.Generic;
using Countdown.Data;
using Countdown.Runtime;
using UnityEngine;

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

        public CountdownCodex Codex { get; private set; }
        public GameState State { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Boot;

        private void Awake()
        {
            Instance = this;
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
            State.Shortlist = new List<DiseaseData>(Codex.diseases);
            CurrentPhase = GamePhase.Boot;
            Debug.Log($"New playthrough started. Disease: {disease.id} (tier {disease.tier}).");
        }

        // Opens a station panel as a modal overlay. Panel show/hide wiring itself is added
        // once the UI panels exist (Interactable stations step) - this only tracks phase
        // and locks player movement in the meantime.
        public void OpenPanel(GamePhase phase)
        {
            CurrentPhase = phase;
            if (player != null)
                player.SetInputEnabled(false);
        }

        public void ClosePanel()
        {
            CurrentPhase = GamePhase.Boot;
            if (player != null)
                player.SetInputEnabled(true);
        }
    }
}
