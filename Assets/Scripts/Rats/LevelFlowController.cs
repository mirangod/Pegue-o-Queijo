using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    private int floresColetadas;
    private bool paused;
    private GameObject pauseButtonObject;
    private GameObject pauseOverlay;
    private GameObject pauseMenuPanel;
    private GameObject settingsPanel;
    private TMP_Text volumeText;

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
        Time.timeScale = 1f;
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
        CreatePauseInterface();

        Rat rat = FindObjectOfType<Rat>();
        if (rat != null)
            rat.RegisterDeathListener(ShowDefeat);
    }

    private void Update()
    {
        if (!completed) tempoDecorrido += Time.deltaTime;
    }

    private void OnDisable()
    {
        if (paused)
        {
            paused = false;
            Time.timeScale = 1f;
        }
    }

    private void CreatePauseInterface()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Não foi encontrado um Canvas para criar o menu de pausa.", this);
            return;
        }

        Transform root = canvas.transform;
        pauseButtonObject = CreateButton("Pausar", root, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(100f, 52f), TogglePause);
        pauseOverlay = new GameObject("Pause Overlay", typeof(RectTransform), typeof(Image));
        pauseOverlay.transform.SetParent(root, false);
        RectTransform overlayRect = (RectTransform)pauseOverlay.transform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        Image shade = pauseOverlay.GetComponent<Image>();
        shade.color = new Color(0f, 0f, 0f, 0.72f);
        pauseMenuPanel = CreatePanel("Pause Menu", pauseOverlay.transform, Vector2.zero, new Vector2(360f, 330f));
        CreateLabel("Jogo pausado", pauseMenuPanel.transform, new Vector2(0f, 118f), new Vector2(320f, 48f), 30);
        CreateButton("Continuar", pauseMenuPanel.transform, new Vector2(0f, 50f), new Vector2(270f, 52f), ResumeGame);
        CreateButton("Configurações", pauseMenuPanel.transform, new Vector2(0f, -15f), new Vector2(270f, 52f), OpenSettings);
        CreateButton("Mapa", pauseMenuPanel.transform, new Vector2(0f, -80f), new Vector2(270f, 52f), BackToSelection);

        settingsPanel = CreatePanel("Settings Menu", pauseOverlay.transform, Vector2.zero, new Vector2(360f, 270f));
        CreateLabel("Configurações", settingsPanel.transform, new Vector2(0f, 90f), new Vector2(320f, 48f), 28);
        volumeText = CreateLabel(string.Empty, settingsPanel.transform, new Vector2(0f, 25f), new Vector2(280f, 45f), 22);
        CreateButton("−", settingsPanel.transform, new Vector2(-100f, -35f), new Vector2(60f, 50f), DecreaseVolume);
        CreateButton("+", settingsPanel.transform, new Vector2(100f, -35f), new Vector2(60f, 50f), IncreaseVolume);
        CreateButton("Voltar", settingsPanel.transform, new Vector2(0f, -95f), new Vector2(220f, 48f), ReturnToPauseMenu);
        pauseOverlay.SetActive(false);
        UpdateVolumeLabel();
    }

    public void TogglePause()
    {
        if (completed || pauseOverlay == null) return;
        paused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        pauseOverlay.SetActive(true);
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
    }

    private void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void ReturnToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    private void DecreaseVolume()
    {
        AudioListener.volume = Mathf.Clamp01(AudioListener.volume - 0.1f);
        UpdateVolumeLabel();
    }

    private void IncreaseVolume()
    {
        AudioListener.volume = Mathf.Clamp01(AudioListener.volume + 0.1f);
        UpdateVolumeLabel();
    }

    private void UpdateVolumeLabel()
    {
        if (volumeText != null) volumeText.text = $"Volume geral: {Mathf.RoundToInt(AudioListener.volume * 100f)}%";
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)panel.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
        return panel;
    }

    private static GameObject CreateButton(string label, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.24f, 0.38f, 0.52f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateLabel(label, buttonObject.transform, Vector2.zero, size, 21);
        return buttonObject;
    }

    private static GameObject CreateButton(string label, Transform parent, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        return CreateButton(label, parent, new Vector2(0.5f, 0.5f), position, size, action);
    }

    private static TMP_Text CreateLabel(string label, Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject(label + " Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = label;
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
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
        if (pauseButtonObject != null) pauseButtonObject.SetActive(false);
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
        if (pauseButtonObject != null) pauseButtonObject.SetActive(false);
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
        if (!Application.CanStreamedLevelBeLoaded(selectionSceneName))
        {
            Debug.LogError($"A cena de seleção '{selectionSceneName}' não está nas Build Settings.", this);
            return;
        }

        ResumeGame();
        SceneManager.LoadScene(selectionSceneName);
    }
}
