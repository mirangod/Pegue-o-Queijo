using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
    protected Rigidbody2D rig;
    
    public float speed;
    public float jumpForce;

    public float move;
    private float _move;

    public Transform feet;
    public bool isGrounded;
    public LayerMask groundLayer;

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
        Debug.Log("Jumping");
        // fazer função para pular e parar dee se mover enquanto estiver no ar
        rig.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void StopMoving()
    {
        Debug.Log("Parou de andar");

        move = 0;
    }
}
