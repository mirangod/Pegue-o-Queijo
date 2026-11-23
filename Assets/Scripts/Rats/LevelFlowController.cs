using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelFlowController : MonoBehaviour
{
    private static bool hasSelectedObjective;
    private static bool selectedObjectiveRequiresCheese;
    private static string selectedLevelId;

    [SerializeField] private bool requiresCheese;
    [Header("Fase e conquistas")]
    [SerializeField] private string levelId;
    [SerializeField, Range(1, 3)] private int quantidadeDeEstrelasDisponiveis = 1;
    [SerializeField] private DesafioDeEstrela[] desafiosDeEstrela;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToSelectionButton;
    [SerializeField] private Button defeatPlayAgainButton;
    [SerializeField] private Button defeatBackToSelectionButton;
    [SerializeField] private string selectionSceneName = "World";

    private bool completed;
    private bool missingPanelReported;
    private float tempoDecorrido;
    private int moedasColetadas;
    private bool objetivoSecretoConcluido;

    private void OnValidate()
    {
        quantidadeDeEstrelasDisponiveis = Mathf.Clamp(quantidadeDeEstrelasDisponiveis, 1, 3);
        int desafios = quantidadeDeEstrelasDisponiveis - 1;
        if (desafiosDeEstrela == null) desafiosDeEstrela = new DesafioDeEstrela[desafios];
        if (desafiosDeEstrela.Length != desafios) System.Array.Resize(ref desafiosDeEstrela, desafios);
        for (int i = 0; i < desafiosDeEstrela.Length; i++)
            if (desafiosDeEstrela[i] == null) desafiosDeEstrela[i] = new DesafioDeEstrela();
    }

    private void Awake()
    {
        if (hasSelectedObjective)
            requiresCheese = selectedObjectiveRequiresCheese;
        if (!string.IsNullOrEmpty(selectedLevelId))
            levelId = selectedLevelId;
        selectedLevelId = null;
        hasSelectedObjective = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        RegisterButton(playAgainButton, PlayAgain);
        RegisterButton(backToSelectionButton, BackToSelection);
        RegisterButton(defeatPlayAgainButton, PlayAgain);
        RegisterButton(defeatBackToSelectionButton, BackToSelection);

        Rat rat = FindObjectOfType<Rat>();
        if (rat != null)
            rat.RegisterDeathListener(ShowDefeat);
    }

    private void Update()
    {
        if (!completed) tempoDecorrido += Time.deltaTime;
    }

    public static void SetObjectiveForNextLevel(bool objectiveRequiresCheese, string id = null)
    {
        hasSelectedObjective = true;
        selectedObjectiveRequiresCheese = objectiveRequiresCheese;
        selectedLevelId = id;
    }

    public void RegistrarMoedaColetada() => moedasColetadas++;
    public void RegistrarFlorColetada() => floresColetadas++;
    public void ConcluirObjetivoSecreto() => objetivoSecretoConcluido = true;
    private int floresColetadas;

    public void TryComplete(LevelGoalPoint point, Rat rat)
    {
        if (completed || rat == null)
            return;

        bool correctGoal = requiresCheese
            ? point == LevelGoalPoint.Home && rat.CarregandoQueijo
            : point == LevelGoalPoint.Finish;

        if (!correctGoal)
            return;

        if (victoryPanel == null)
        {
            if (!missingPanelReported)
            {
                Debug.LogError("Associe o painel de vitória ao LevelFlowController.", this);
                missingPanelReported = true;
            }
            return;
        }

        completed = true;
        int estrelas = 1;
        for (int i = 0; desafiosDeEstrela != null && i < desafiosDeEstrela.Length; i++)
        {
            if (desafiosDeEstrela[i] == null) continue;
            if (desafiosDeEstrela[i].Concluido(tempoDecorrido, moedasColetadas, floresColetadas, objetivoSecretoConcluido))
                estrelas++;
        }
        ProgressaoDeFases.RegistrarConclusao(levelId, estrelas);
        rat.FreezeForVictory();
        victoryPanel.SetActive(true);
    }

    private void ShowDefeat()
    {
        if (completed)
            return;

        completed = true;
        if (defeatPanel == null)
        {
            Debug.LogError("Associe o painel de derrota ao LevelFlowController.", this);
            return;
        }

        defeatPanel.SetActive(true);
    }

    private void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToSelection()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(selectionSceneName))
        {
            Debug.LogError($"A cena de seleção '{selectionSceneName}' não está nas Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(selectionSceneName);
    }
}
