using Countdown.Core;
using TMPro;
using UnityEngine;

namespace Countdown.UI.Common
{
    // Displays how many patients have been cured this run - top-left HUD element.
    // Refreshes on GameEvents.OnNewPatient, which fires both when a new patient
    // begins after a cure (count went up) and after a restart (count reset to 0) -
    // so one subscription covers both cases with no extra logic needed here.
    public class ScoreCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string format = "Cured: {0}";

        private void OnEnable()
        {
            GameEvents.OnNewPatient += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnNewPatient -= Refresh;
        }

        private void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || label == null)
                return;

            label.text = string.Format(format, gm.PatientsCured);
        }
    }
}
