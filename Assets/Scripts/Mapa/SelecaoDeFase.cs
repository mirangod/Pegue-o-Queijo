using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>Abre o painel de configuração correspondente a uma fase desbloqueada.</summary>
public class SelecaoDeFase : MonoBehaviour
{
    [SerializeField] private string nomeDaFase = "Fase 1";
    [SerializeField] private string nomeDaCena;
    [SerializeField] private bool desbloqueada = true;
    [SerializeField] private GameObject painelDeFase;
    [SerializeField] private TMP_Text textoDaFase;
    [SerializeField] private Button botaoJogar;

    private static string cenaSelecionada;

    private void Awake()
    {
        if (painelDeFase != null)
            painelDeFase.SetActive(false);
        if (botaoJogar != null)
            botaoJogar.onClick.AddListener(IniciarFaseSelecionada);
    }

    public void AoSelecionarFase()
    {
        if (!desbloqueada)
            return;

        cenaSelecionada = nomeDaCena;
        if (textoDaFase != null)
            textoDaFase.text = nomeDaFase;
        if (painelDeFase != null)
            painelDeFase.SetActive(true);
    }

    public void FecharPainel()
    {
        if (painelDeFase != null)
            painelDeFase.SetActive(false);
    }

    public void IniciarFaseSelecionada()
    {
        if (string.IsNullOrEmpty(cenaSelecionada))
        {
            Debug.LogWarning($"Configure a cena da fase '{nomeDaFase}' para iniciar pelo painel.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(cenaSelecionada))
        {
            Debug.LogError($"A cena '{cenaSelecionada}' não está incluída nas Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(cenaSelecionada);
    }

    public void DefinirDesbloqueada(bool valor)
    {
        desbloqueada = valor;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Fase '{nomeDaFase}' selecionada.", this);
        AoSelecionarFase();
    }
}
