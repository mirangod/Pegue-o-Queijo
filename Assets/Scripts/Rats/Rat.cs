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
    [Header("Queijo nas costas")]
    [SerializeField, Min(0.01f)] private float queijoMassMultiplier = 2f;
    [SerializeField, Range(0.1f, 1f)] private float queijoJumpMultiplier = 0.65f;
    [SerializeField, Range(0.1f, 1f)] private float queijoJumpHoldMultiplier = 0.65f;
    [SerializeField, Min(0f)] private float queijoAcceleration = 250f;
    [SerializeField, Min(0f)] private float queijoTurnAcceleration = 80f;
    [SerializeField, Min(0f)] private float queijoStopAcceleration = 160f;
    [SerializeField, Min(0f)] private float interactionRange = 1.5f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public float move;

    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;
    private Collider2D ratCollider;
    private float massaOriginal = 1f;
    private bool carregandoQueijo;
    private int facingDirection = 1;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();
    private readonly HashSet<int> leftPointers = new HashSet<int>();
    private readonly HashSet<int> rightPointers = new HashSet<int>();

    protected virtual void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        ratCollider = GetComponent<Collider2D>();
        if (rig != null)
            massaOriginal = rig.mass;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool CarregandoQueijo => carregandoQueijo;
    public int FacingDirection => facingDirection;

    public bool TryCarryCheese()
    {
        if (carregandoQueijo || rig == null)
            return false;

        carregandoQueijo = true;
        rig.mass = massaOriginal * queijoMassMultiplier;
        return true;
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
        rig.velocity = new Vector2(rig.velocity.x, GetJumpForce());
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

        if (move != 0f)
        {
            facingDirection = move < 0f ? -1 : 1;
            if (spriteRenderer != null)
                spriteRenderer.flipX = facingDirection < 0;
        }

        float targetSpeed = move * speed;
        if (!carregandoQueijo)
        {
            rig.velocity = new Vector2(targetSpeed, rig.velocity.y);
        }
        else
        {
            float acceleration = targetSpeed == 0f
                ? queijoStopAcceleration
                : rig.velocity.x * targetSpeed < 0f
                    ? queijoTurnAcceleration
                    : queijoAcceleration;
            float horizontalSpeed = Mathf.MoveTowards(
                rig.velocity.x,
                targetSpeed,
                acceleration * Time.fixedDeltaTime);
            rig.velocity = new Vector2(horizontalSpeed, rig.velocity.y);
        }

        if (!jumpHeld || rig.velocity.y <= 0f)
            return;

        float holdTime = maxJumpHoldTime * (carregandoQueijo ? queijoJumpHoldMultiplier : 1f);
        if (jumpHoldTimer < holdTime)
        {
            float holdForce = jumpHoldForce * (carregandoQueijo ? queijoJumpMultiplier : 1f);
            rig.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
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
                rig.velocity = new Vector2(rig.velocity.x, GetJumpForce());
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

    public void Interact()
    {
        if (carregandoQueijo)
            return;

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);
        Interactable nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            Interactable candidate = nearbyCollider.GetComponentInParent<Interactable>();
            if (candidate == null || !candidate.CanInteract(this))
                continue;

            float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        if (nearest != null)
            nearest.Interact(this);
    }

    public void FreezeForVictory()
    {
        move = 0f;
        jumpHeld = false;
        leftPointers.Clear();
        rightPointers.Clear();

        if (rig != null)
        {
            rig.velocity = Vector2.zero;
            rig.simulated = false;
        }
    }

    public Vector3 GetBackOffset(Vector2 offset)
    {
        return new Vector3(-facingDirection * Mathf.Abs(offset.x), offset.y, 0f);
    }

    private float GetJumpForce()
    {
        return jumpForce * (carregandoQueijo ? queijoJumpMultiplier : 1f);
    }
}
