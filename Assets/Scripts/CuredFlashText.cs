using System.Collections;
using TMPro;
using UnityEngine;

namespace Countdown.UI.Common
{
    // Big text that flashes on screen briefly (fade in, hold, fade out) then hides
    // itself again. GameManager triggers this on a cure; it does nothing on its own.
    public class CuredFlashText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private string message = "Cured!";
        [SerializeField] private float fadeInSeconds = 0.25f;
        [SerializeField] private float holdSeconds = 1.2f;
        [SerializeField] private float fadeOutSeconds = 0.4f;

        private void Awake()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        public IEnumerator Flash()
        {
            gameObject.SetActive(true);
            if (label != null)
                label.text = message;

            yield return Fade(0f, 1f, fadeInSeconds);
            yield return new WaitForSeconds(holdSeconds);
            yield return Fade(1f, 0f, fadeOutSeconds);

            gameObject.SetActive(false);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (canvasGroup == null)
                yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}