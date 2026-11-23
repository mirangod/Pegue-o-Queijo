using TMPro;
using UnityEngine;

/// <summary>HUD do mapa. Se os textos não forem associados, monta uma HUD compacta no Canvas.</summary>
public class AdministradorDeRecursos : MonoBehaviour
{
    [SerializeField] private TMP_Text textoEstrelas;
    [SerializeField] private TMP_Text textoMoedas;
    [SerializeField] private TMP_Text textoFlores;
    [SerializeField] private Vector2 posicao = new Vector2(20f, -20f);
    [SerializeField] private Vector2 tamanho = new Vector2(190f, 90f);

    private void Awake()
    {
        if (textoEstrelas == null || textoMoedas == null || textoFlores == null)
            CriarHud();
        AtualizarHud();
    }

    private void Update() => AtualizarHud();

    private void AtualizarHud()
    {
        if (textoEstrelas != null) textoEstrelas.text = "★ " + ProgressaoDeFases.TotalEstrelas;
        if (textoMoedas != null) textoMoedas.text = "Moedas: " + ProgressaoDeFases.TotalMoedas;
        if (textoFlores != null) textoFlores.text = "Flores: " + ProgressaoDeFases.TotalFlores;
    }

    private void CriarHud()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        RectTransform painel = CriarRetangulo("HUD de recursos", canvas.transform, posicao, tamanho);
        UnityEngine.UI.Image fundo = painel.gameObject.AddComponent<UnityEngine.UI.Image>();
        fundo.color = new Color(0f, 0f, 0f, 0.68f);
        fundo.raycastTarget = false;
        textoEstrelas = CriarTexto("Estrelas", painel, new Vector2(8, -5), new Vector2(170, 25));
        textoMoedas = CriarTexto("Moedas", painel, new Vector2(8, -32), new Vector2(170, 25));
        textoFlores = CriarTexto("Flores", painel, new Vector2(8, -59), new Vector2(170, 25));
    }

    private static TMP_Text CriarTexto(string nome, Transform pai, Vector2 pos, Vector2 size)
    {
        RectTransform rect = CriarRetangulo(nome, pai, pos, size);
        TMP_Text texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
        texto.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        texto.fontSize = 20;
        texto.color = Color.white;
        texto.alignment = TextAlignmentOptions.Left;
        return texto;
    }

    private static RectTransform CriarRetangulo(string nome, Transform pai, Vector2 pos, Vector2 size)
    {
        GameObject objeto = new GameObject(nome, typeof(RectTransform));
        objeto.transform.SetParent(pai, false);
        RectTransform rect = (RectTransform)objeto.transform;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return rect;
    }
}
