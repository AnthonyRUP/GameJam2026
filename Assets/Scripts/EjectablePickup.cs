using System.Collections;
using Countdown.Player;
using UnityEngine;

namespace Countdown.World
{
    // Shared behavior for anything that pops out of a machine with a single hop and
    // can only be picked up once it's settled (BloodSamplePickup, CompoundPickup).
    // Subclasses just say what "successfully collected" means for them - the
    // eject animation, settle-gating, and collider safety are all handled here once.
    [RequireComponent(typeof(Collider2D))]
    public abstract class EjectablePickup : Interactable, ISettleGate
    {
        [Header("Eject pop")]
        [Tooltip("The child transform holding the SpriteRenderer - this is what arcs upward. Leave unset to skip the arc (root still slides to its resting spot).")]
        [SerializeField] private Transform visualSprite;
        [Tooltip("World-space direction it pops toward, e.g. (0,-1) to come out toward the bottom of the screen.")]
        [SerializeField] private Vector2 ejectDirection = new(0f, -1f);
        [Tooltip("Ground distance from the spawn point to where it finally rests.")]
        [SerializeField] private float ejectDistance = 0.4f;
        [Tooltip("How high the hop arcs, in world units.")]
        [SerializeField] private float ejectHeight = 0.25f;
        [Tooltip("How long the whole hop takes, in seconds.")]
        [SerializeField] private float ejectDuration = 0.3f;

        private bool _landed;

        public bool HasSettled => _landed;

        private void Awake()
        {
            // Belt-and-suspenders: a non-trigger collider here would physically shove
            // the player and never fire OnTriggerEnter2D, softlocking movement and
            // whatever machine is waiting for this to be collected. Force it.
            var col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Start()
        {
            StartCoroutine(PlayEjectAnimation());
        }

        protected override void OnInteract()
        {
            if (!_landed)
                return;

            var playerGo = GameObject.FindGameObjectWithTag("Player");
            var inventory = playerGo != null ? playerGo.GetComponent<PlayerInventory>() : null;
            if (inventory == null)
                return;

            if (TryCollect(inventory))
                Destroy(gameObject);
        }

        // Return true if the item was successfully handed to the inventory (e.g. the
        // player had a free hand). Returning false leaves this pickup in the world.
        protected abstract bool TryCollect(PlayerInventory inventory);

        private IEnumerator PlayEjectAnimation()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)(ejectDirection.normalized * ejectDistance);

            float elapsed = 0f;
            while (elapsed < ejectDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ejectDuration);

                transform.position = Vector3.Lerp(startPos, endPos, t);

                if (visualSprite != null)
                {
                    // Simple upward arc: 0 at t=0 and t=1, peak at t=0.5
                    float arc = 4f * ejectHeight * t * (1f - t);
                    visualSprite.localPosition = new Vector3(0f, arc, 0f);
                }

                yield return null;
            }

            transform.position = endPos;
            if (visualSprite != null)
                visualSprite.localPosition = Vector3.zero;

            _landed = true;
        }
    }
}
