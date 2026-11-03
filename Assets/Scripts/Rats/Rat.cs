using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
    protected Rigidbody2D rig;
    
    public float speed;
    public float jumpForce;

    [SerializeField, Min(0f)] private float maxJumpHoldTime = 0.25f;
    [SerializeField, Min(0f)] private float jumpHoldForce = 12f;
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

    public float move;

    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;

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
        // Mantido para compatibilidade com eventos antigos de UI.
        JumpPressed();
    }

    public void JumpPressed()
    {
        if (rig == null || !isGrounded)
            return;

        isGrounded = false;
        jumpHeld = true;
        jumpHoldTimer = 0f;
        rig.velocity = new Vector2(rig.velocity.x, jumpForce);
    }

    public void JumpReleased()
    {
        jumpHeld = false;

        // Soltar cedo reduz a velocidade ascendente e produz um pulo mais curto.
        if (rig != null && rig.velocity.y > 0f)
            rig.velocity = new Vector2(rig.velocity.x, rig.velocity.y * jumpCutMultiplier);
    }

    protected virtual void FixedUpdate()
    {
        if (!jumpHeld || rig == null || rig.velocity.y <= 0f)
            return;

        if (jumpHoldTimer < maxJumpHoldTime)
        {
            rig.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
        else
        {
            jumpHeld = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void StopMoving()
    {
        Debug.Log("Parou de andar");

        move = 0;
    }
}
