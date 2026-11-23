using UnityEngine;

/// <summary>Coloque no gatilho/objeto da missão secreta e configure o alvo no Inspetor.</summary>
public class ObjetivoSecretoDaFase : MonoBehaviour
{
    [SerializeField] private LevelFlowController controlador;
    [SerializeField] private bool concluirAoEntrarNoGatilho = true;
    private bool concluido;

    private void Awake()
    {
        if (controlador == null) controlador = FindObjectOfType<LevelFlowController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (concluirAoEntrarNoGatilho && other.GetComponentInParent<Rat>() != null)
            Concluir();
    }

    public void Concluir()
    {
        if (concluido || controlador == null) return;
        concluido = true;
        controlador.ConcluirObjetivoSecreto();
    }
}
