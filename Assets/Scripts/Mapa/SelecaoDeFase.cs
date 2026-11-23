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
    [Header("Progressão")]
    [SerializeField] private string idDaFase;
    [SerializeField, Range(1, 3)] private int quantidadeDeCriterios = 1;
    [SerializeField] private bool exigirTodosOsCriterios = true;
    [SerializeField] private CriterioDeDesbloqueio[] criteriosDeDesbloqueio = { new CriterioDeDesbloqueio() };
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
        if (!EstaDesbloqueada())
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

        LevelFlowController.SetObjectiveForNextLevel(faseSelecionadaDeQueijo, idDaFase);
        SceneManager.LoadScene(cenaSelecionada);
    }

    public bool EstaDesbloqueada()
    {
        int quantidade = Mathf.Clamp(quantidadeDeCriterios, 1, 3);
        if (criteriosDeDesbloqueio == null || criteriosDeDesbloqueio.Length < quantidade) return false;
        var ativos = new System.Collections.Generic.List<CriterioDeDesbloqueio>(quantidade);
        for (int i = 0; i < quantidade; i++) ativos.Add(criteriosDeDesbloqueio[i]);
        return desbloqueada && ProgressaoDeFases.AvaliarTodos(ativos, exigirTodosOsCriterios);
    }

    private void OnValidate()
    {
        quantidadeDeCriterios = Mathf.Clamp(quantidadeDeCriterios, 1, 3);
        if (criteriosDeDesbloqueio == null) criteriosDeDesbloqueio = new CriterioDeDesbloqueio[quantidadeDeCriterios];
        if (criteriosDeDesbloqueio.Length != quantidadeDeCriterios)
            System.Array.Resize(ref criteriosDeDesbloqueio, quantidadeDeCriterios);
        for (int i = 0; i < criteriosDeDesbloqueio.Length; i++)
            if (criteriosDeDesbloqueio[i] == null) criteriosDeDesbloqueio[i] = new CriterioDeDesbloqueio();
    }

    public int EstrelasConquistadas() => ProgressaoDeFases.Estrelas(idDaFase);

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
