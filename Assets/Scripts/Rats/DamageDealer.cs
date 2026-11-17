using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 1;

    public int Damage => damage;
}
