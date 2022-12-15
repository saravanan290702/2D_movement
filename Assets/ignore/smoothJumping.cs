using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class smoothJumping : MonoBehaviour
{

    [SerializeField] public float _coyoteTime = .1f;
    public float _coyoteTimeCounter;
    [SerializeField] public float _jumpBufferTime = .2f;
    public float _jumpBufferCounter;
    private Rigidbody2D rb;

    private Collision coll;
    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f)
        {
            _jumpBufferCounter = 0f;
            Debug.Log("called");
        }
        else if (rb.velocity.y > 0)
        {
            _coyoteTimeCounter = 0f;
            Debug.Log("off");
        }
    }
    private void FixedUpdate()
    {
        if (coll.onGround)
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

    }
}
