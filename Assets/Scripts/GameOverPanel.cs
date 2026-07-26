using Countdown.Core;
using UnityEngine;

namespace Countdown.UI.Common
{
    // The "patient died" screen. Restart wipes the score (PatientsCured resets to 0)
    // and spawns a fresh patient - GameManager.StartNewPlaythrough() -> BeginNextPatient()
    // already closes this panel and re-enables player input as part of that sequence,
    // so this button doesn't need to do anything else itself.
    public class GameOverPanel : MonoBehaviour
    {
        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewPlaythrough();
        }

        // Hook up your second button's handler here once you've decided what it does.
    }
}
