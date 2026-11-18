using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelFlowController : MonoBehaviour
{
    private static bool hasSelectedObjective;
    private static bool selectedObjectiveRequiresCheese;

    [SerializeField] private bool requiresCheese;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToSelectionButton;
    [SerializeField] private Button defeatPlayAgainButton;
    [SerializeField] private Button defeatBackToSelectionButton;
    [SerializeField] private string selectionSceneName = "World";

    private bool completed;
    private bool missingPanelReported;

    private void Awake()
    {
        if (hasSelectedObjective)
            requiresCheese = selectedObjectiveRequiresCheese;

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

    public static void SetObjectiveForNextLevel(bool objectiveRequiresCheese)
    {
        hasSelectedObjective = true;
        selectedObjectiveRequiresCheese = objectiveRequiresCheese;
    }

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
