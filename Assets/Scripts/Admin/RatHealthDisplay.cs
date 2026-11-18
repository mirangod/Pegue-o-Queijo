using UnityEngine;
using UnityEngine.UI;

public class RatHealthDisplay : MonoBehaviour
{
    [SerializeField] private Rat rat;
    [SerializeField] private Text label;

    private int lastHealth = -1;
    private int lastMaxHealth = -1;

    private void Awake()
    {
        if (rat == null)
            rat = FindObjectOfType<Rat>();
        if (label == null)
            label = GetComponent<Text>();
    }

    private void Update()
    {
        if (rat == null || label == null)
            return;

        if (lastHealth == rat.CurrentHealth && lastMaxHealth == rat.MaxHealth)
            return;

        lastHealth = rat.CurrentHealth;
        lastMaxHealth = rat.MaxHealth;
        label.text = $"Vidas: {lastHealth}/{lastMaxHealth}";
    }
}
