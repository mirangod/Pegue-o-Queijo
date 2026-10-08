using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatAdmin : MonoBehaviour
{
    private Rat rat;
    private Felix felix;
    private Luna luna;
    private Bandit bandit;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        switch (player.name)
        {
            case "Felix":
                felix = player.GetComponent<Felix>();
                break;
            case "Luna":
                luna = player.GetComponent<Luna>();
                break;
            case "Bandit":
                bandit = player.GetComponent<Bandit>();
                break;
            default:
                Debug.Log("Player não encontrado");
                break;
        }
    }

    public void Clicked(string action)
    {
        
    }
}
