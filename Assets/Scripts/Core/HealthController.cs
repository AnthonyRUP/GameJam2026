using Countdown.Data;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.Core
{
    // Always-ticking health decay, guarded only by IsGameOver - never by which panel is
    // open or whether the player is moving. Health keeps decaying during blood draws,
    // synthesis, everything, matching the brief's "continuous real-time decay" design.
    public class HealthController : MonoBehaviour
    {
        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State == null || gm.State.IsGameOver)
                return;

            var state = gm.State;
            var mechanics = gm.Codex.mechanics;

            state.Health -= mechanics.base_decay_per_second * Time.deltaTime;

            CheckSymptomThresholds(state, mechanics);

            if (state.Health <= 0f)
            {
                state.Health = 0f;
                state.IsGameOver = true;
                GameEvents.RaiseGameOver();
                gm.OpenPanel(GamePhase.GameOverLose);
            }
        }

        private static void CheckSymptomThresholds(GameState state, MechanicsData mechanics)
        {
            var t = mechanics.symptom_reveal_health_thresholds;
            int shouldBeRevealed = state.Health <= t.T3 ? 3 : state.Health <= t.T2 ? 2 : state.Health <= t.T1 ? 1 : 0;
            if (shouldBeRevealed > state.RevealedSymptomCount)
            {
                state.RevealedSymptomCount = shouldBeRevealed;
                GameEvents.RaiseSymptomRevealed();
            }
        }
    }
}
