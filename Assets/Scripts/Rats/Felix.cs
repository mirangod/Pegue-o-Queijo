using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Felix : Rat
{
    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(feet.position, 0.1f, groundLayer);

        rig.velocity = new Vector2(move * speed, rig.velocity.y);
    }
}
