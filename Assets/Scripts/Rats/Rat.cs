using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Rat : MonoBehaviour
{
    protected Rigidbody2D rig;
    
    public float speed;
    public float jumpForce;

    [SerializeField, Min(0f)] private float maxJumpHoldTime = 0.25f;
    [SerializeField, Min(0f)] private float jumpHoldForce = 12f;
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private string autoBounceTag = "Bounceable";
    [Header("Objeto carregado")]
    [SerializeField, Min(0.01f)] private float itemMassMultiplier = 2f;
    [SerializeField, Range(0.1f, 1f)] private float itemJumpMultiplier = 0.65f;
    [SerializeField, Range(0.1f, 1f)] private float itemJumpHoldMultiplier = 0.65f;
    [SerializeField, Min(0f)] private float itemAcceleration = 250f;
    [SerializeField, Min(0f)] private float itemTurnAcceleration = 80f;
    [SerializeField, Min(0f)] private float itemStopAcceleration = 160f;
    [SerializeField, Min(0f)] private float interactionRange = 1.5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Vida")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField, Min(0f)] private float damageCooldown = 1f;
    [SerializeField] private UnityEvent onHealthChanged = new UnityEvent();
    [SerializeField] private UnityEvent onDeath = new UnityEvent();

    public float move;

    private bool isGrounded;
    private bool jumpHeld;
    private float jumpHoldTimer;
    private Collider2D ratCollider;
    private float massaOriginal = 1f;
    private CarryableInteractable objetoCarregado;
    private int facingDirection = 1;
    private int currentHealth;
    private float nextDamageTime;
    private bool dead;
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
        currentHealth = maxHealth;
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => dead;
    public bool CarregandoQueijo => objetoCarregado is CheeseInteractable;
    public bool CarregandoAlgo => objetoCarregado != null;
    public bool PodeCarregarObjeto => objetoCarregado == null && rig != null;
    public int FacingDirection => facingDirection;
    public Vector2 CurrentVelocity => rig != null ? rig.velocity : Vector2.zero;

    public bool TryCarryObject(CarryableInteractable objeto)
    {
        if (dead || !PodeCarregarObjeto || objeto == null || !objeto.AttachToRat(this))
            return false;

        objetoCarregado = objeto;
        rig.mass = massaOriginal * itemMassMultiplier;
        return true;
    }

    public void DropCarriedObject()
    {
        if (objetoCarregado == null)
            return;

        CarryableInteractable objeto = objetoCarregado;
        objetoCarregado = null;
        if (rig != null)
            rig.mass = massaOriginal;
        objeto.DropFromRat(this);
    }

    public void MoveLeft()
    {
        if (dead)
            return;

        Debug.Log("Moving left");

        move = -1;
    }

    public void MoveRight()
    {
        if (dead)
            return;

        Debug.Log("Moving right");

        move = 1;
    }

    /// <summary>Atualiza a entrada de um botão direcional, mantendo outros dedos pressionados.</summary>
    public void SetMoveInput(int direction, int pointerId, bool pressed)
    {
        if (dead)
            return;

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
        if (dead || rig == null || !isGrounded)
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
        if (!CarregandoAlgo)
        {
            rig.velocity = new Vector2(targetSpeed, rig.velocity.y);
        }
        else
        {
            float acceleration = targetSpeed == 0f
                ? itemStopAcceleration
                : rig.velocity.x * targetSpeed < 0f
                    ? itemTurnAcceleration
                    : itemAcceleration;
            float horizontalSpeed = Mathf.MoveTowards(
                rig.velocity.x,
                targetSpeed,
                acceleration * Time.fixedDeltaTime);
            rig.velocity = new Vector2(horizontalSpeed, rig.velocity.y);
        }

        if (!jumpHeld || rig.velocity.y <= 0f)
            return;

        float holdTime = maxJumpHoldTime * (CarregandoAlgo ? itemJumpHoldMultiplier : 1f);
        if (jumpHoldTimer < holdTime)
        {
            float holdForce = jumpHoldForce * (CarregandoAlgo ? itemJumpMultiplier : 1f);
            rig.AddForce(Vector2.up * holdForce, ForceMode2D.Force);
            jumpHoldTimer += Time.fixedDeltaTime;
        }
        else
        {
            jumpHeld = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (dead)
            return;

        HandleDamage(collision.collider);

        if (dead || rig == null || ratCollider == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            bool landedOnTop = contact.normal.y > 0.5f && rig.velocity.y <= 0f;

            if (landedOnTop && HasBounceableTag(collision.collider))
            {
                Enemy enemy = collision.collider.GetComponentInParent<Enemy>();
                if (enemy != null)
                    enemy.TakeDamage(1);

                BreakableObject breakable = collision.collider.GetComponentInParent<BreakableObject>();
                if (breakable != null)
                    breakable.Break();

                // Todo objeto com a tag Bounceable lança o rato, mesmo sem efeito especial.
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleDamage(other);
    }

    public void TakeDamage(int damage)
    {
        if (dead || damage <= 0 || Time.time < nextDamageTime)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        nextDamageTime = Time.time + damageCooldown;
        onHealthChanged?.Invoke();

        if (currentHealth == 0)
            Die();
    }

    public void RegisterDeathListener(UnityAction listener)
    {
        if (listener != null)
            onDeath.AddListener(listener);
    }

    private void HandleDamage(Collider2D other)
    {
        if (dead || other == null)
            return;

        DamageDealer damageDealer = other.GetComponentInParent<DamageDealer>();
        if (damageDealer != null)
            TakeDamage(damageDealer.Damage);
    }

    private void Die()
    {
        dead = true;
        move = 0f;
        jumpHeld = false;
        leftPointers.Clear();
        rightPointers.Clear();

        if (rig != null)
        {
            rig.velocity = Vector2.zero;
            rig.simulated = false;
        }

        onDeath?.Invoke();
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
        if (dead)
            return;

        if (CarregandoAlgo)
        {
            DropCarriedObject();
            return;
        }

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
        return jumpForce * (CarregandoAlgo ? itemJumpMultiplier : 1f);
    }

    private bool HasBounceableTag(Collider2D collider)
    {
        for (Transform current = collider.transform; current != null; current = current.parent)
        {
            if (current.CompareTag(autoBounceTag))
                return true;
        }

        return false;
    }
}
