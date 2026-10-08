using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Felix : Rat
{
    private void Update()
    {
        //move = Input.GetAxisRaw("Horizontal");

        rig.velocity = new Vector2(move * speed, rig.velocity.y);
    }
}
