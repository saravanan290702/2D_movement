using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class playerMovement : MonoBehaviour
{
    [Header("basic")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask _cornerCorrectLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private TrailRenderer trail1;
    [SerializeField] private float _fallClamp = -40f;

    private bool groundTouch;

    private void Start()
    {
        _jumpsLeft = maxJumps;
    }

    void Update()
    {
        horizontal = GetInput().x;
        vertical = GetInput().y;
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(horizontal,vertical);

        Walk(dir); 
        CalculateGravity();



        if (horizontal != 0)
        {
            var newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * Mathf.Sign(horizontal);
            transform.localScale = newScale;
        }

        if (IsGrounded())
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        
        var grabInput = Input.GetButtonDown("WallGrab");    

        if(onWall && IsGrounded() && _canMove && grabInput)
        {
            wallGrab = true;
            wallslide = false;
        }
        if(grabInput || !onWall || !_canMove)
        {
            wallGrab = false;
            wallslide = false;
        }
        if(IsGrounded() && !isDashing)
        {
            wallJumped = false;
            FallMultiplier();
        }
        if(wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(horizontal > 0.2f || horizontal < -0.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0f);

            float speedModifier = vertical >0 ? .05f :1;
            rb.velocity = new Vector2(rb.velocity.x , vertical * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }
        if(onWall && !IsGrounded())
        {
            if(horizontal !=0 && !wallGrab)
            {
                wallslide = true;
                WallSlide();
            }
        }
        if(Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                Jump();
            }
            if (onWall && !IsGrounded())
            {
                WallJump();
            }
        }
        if(Input.GetButtonDown("Dash") && !hasDashed)
        {
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);        }

        if (!onWall || IsGrounded())
        {
            wallslide = false;
        }
        
        if (IsGrounded() && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!IsGrounded() && groundTouch)
        {
            groundTouch = false;
        }
        if(wallGrab || wallslide || !_canMove)
        return;
    }


    private void FixedUpdate()
    {
        CheckCollisions();

        if (IsGrounded())
        {
            ApplyLinearDrag();
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
        }

        if (_canCornerCorrect) CornerCorrect(rb.velocity.y);
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    
    void GroundTouch()
    {
        hasDashed = false;
        isDashing = true;
    }

    //movement
    [Header("movement")]
    [SerializeField] private float _movementAcceleration;
    [SerializeField] private float _maxMoveSpeed;
    private bool _changeDirection => (rb.velocity.x > 0f && horizontal < 0f) || (rb.velocity.x < 0 && horizontal > 0f);
    private float horizontal;
    private float vertical;
    /*private void MoveCharacter()
    {
        if(!_canMove)
        return;

        if(!wallGrab)
        return;
        if(!wallJumped)
        {
            rb.AddForce(new Vector2(horizontal, 0f) * _movementAcceleration);
            if (Mathf.Abs(rb.velocity.x) > _maxMoveSpeed)
                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * _maxMoveSpeed, rb.velocity.y);

        }
        else
        {
            Vector2 dir = new Vector2(horizontal, vertical);
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
            
    }*/
    private void Walk(Vector2 dir)
    {
        if (!_canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    [Header("Jump")]
    //private float speed = 8f;
    [SerializeField] private float jumpingPower = 17f;
    private bool isFacingRight = true;
    [SerializeField] private float _coyoteTime = .1f;
    private float _coyoteTimeCounter;
    [SerializeField] private float _jumpBufferTime = .2f;
    private float _jumpBufferCounter;
    private int maxJumps = 2;
    private int _jumpsLeft;

    //private bool _isJumping = false;
    private void Jump()
    {
        var jumpInput = Input.GetButtonDown("Jump");
        var jumpInputRelease = Input.GetButtonUp("Jump");

        if (rb.velocity.y <= 0 && IsGrounded())
        {
            _jumpsLeft = maxJumps;
        }

        if (_coyoteTimeCounter > 0f && _jumpsLeft > 0 && _jumpBufferCounter > 0f)
        {
            trail1.emitting = true;
            trail.emitting = false;
            rb.velocity =  Vector2.up * jumpingPower;
            _jumpsLeft -= 1;
            _jumpBufferCounter = 0f;
        }

        if (jumpInputRelease && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.2f);
            trail1.emitting = false;
            trail.emitting = false;
            _coyoteTimeCounter = 0f;
        }
        //_isJumping = true;
    }


    /*[Header("Dash")]
    [SerializeField] private float _dashingVelocity = 14f;
    [SerializeField] private float _dashingTime = 0.5f;
    private Vector2 _DashingDir;
    private bool _isDashing;
    private bool _canDash = true;
    private void Dash()
    {
        var dashInput = Input.GetButtonDown("Dash");
        if (dashInput && _canDash)
        {
            _isDashing = true;
            _canDash = false;
            trail.emitting = true;
            trail1.emitting = false;

            _DashingDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (_DashingDir == Vector2.zero)
            {
                _DashingDir = new Vector2(transform.localScale.x, 0);
            }

            StartCoroutine(StopDashing());
        }

        if (_isDashing)
        {
            rb.velocity = _DashingDir.normalized * _dashingVelocity;
            return;
        }

        if (IsGrounded())
        {
            _canDash = true;
        }
    }*/

    [Header("Dash")]
    public float dashSpeed = 20;
    private bool hasDashed;
    public bool isDashing;
    public ParticleSystem dashParticle;
    private void Dash(float x, float y)
    {
        hasDashed = true;


        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        StartCoroutine(GroundDash());

        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (IsGrounded())
            hasDashed = false;
    }
    [Header("grab")]
    private bool wallGrab;
    private bool _canMove;
    public float speed = 10;

    /*void WallGrab()
    {
    }*/

    [Header("wallslide")]
    public float slideSpeed = 5;
    public bool wallslide;
    private void WallSlide()
    {
        if(!_canMove)
        return;

        bool pushingWall = false;

        if ((rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall))
        {
            pushingWall = true;
        }

        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
    }

    [Header("Wall Jump")]
    public bool wallJumped;
    public float wallJumpLerp = 10;

    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    //public int side;
    //public Vector2 rightOffset;
    //public Vector2 leftOffset;

    private void WallJump()
    {
        //wlljump
        onWall = true;
        StopCoroutine(DisableMovement(0));
        StopCoroutine(DisableMovement(0.1f));
        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += wallDir/1.5f + wallDir / 1.5f * jumpingPower;

        wallJumped = true;
    }

    void StickToWall()
    {
        //Push player torwards wall
        if (onRightWall && horizontal >= 0f)
        {
            rb.velocity = new Vector2(1f, rb.velocity.y);
        }
        else if (!onRightWall && horizontal <= 0f)
        {
            rb.velocity = new Vector2(-1f, rb.velocity.y);
        }

        //Face correct direction
        if (onRightWall && !isFacingRight)
        {
            Flip();
        }
        else if (!onRightWall && isFacingRight)
        {
            Flip();
        }
    }
    IEnumerator DisableMovement(float time)
    {
        _canMove = false;
        yield return new WaitForSeconds(time);
        _canMove = true;
    }


    [SerializeField] private float _linearDrag;
    private void ApplyLinearDrag()
    {
        if (Mathf.Abs(horizontal) < 0.4f || _changeDirection)
        {
            rb.drag = _linearDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }


    [SerializeField] private float _airLinearDrag = 2.5f;
    private void ApplyAirLinearDrag()
    {
        rb.drag = _airLinearDrag;
    }

    /*private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(_dashingTime);
        trail.emitting = false;
        _isDashing = false;
    }*/

    private void CalculateGravity()
    {
        if (IsGrounded())
        {
            if (vertical < 0) 
            {
                vertical = 0;
            }
        }
        else
        {
            if (vertical < _fallClamp) vertical = _fallClamp;
        }
    }


    [Header("Ground Collision Variables")]
    private bool _onGround;

    //public float collisionRadius;
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer);
    }

    /*private void HandleWalls()
    {
        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        side = onRightWall ? 1 : -1;

    }*/

    [Header("Corner Correction Variables")]
    [SerializeField] private float _topRaycastLength;
    [SerializeField] private Vector3 _edgeRaycastOffset;
    [SerializeField] private Vector3 _innerRaycastOffset;
    private bool _canCornerCorrect;
    void CornerCorrect(float Yvelocity)
    {
        //Push player to the right
        RaycastHit2D _hit = Physics2D.Raycast(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.left, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider != null)
        {
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x + _newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, Yvelocity);
            return;
        }

        //Push player to the left
        _hit = Physics2D.Raycast(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.right, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider != null)
        {
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x - _newPos, transform.position.y, transform.position.z);
            rb.velocity = new Vector2(rb.velocity.x, Yvelocity);
        }
    }

    [SerializeField] private float _wallRaycastLength;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    [SerializeField] private LayerMask _groundLayer;
    public int wallSide;
    private void CheckCollisions()
    {
        //Ground Collisions
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        //Corner Collisions
        _canCornerCorrect = Physics2D.Raycast(transform.position + _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) &&
                            !Physics2D.Raycast(transform.position + _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) ||
                            Physics2D.Raycast(transform.position - _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) &&
                            !Physics2D.Raycast(transform.position - _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer);

        //Wall Collisions
        onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer) ||
                    Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);
        onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer);
        onLeftWall= Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);

        wallSide = onRightWall ? -1:1;

    }


    [SerializeField] private float _fallMultiplier = 2.5f;
    [SerializeField] private float _lowJumpFallMultiplier = 2f;
    private void FallMultiplier()
    {
        /*if (rb.velocity.y < 0)
        {
            rb.gravityScale = _fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !Input.GetButtonDown("Jump"))
        {
            rb.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }*/
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (_lowJumpFallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Ground Check
        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        //Corner Check
        Gizmos.DrawLine(transform.position + _edgeRaycastOffset, transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _edgeRaycastOffset, transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset, transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _innerRaycastOffset, transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength);

        //Corner Distance Check
        Gizmos.DrawLine(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.left * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.right * _topRaycastLength);


        //Wall Check
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * _wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _wallRaycastLength);

    }
}