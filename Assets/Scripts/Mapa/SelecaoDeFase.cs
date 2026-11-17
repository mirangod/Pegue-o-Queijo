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
    [SerializeField] private bool faseDeQueijo;
    [SerializeField] private GameObject painelDeFase;
    [SerializeField] private TMP_Text textoDaFase;
    [SerializeField] private Button botaoJogar;
    [SerializeField] private Button botaoFechar;

    private static string cenaSelecionada;
    private static bool faseSelecionadaDeQueijo;
    private static Button botaoJogarRegistrado;
    private static Button botaoFecharRegistrado;

    private void Awake()
    {
        if (painelDeFase != null)
            painelDeFase.SetActive(false);
        if (botaoJogar != null && botaoJogarRegistrado != botaoJogar)
        {
            botaoJogar.onClick.AddListener(IniciarFaseSelecionada);
            botaoJogarRegistrado = botaoJogar;
        }
        if (botaoFechar != null && botaoFecharRegistrado != botaoFechar)
        {
            botaoFechar.onClick.AddListener(FecharPainel);
            botaoFecharRegistrado = botaoFechar;
        }
    }

    public void AoSelecionarFase()
    {
        if (!desbloqueada)
            return;

        cenaSelecionada = nomeDaCena;
        faseSelecionadaDeQueijo = faseDeQueijo;
        if (textoDaFase != null)
            textoDaFase.text = nomeDaFase;
        if (painelDeFase != null)
        {
            painelDeFase.SetActive(true);
            if (GameObject.FindGameObjectWithTag("Arrastar") != null)
            {
                GameObject.FindGameObjectWithTag("Arrastar").GetComponent<MapaArrastavel>().canMove = false;
            }
        }
        else
        {
            Debug.LogWarning($"Configure o painel de fase para a fase '{nomeDaFase}'.", this);
        }
    }

    public void FecharPainel()
    {
        if (painelDeFase != null)
            painelDeFase.SetActive(false);

        if (GameObject.FindGameObjectWithTag("Arrastar") != null)
        {
            GameObject.FindGameObjectWithTag("Arrastar").GetComponent<MapaArrastavel>().canMove = true;
        }
        else
        {
            Debug.LogWarning($"Configure o painel de fase para a fase '{nomeDaFase}'.", this);
        }
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

        LevelFlowController.SetObjectiveForNextLevel(faseSelecionadaDeQueijo);
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
