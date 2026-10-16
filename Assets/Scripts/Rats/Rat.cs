using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
    protected Rigidbody2D rig;
    
    public float speed;
    public float jumpForce;
    [Min(0f)] public float jumpHoldDuration = 0.25f;
    [Min(0f)] public float jumpHoldForce = 20f;

    public float move;
    private float _move;

    public Transform feet;
    public bool isGrounded;
    public LayerMask groundLayer;

    private bool jumpHeld;
    private float jumpHoldEndTime;

    protected virtual void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    public void MoveLeft()
    {
        Debug.Log("Moving left");

        move = -1;
    }

    public void MoveRight()
    {
        Debug.Log("Moving right");

        move = 1;
    }

    public void Jump()
    {
        if (isGrounded)
            rig.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void StartJumpHold()
    {
        if (jumpHeld)
            return;

        if (!isGrounded)
            return;

        Jump();
        jumpHeld = true;
        jumpHoldEndTime = Time.time + jumpHoldDuration;
    }

    public void StopJumpHold()
    {
        jumpHeld = false;
    }

    protected virtual void FixedUpdate()
    {
        if (!jumpHeld)
            return;

        if (Time.time >= jumpHoldEndTime || rig.velocity.y <= 0f)
        {
            jumpHeld = false;
            return;
        }

        rig.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
    }

    public void StopMoving()
    {
        Debug.Log("Parou de andar");

        move = 0;
    }
}
