using System.Collections.Generic;
using UnityEngine;

namespace Countdown.World
{
    // Single arbiter for "which nearby interactable, if any, actually responds right
    // now." Every Interactable registers itself here while the player's inside its
    // trigger; each frame this picks the closest eligible one and exposes it as
    // ActiveInteractable. Interactable only accepts an E-press, and StationHighlight
    // only shows itself, when they match this - so if you're standing between two
    // overlapping stations, exactly one of them can react to you at all, never both.
    public class NearestInteractableSelector : MonoBehaviour
    {
        public static NearestInteractableSelector Instance { get; private set; }

        private readonly List<Interactable> _candidates = new();
        private Transform _player;

        public Interactable ActiveInteractable { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_candidates.Count == 0)
            {
                ActiveInteractable = null;
                return;
            }

            if (_player == null)
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                _player = playerGo != null ? playerGo.transform : null;
                if (_player == null)
                {
                    ActiveInteractable = null;
                    return;
                }
            }

            Interactable closest = null;
            float closestSqrDist = float.MaxValue;

            foreach (var candidate in _candidates)
            {
                if (candidate == null || !candidate.CanBeActive)
                    continue;

                float sqrDist = (candidate.transform.position - _player.position).sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closest = candidate;
                }
            }

            ActiveInteractable = closest;
        }

        public void Register(Interactable interactable)
        {
            if (!_candidates.Contains(interactable))
                _candidates.Add(interactable);
        }

        public void Unregister(Interactable interactable)
        {
            _candidates.Remove(interactable);
            if (ActiveInteractable == interactable)
                ActiveInteractable = null;
        }
    }
}
