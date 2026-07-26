using UnityEngine;

/// <summary>
/// Reads movement from the generated InputSystem_Actions wrapper (Player/Move action)
/// and drives a Rigidbody2D via MovePosition. Flips the sprite horizontally based on
/// movement direction so a single-direction walk-cycle sheet can be reused for left/right.
/// Also drives a second, optional "hands" Animator (a child object holding the
/// scientist's bare-hands sprite/animation) with the same Speed parameter, so its
/// Idle_Hands/Walk_Hands clips stay perfectly in sync with the body's Idle/Walk -
/// PlayerInventory is what actually shows/hides the hands GameObject based on
/// whether something's being carried.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [Tooltip("Optional - the hands' own Animator (on the child hands object PlayerInventory toggles). Driven with the same Speed parameter and flip so it stays in sync with the body.")]
    [SerializeField] private Animator handsAnimator;
    [Tooltip("Optional - the hands' own SpriteRenderer, flipped to match the body's facing direction.")]
    [SerializeField] private SpriteRenderer handsRenderer;

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
        _actions ??= new InputSystem_Actions();
        _actions.Player.Enable();
    }

    private void OnDisable()
    {
        _actions?.Player.Disable();
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

        bool facingLeft = _moveInput.x < 0f;
        if (spriteRenderer != null && Mathf.Abs(_moveInput.x) > 0.01f)
            spriteRenderer.flipX = facingLeft;
        if (handsRenderer != null && Mathf.Abs(_moveInput.x) > 0.01f)
            handsRenderer.flipX = facingLeft;

        if (animator != null)
            animator.SetFloat(SpeedParam, _moveInput.sqrMagnitude);
        if (handsAnimator != null)
            handsAnimator.SetFloat(SpeedParam, _moveInput.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        if (_moveInput.sqrMagnitude <= 0.0001f)
            return;

        Vector2 nextPosition = _rb.position + _moveInput * (moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(nextPosition);
    }

    // Read by ScientistHelpPanel - any movement input closes the help popup, same
    // as pressing Escape closes a phase panel.
    public bool IsMoving => _moveInput.sqrMagnitude > 0.0001f;

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