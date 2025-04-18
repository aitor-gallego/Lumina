using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    // publicas
    [Header("movimiento")]
    public float speed = 10;
    public float acceleration = 20;
    public float deceleration = 20;
    public float velocity = 1;

    [Header("salto")]
    public float jump = 21;
    public float coyoteTime = 0.05f;
    public float jumpBufferTime = 0.1f;

    [Header("suelo")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.1f;

    [Header("animacion")]
    public SpriteRenderer sprite;

    // privadas
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls inputsystem;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isJumped;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        inputsystem = new PlayerControls();
        inputsystem.Player.SetCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateJumpTimers();
        Jump();
        Animations();
    }

    void FixedUpdate()
    {
        CheckGround();
        Movement();
    }

    // input system
    void OnEnable()
    {
        inputsystem.Enable();
    }

    void OnDisable()
    {
        inputsystem.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    // actualiza coyote time y jump buffer
    void UpdateJumpTimers()
    {
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;
    }

    // movimiento
    void Movement()
    {
        float xInput = moveInput.x;
        float target = xInput * speed;
        float difference = target - rb.linearVelocity.x;
        float type = (Mathf.Abs(target) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(difference) * type, velocity) * Mathf.Sign(difference);

        rb.AddForce(Vector2.right * movement);
    }

    // salto
    void Jump()
    {
        if (!isJumped && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            jumpBufferCounter = 0f;
            isJumped = true;
            coyoteTimeCounter = 0f;
        }
    }

    void CheckGround()
    {
        bool grounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);

        if (!grounded && isGrounded)
            isJumped = false;
    }

    // animaciones
    void Animations()
    {
        // parametros del animador
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

        // flip
        if (moveInput.x > 0.1f)
            sprite.flipX = false;
        else if (moveInput.x < -0.1f)
            sprite.flipX = true;
    }
}
