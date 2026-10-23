using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Use no botão Jogar do menu para abrir a cena do mapa.</summary>
public class NavegacaoDoMenu : MonoBehaviour
{
    [SerializeField] private string cenaDoMapa = "WorldSelection";

    public void Jogar()
    {
        if (Application.CanStreamedLevelBeLoaded(cenaDoMapa))
            SceneManager.LoadScene(cenaDoMapa);
        else
            Debug.LogError($"A cena '{cenaDoMapa}' não está incluída nas Build Settings.", this);
    }
}

