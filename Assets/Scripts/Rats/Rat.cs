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
    [SerializeField] private string autoBounceTag = "Bounceable";

    public float move;

    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;
    private Collider2D ratCollider;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();
    private readonly HashSet<int> leftPointers = new HashSet<int>();
    private readonly HashSet<int> rightPointers = new HashSet<int>();

    protected virtual void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        ratCollider = GetComponent<Collider2D>();
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

    /// <summary>Atualiza a entrada de um botão direcional, mantendo outros dedos pressionados.</summary>
    public void SetMoveInput(int direction, int pointerId, bool pressed)
    {
        HashSet<int> pointers = direction < 0 ? leftPointers : rightPointers;
        if (pressed)
            pointers.Add(pointerId);
        else
            pointers.Remove(pointerId);

        UpdateMoveFromPointers();
    }

    public void ClearMoveInput(int direction)
    {
        (direction < 0 ? leftPointers : rightPointers).Clear();
        UpdateMoveFromPointers();
    }

    private void UpdateMoveFromPointers()
    {
        move = leftPointers.Count > 0 && rightPointers.Count > 0
            ? 0f
            : leftPointers.Count > 0 ? -1f
            : rightPointers.Count > 0 ? 1f
            : 0f;
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
        if (rig == null)
            return;

        rig.velocity = new Vector2(move * speed, rig.velocity.y);

        if (!jumpHeld || rig.velocity.y <= 0f)
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
        if (rig == null || ratCollider == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            bool landedOnTop = contact.normal.y > 0.5f && rig.velocity.y <= 0f;

            if (landedOnTop && collision.collider.tag == autoBounceTag)
            {
                // O objeto não precisa de script: a tag no collider dispara o pulo do rato.
                rig.velocity = new Vector2(rig.velocity.x, jumpForce);
                jumpHeld = false;
                jumpHoldTimer = 0f;
                groundColliders.Remove(collision.collider);
                isGrounded = false;
                return;
            }

            if (landedOnTop)
            {
                groundColliders.Add(collision.collider);
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
        isGrounded = groundColliders.Count > 0;
    }

    public void StopMoving()
    {
        Debug.Log("Parou de andar");

        move = 0;
        leftPointers.Clear();
        rightPointers.Clear();
    }
}
