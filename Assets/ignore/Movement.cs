using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collision coll;

    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;

    //Inputs
    float x;
    float y;
    float xRaw;
    float yRaw;
    Vector2 dir;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {

        Inputs();
        Walk(dir);

        if(coll.onGround && !coll.onGround)
        {
            WallSlide();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(coll.onGround)
            {
                Jump();
            }
        }

    }

    private void Inputs()
    {
         x = Input.GetAxis("Horizontal");
         y = Input.GetAxis("Vertical");
         xRaw = Input.GetAxisRaw("Horizontal");
         yRaw = Input.GetAxisRaw("Vertical");
         dir = new Vector2(x, y);

    }

    private void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
    }
    
    private void Walk(Vector2 dir)
    {
        rb.velocity = (new Vector2(dir.x * speed, rb.velocity.y));
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += Vector2.up * jumpForce;
    }

}
