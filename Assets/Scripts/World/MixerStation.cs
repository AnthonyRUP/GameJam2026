using System.Collections;
using Countdown.Player;
using Countdown.Runtime;
using UnityEngine;

namespace Countdown.World
{
    // Interacting while carrying a reagent inserts it into one of the mixer's three
    // slots (color/concentration/shape) - a second reagent of a category already
    // filled is rejected outright. Filling a slot lights up one more indicator light
    // (order doesn't matter - it's just "how many are filled so far"). Once all
    // three are filled, the mixer stops accepting input, plays its closing
    // animation, holds closed for a beat (the "mixing" itself), then plays its
    // opening animation - only once fully open does the finished compound pop out
    // as a CompoundPickup, the same way a vial pops out of the Injector, rather
    // than teleporting straight into the player's hands.
    public class MixerStation : Interactable
    {
        private static readonly int CloseTrigger = Animator.StringToHash("Close");
        private static readonly int OpenTrigger = Animator.StringToHash("Open");

        [SerializeField] private Animator animator;
        [Tooltip("3 lights, left-to-right. Lit left-to-right as slots fill, regardless of which category filled them.")]
        [SerializeField] private GameObject[] slotLights;
        [Tooltip("Seconds the closing animation takes.")]
        [SerializeField] private float closeAnimationSeconds = 1f;
        [Tooltip("Seconds the mixer stays closed (mixing) before it opens again.")]
        [SerializeField] private float closedHoldSeconds = 1.5f;
        [Tooltip("Seconds the opening animation takes.")]
        [SerializeField] private float openAnimationSeconds = 1f;
        [SerializeField] private Transform sampleSpawnPoint;
        [SerializeField] private GameObject compoundPickupPrefab;

        private string _color;
        private string _concentration;
        private string _shape;
        private bool _busy;

        protected override void OnInteract()
        {
            if (_busy)
                return;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            var inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            if (inventory == null || inventory.Held != HeldItemKind.Reagent)
                return;

            string category = inventory.HeldReagentCategory;
            string value = inventory.HeldReagentValue;

            if (IsSlotFilled(category))
                return; // already holds one of this category - reject the duplicate

            FillSlot(category, value);
            inventory.Clear();
            UpdateLights();

            if (_color != null && _concentration != null && _shape != null)
            {
                var compound = new Compound { Color = _color, Concentration = _concentration, Shape = _shape };
                _color = null;
                _concentration = null;
                _shape = null;
                StartCoroutine(RunMixSequence(compound));
            }
        }

        private bool IsSlotFilled(string category) => category switch
        {
            "color" => _color != null,
            "concentration" => _concentration != null,
            "shape" => _shape != null,
            _ => true // unrecognized category - reject defensively
        };

        private void FillSlot(string category, string value)
        {
            switch (category)
            {
                case "color": _color = value; break;
                case "concentration": _concentration = value; break;
                case "shape": _shape = value; break;
            }
        }

        private void UpdateLights()
        {
            if (slotLights == null)
                return;

            int filled = 0;
            if (_color != null) filled++;
            if (_concentration != null) filled++;
            if (_shape != null) filled++;

            for (int i = 0; i < slotLights.Length; i++)
            {
                if (slotLights[i] != null)
                    slotLights[i].SetActive(i < filled);
            }
        }

        private IEnumerator RunMixSequence(Compound compound)
        {
            _busy = true;

            if (animator != null)
                animator.SetTrigger(CloseTrigger);
            yield return new WaitForSeconds(closeAnimationSeconds);

            yield return new WaitForSeconds(closedHoldSeconds);

            TurnOffLights(); // lights only reset now, right as the mixer opens

            if (animator != null)
                animator.SetTrigger(OpenTrigger);
            yield return new WaitForSeconds(openAnimationSeconds);

            SpawnCompoundPickup(compound);
            _busy = false;
        }

        private void TurnOffLights()
        {
            if (slotLights == null)
                return;

            foreach (var light in slotLights)
            {
                if (light != null)
                    light.SetActive(false);
            }
        }

        private void SpawnCompoundPickup(Compound compound)
        {
            if (compoundPickupPrefab == null || sampleSpawnPoint == null)
                return;

            var pickupGo = Instantiate(compoundPickupPrefab, sampleSpawnPoint.position, sampleSpawnPoint.rotation);
            var pickup = pickupGo.GetComponent<CompoundPickup>();
            if (pickup != null)
                pickup.Compound = compound;
        }
    }
}