using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ações dos botões e painéis do menu principal.
/// Conecte os métodos aos botões pelo evento On Click do componente Button.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Telas do menu")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Cena aberta pelo botão Jogar")]
    [SerializeField] private string worldSelectionSceneName = "WorldSelection";

    public void Play()
    {
        if (string.IsNullOrWhiteSpace(worldSelectionSceneName))
        {
            Debug.LogError("Defina o nome da cena de seleção de mundos no MainMenuController.", this);
            return;
        }

        SceneManager.LoadScene(worldSelectionSceneName);
    }

    public void OpenSettings()
    {
        SetPanelActive(settingsPanel, true, "Configurações");
        SetPanelActive(creditsPanel, false, "Créditos");
    }

    public void OpenCredits()
    {
        SetPanelActive(creditsPanel, true, "Créditos");
        SetPanelActive(settingsPanel, false, "Configurações");
    }

    public void ClosePanels()
    {
        SetPanelActive(settingsPanel, false, "Configurações");
        SetPanelActive(creditsPanel, false, "Créditos");
    }

    private void SetPanelActive(GameObject panel, bool isActive, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"O painel de {panelName} não foi associado no MainMenuController.", this);
            return;
        }

        panel.SetActive(isActive);
    }
}
