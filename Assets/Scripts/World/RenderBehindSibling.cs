using UnityEngine;

namespace Countdown.World
{
    // Keeps this renderer's sortingOrder pinned a fixed number of steps from another
    // renderer - e.g. a patient sprite that must always render inside/behind its
    // container regardless of the container's own dynamic Y-sort value, or a held-item
    // icon that must always render just in front of the player (unlike
    // YSortSpriteRenderer, which is for things that should dynamically re-sort against
    // the player as it moves).
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class RenderBehindSibling : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private int offset = -1;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (_sr == null)
                _sr = GetComponent<SpriteRenderer>();
            if (target == null)
                return;
            _sr.sortingOrder = target.sortingOrder + offset;
        }
    }
}
