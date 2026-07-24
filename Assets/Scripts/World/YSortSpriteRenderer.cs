using UnityEngine;

namespace Countdown.World
{
    // Sorts sprites by Y position so whatever is lower on screen renders in front -
    // gives correct "player walks in front of / behind a station" depth cues.
    // Fixes a real bug: previously Player and every station shared the exact same
    // fixed sortingOrder, so overlap order came down to an unstable tie-break that
    // Mono (Editor) and IL2CPP/WebAssembly (WebGL) resolved differently.
    // ExecuteAlways so this also keeps sortingOrder correct while just editing the
    // scene (not playing) - otherwise newly-placed objects sit at sortingOrder=0
    // until Play Mode first runs LateUpdate, which can tie/lose against other
    // objects in the meantime (misleading in the Scene view and in screenshots).
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSortSpriteRenderer : MonoBehaviour
    {
        // Large enough that no realistic Y position in the room pushes the result
        // below the Walls tilemap's fixed sortingOrder (1).
        private const int BaseOrder = 100;
        private const float Precision = 20f;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (_sr == null)
                _sr = GetComponent<SpriteRenderer>();
            _sr.sortingOrder = BaseOrder - Mathf.RoundToInt(transform.position.y * Precision);
        }
    }
}
