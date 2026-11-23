using UnityEngine;

public enum RecursoColetavel { Moeda, Flor }

/// <summary>Adicione ao coletável; configure um Collider2D trigger e o tipo/quantidade no Inspetor.</summary>
public class ColetavelDeRecurso : MonoBehaviour
{
    [SerializeField] private RecursoColetavel recurso;
    [SerializeField, Min(1)] private int quantidade = 1;
    private bool coletado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado || other.GetComponentInParent<Rat>() == null) return;
        coletado = true;
        ProgressaoDeFases.RegistrarRecurso(recurso, quantidade);
        LevelFlowController controlador = FindObjectOfType<LevelFlowController>();
        if (controlador != null)
        {
            if (recurso == RecursoColetavel.Moeda) controlador.RegistrarMoedaColetada();
            else controlador.RegistrarFlorColetada();
        }
        Destroy(gameObject);
    }
}
