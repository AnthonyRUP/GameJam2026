using Countdown.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Countdown.World
{
    // Physical trigger zone for a station the player walks up to. OnInteract opens the
    // corresponding phase panel via GameManager - concrete per-station differentiation
    // (e.g. Injector's dual draw/administer role) is layered on top as those systems land.
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private GamePhase phaseToOpen;
        [SerializeField] private GameObject promptRoot; // shown when player is in range, optional

        protected bool _playerInRange;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (promptRoot != null) promptRoot.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (promptRoot != null) promptRoot.SetActive(false);
        }

        private void Update()
        {
            if (_playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                Interact();
        }

        public void Interact() => OnInteract();

        protected virtual void OnInteract()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OpenPanel(phaseToOpen);
        }
    }
}
