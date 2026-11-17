using UnityEngine;

public enum LevelGoalPoint
{
    Finish,
    Home
}

public class LevelGoalTrigger : MonoBehaviour
{
    [SerializeField] private LevelGoalPoint point = LevelGoalPoint.Finish;
    [SerializeField] private LevelFlowController levelFlow;

    private void Awake()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();
        if (goalCollider == null)
        {
            Debug.LogError("Adicione um Collider2D ao ponto de objetivo.", this);
            return;
        }

        if (!goalCollider.isTrigger)
            Debug.LogWarning("O Collider2D do objetivo deve estar com Is Trigger ativado.", this);

        if (levelFlow == null)
            levelFlow = FindObjectOfType<LevelFlowController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryComplete(other.GetComponentInParent<Rat>());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryComplete(other.GetComponentInParent<Rat>());
    }

    private void TryComplete(Rat rat)
    {
        if (levelFlow == null)
            return;

        levelFlow.TryComplete(point, rat);
    }
}
