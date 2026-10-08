using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
    protected Rigidbody2D rig;
    
    public float speed;
    public float jumpForce;

    public float move;

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
    }

    public void StopMoving()
    {
        Debug.Log("Parou de andar");

        move = 0;
    }
}
