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
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("suelo")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.15f;

    [Header("animacion")]
    public SpriteRenderer sprite;

    [Header("push y pull")]
    [SerializeField] private float pushRadio = 0.25f;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private float holdOffsetX = 0.7f;

    // privadas
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls inputsystem;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isJumped;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private FixedJoint2D pushPullJoint;
    private GameObject currentBox;
    private Collider2D currentBoxCollider;
    private Rigidbody2D currentBoxRb;
    private SpriteRenderer currentBoxSprite;
    private int originalBoxSortingOrder;
    private float currentHoldDir;
    private float rotationAngle = 90f;
    private bool reseting;
    private float resetHoldTime = 0.3f;
    private float resetTimer = 0f;
    private bool resetInput = true;
    private bool resetCompleted = false;
    public static bool inputBlocked = false;
    private bool jumpHeld;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Awake()
    {
        inputBlocked = false;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        inputsystem = new PlayerControls();
        inputsystem.Player.SetCallbacks(this);
        pushPullJoint = gameObject.AddComponent<FixedJoint2D>();
        pushPullJoint.enabled = false;

        foreach (Rigidbody2D boxRb in FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            if (((1 << boxRb.gameObject.layer) & boxLayer) != 0)
            {
                boxRb.constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;

        UpdateJumpTimers();
        Jump();
        Animations();

        if (rb.linearVelocity.y < 0 && isJumped)
        {
            isJumped = false;
        }

        if (reseting)
        {
            resetTimer += Time.deltaTime;

            if (resetTimer >= resetHoldTime && !resetCompleted)
            {
                Reset();
                resetCompleted = true;
            }
        }

    }

    void FixedUpdate()
    {
        CheckGround();
        Movement();

        if (currentBox != null && currentBoxCollider != null)
        {
            Bounds b = currentBoxCollider.bounds;

            float checkThickness = groundCheckDistance;

            Vector2 bottomLeft = new(b.min.x, b.min.y - checkThickness * 0.5f);
            Vector2 topRight = new(b.max.x, b.min.y + checkThickness * 0.5f);

            bool boxGrounded = Physics2D.OverlapArea(bottomLeft, topRight, groundLayer) != null;

            if (!boxGrounded || !isGrounded)
                DetachBox();
        }
    }


    // input system
    public void OnEnable()
    {
        if (inputsystem != null)
            inputsystem.Enable();
    }

    public void OnDisable()
    {
        if (inputsystem != null)
            inputsystem.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputBlocked) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0 || inputBlocked) return;

        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpHeld = true;
        }

        if (context.canceled)
        {
            jumpHeld = false;
        }
    }
    public void OnPushPull(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;
        if (inputBlocked) return;

        if (context.started)
            TryAttachBox();
        else if (context.canceled)
            DetachBox();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (!pushPullJoint.enabled) return;
        if (inputBlocked) return;

        if (context.started && currentBox != null)
        {
            Transform child = currentBox.transform.childCount > 0 ? currentBox.transform.GetChild(0) : null;
            child.Rotate(0f, 0f, rotationAngle);
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (context.started && resetInput && !resetCompleted)
        {
            reseting = true;
            resetTimer = 0f;
            resetInput = false;

            animator.SetTrigger("Reseting");
        }
        else if (context.canceled)
        {
            reseting = false;
            resetTimer = 0f;
            resetCompleted = false;
            resetInput = true;
        }
    }

    void UpdateJumpTimers()
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    void Movement()
    {
        float targetSpeed = moveInput.x * speed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velocity) * Mathf.Sign(speedDif);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement * Time.fixedDeltaTime, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (!isJumped && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            jumpBufferCounter = 0f;
            isJumped = true;
            coyoteTimeCounter = 0f;
            if (currentBox != null) DetachBox();
        }

        // salto variable
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void CheckGround()
    {
        bool grounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);
        if (!grounded && isGrounded) isJumped = false;
    }

    void Animations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

        if (animator.GetBool("isPushing"))
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                animator.SetBool("pushWalk", true);
                animator.SetBool("pushIdle", false);
            }
            else
            {
                animator.SetBool("pushWalk", false);
                animator.SetBool("pushIdle", true);
            }
        }
        else
        {
            animator.SetBool("pushWalk", false);
            animator.SetBool("pushIdle", false);

            if (moveInput.x > 0.01f)
                sprite.flipX = false;
            else if (moveInput.x < -0.01f)
                sprite.flipX = true;
        }
    }

    private void TryAttachBox()
    {
        if (currentBox != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, pushRadio, boxLayer);
        if (hit == null) return;

        bool facingRight = !sprite.flipX;
        float deltaX = hit.transform.position.x - transform.position.x;
        if ((facingRight && deltaX < 0) || (!facingRight && deltaX > 0)) return;

        currentBox = hit.gameObject;
        currentBoxCollider = currentBox.GetComponent<Collider2D>();
        currentBoxRb = currentBox.GetComponent<Rigidbody2D>();
        currentBoxSprite = currentBox.GetComponent<SpriteRenderer>();
        originalBoxSortingOrder = currentBoxSprite.sortingOrder;

        currentHoldDir = facingRight ? 1f : -1f;
        Vector2 worldOffset = new(currentHoldDir * holdOffsetX, 0);

        currentBoxRb.position = (Vector2)transform.position + worldOffset;
        currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        pushPullJoint.connectedBody = currentBoxRb;
        pushPullJoint.autoConfigureConnectedAnchor = false;
        pushPullJoint.anchor = Vector2.zero;
        pushPullJoint.connectedAnchor = -worldOffset;
        pushPullJoint.enableCollision = false;
        pushPullJoint.enabled = true;

        animator.SetBool("isPushing", true);
    }

    private void DetachBox()
    {
        if (!pushPullJoint.enabled) return;
        if (currentBoxSprite != null)
        {
            currentBoxSprite.sortingOrder = originalBoxSortingOrder;
            currentBoxSprite = null;
        }
        pushPullJoint.connectedBody = null;
        pushPullJoint.enabled = false;
        currentBoxRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        animator.SetBool("isPushing", false);
        currentBox = null;
        currentBoxRb = null;
        currentBoxCollider = null;
    }

    public void Reset()
    {
        SceneTransitionController.Instance.ResetScene();
    }
}