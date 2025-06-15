using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerControls.IPlayerActions
{
    public static bool inputBlocked = false;

    // publicas
    [Header("movimiento")]
    public float speed = 6;
    public float acceleration = 25;
    public float deceleration = 40;

    [Header("salto")]
    public float jump = 22;
    public float coyoteTime = 0.05f;
    public float jumpBufferTime = 0.1f;
    public float fallMultiplier = 5f;

    [Header("suelo")]
    public bool Landed { get; private set; }
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.15f;

    [Header("animacion")]
    public SpriteRenderer sprite;

    [Header("push y pull")]
    [SerializeField] private float pushRadio = 0.25f;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private float holdOffsetX = 0.7f;

    [Header("particulas")]
    public ParticleSystem walkParticles;
    public ParticleSystem jumpParticles;

    [Header("sfx")]
    public AudioClip jumpSFX;
    public AudioClip pushSFX;

    // privadas
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls inputsystem;
    private Vector2 moveInput;
    private bool grounded;
    private bool jumped;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private FixedJoint2D pushPullJoint;
    private GameObject currentBox;
    private Collider2D currentBoxCollider;
    private Rigidbody2D currentBoxRb;
    private SpriteRenderer currentBoxSprite;
    private float currentBoxDir;
    private int boxSortOrder;
    private bool reseting;
    private float resetHoldTime = 0.3f;
    private float resetTimer = 0f;
    private bool resetInput = true;
    private bool resetCompleted = false;
    private float lastNonZeroX = 1f;
    private float rotateangle = 1f;
    private float rotateHoldTime = 0f;
    private float rotateDelay = 0.3f;
    private float rotatetimer = 0f;
    private bool rotatingleft = false;
    private bool rotatingright = false;
    private float rotatingspeed = 0.02f;

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

        foreach (Rigidbody2D box in FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            if (((1 << box.gameObject.layer) & boxLayer) != 0)
            {
                box.constraints = RigidbodyConstraints2D.FreezePositionX;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastNonZeroX = sprite.flipX ? -1f : 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;

        UpdateJumpTimers();
        Jump();
        Animations();

        if (rb.linearVelocity.y < 0 && jumped)
        {
            jumped = false;
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

        if (currentBox != null)
        {
            if (rotatingleft)
            {
                rotateHoldTime += Time.deltaTime;

                if (rotateHoldTime >= rotateDelay)
                {
                    rotatetimer += Time.deltaTime;
                    if (rotatetimer >= rotatingspeed)
                    {
                        RotateBox(rotateangle);
                        rotatetimer = 0f;
                    }
                }
            }

            if (rotatingright)
            {
                rotateHoldTime += Time.deltaTime;

                if (rotateHoldTime >= rotateDelay)
                {
                    rotatetimer += Time.deltaTime;
                    if (rotatetimer >= rotatingspeed)
                    {
                        RotateBox(-rotateangle);
                        rotatetimer = 0f;
                    }
                }
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

            if (!boxGrounded || !grounded)
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

        Vector2 rawInput = context.ReadValue<Vector2>();
        moveInput = new Vector2(Mathf.Abs(rawInput.x) < 0.15f ? 0f : rawInput.x, rawInput.y);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0 || inputBlocked) return;

        if (context.started)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    public void OnPushPull(InputAction.CallbackContext context)
    {
        if (!grounded || inputBlocked || context.started == false) return;

        if (currentBox == null)
        {
            TryAttachBox();
        }
        else
        {
            DetachBox();
        }
    }

    public void OnRotateLeft(InputAction.CallbackContext context)
    {
        if (!pushPullJoint.enabled || inputBlocked || currentBox == null) return;

        if (context.started)
        {
            RotateBox(rotateangle);
            rotatingleft = true;
            rotateHoldTime = 0f;
            rotatetimer = 0f;
        }
        else if (context.canceled)
        {
            rotatingleft = false;
        }
    }

    public void OnRotateRight(InputAction.CallbackContext context)
    {
        if (!pushPullJoint.enabled || inputBlocked || currentBox == null) return;

        if (context.started)
        {
            RotateBox(-rotateangle);
            rotatingright = true;
            rotateHoldTime = 0f;
            rotatetimer = 0f;
        }
        else if (context.canceled)
        {
            rotatingright = false;
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

    // move
    void Movement()
    {
        float targetSpeed = moveInput.x * speed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 1) * Mathf.Sign(speedDif);
        Vector2 final = new Vector2(rb.linearVelocity.x + movement * Time.fixedDeltaTime, rb.linearVelocity.y);
        rb.linearVelocity = final;
    }

    // jump
    void Jump()
    {
        if (!jumped && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            jumpBufferCounter = 0f;
            jumped = true;
            coyoteTimeCounter = 0f;
            if (currentBox != null)
                DetachBox();

            AudioController.Instance.PlaySFX(jumpSFX);
        }
    }

    void UpdateJumpTimers()
    {
        if (grounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    void CheckGround()
    {
        bool groundedBefore = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundLayer);

        Landed = !groundedBefore && grounded;

        if (Landed)
        {
            jumped = false;

            if (jumpParticles != null)
            {
                jumpParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                jumpParticles.Play();
            }
        }
    }

    // rotate
    private void RotateBox(float angle)
    {
        if (currentBox == null) return;

        Transform child = currentBox.transform.childCount > 0 ? currentBox.transform.GetChild(0) : null;
        if (child != null)
        {
            child.Rotate(0f, 0f, angle);
        }
    }


    // box
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
        boxSortOrder = currentBoxSprite.sortingOrder;

        currentBoxDir = facingRight ? 1f : -1f;
        Vector2 worldOffset = new(currentBoxDir * holdOffsetX, 0);

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
            currentBoxSprite.sortingOrder = boxSortOrder;
            currentBoxSprite = null;
        }

        pushPullJoint.connectedBody = null;
        pushPullJoint.enabled = false;
        currentBoxRb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("isPushing", false);

        rotatingleft = false;
        rotatingright = false;
        rotateHoldTime = 0f;
        rotatetimer = 0f;

        currentBox = null;
        currentBoxRb = null;
        currentBoxCollider = null;

        AudioController.Instance.StopLooping();
    }


    // reset
    public void Reset()
    {
        SceneTransitionController.Instance.ResetScene();
    }

    // animaciones
    void Animations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isGrounded", grounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

        if (animator.GetBool("isPushing"))
        {
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                animator.SetBool("pushWalk", true);
                animator.SetBool("pushIdle", false);

                if (pushSFX != null)
                    AudioController.Instance.PlayLooping(pushSFX);
            }
            else
            {
                animator.SetBool("pushWalk", false);
                animator.SetBool("pushIdle", true);

                AudioController.Instance.StopLooping();
            }
        }
        else
        {
            animator.SetBool("pushWalk", false);
            animator.SetBool("pushIdle", false);
            AudioController.Instance.StopLooping();

            if (Mathf.Abs(moveInput.x) > 0.2f)
                lastNonZeroX = Mathf.Sign(moveInput.x);

            sprite.flipX = lastNonZeroX < 0f;
        }
    }
}