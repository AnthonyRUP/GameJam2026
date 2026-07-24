using UnityEngine;

/// <summary>
/// Reads movement from the generated InputSystem_Actions wrapper (Player/Move action)
/// and drives a Rigidbody2D via MovePosition. Flips the sprite horizontally based on
/// movement direction so a single-direction walk-cycle sheet can be reused for left/right.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    private InputSystem_Actions _actions;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private bool _inputEnabled = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        _actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _actions.Player.Enable();
    }

    private void OnDisable()
    {
        _actions.Player.Disable();
    }

    private void OnDestroy()
    {
        _actions?.Dispose();
    }

    private void Update()
    {
        _moveInput = _inputEnabled ? _actions.Player.Move.ReadValue<Vector2>() : Vector2.zero;

        // Normalize diagonal input so diagonal movement isn't faster than axis-aligned movement.
        if (_moveInput.sqrMagnitude > 1f)
            _moveInput.Normalize();

        if (spriteRenderer != null && Mathf.Abs(_moveInput.x) > 0.01f)
            spriteRenderer.flipX = _moveInput.x < 0f;

        if (animator != null)
            animator.SetFloat(SpeedParam, _moveInput.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        if (_moveInput.sqrMagnitude <= 0.0001f)
            return;

        Vector2 nextPosition = _rb.position + _moveInput * (moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(nextPosition);
    }

    /// <summary>
    /// Enables/disables player movement input. Wired up later once UI panels exist
    /// (e.g. dialogue/pause screens should call SetInputEnabled(false)).
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled)
            _moveInput = Vector2.zero;
    }
}
