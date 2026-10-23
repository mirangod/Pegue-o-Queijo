using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Move a câmera ao arrastar o mapa, sem ultrapassar os limites definidos.</summary>
public class MapaArrastavel : MonoBehaviour
{
    [SerializeField] private Camera cameraDoMapa;
    [SerializeField] private Vector2 limiteMinimo = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 limiteMaximo = new Vector2(10f, 10f);

    private Vector2 ultimaPosicaoDoPonteiro;
    private bool arrastando;

    private void Awake()
    {
        if (cameraDoMapa == null)
            cameraDoMapa = GetComponent<Camera>();
    }

    private void Update()
    {
        if (cameraDoMapa == null)
            return;

        // Touch no celular. A posição do dedo em UI continua livre para clicar nos botões.
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);
            if (toque.phase == TouchPhase.Began)
            {
                arrastando = !PointerSobreUI(toque.fingerId);
                ultimaPosicaoDoPonteiro = toque.position;
            }
            else if (toque.phase == TouchPhase.Moved && arrastando)
            {
                MoverCamera(toque.position);
            }
            else if (toque.phase == TouchPhase.Ended || toque.phase == TouchPhase.Canceled)
            {
                arrastando = false;
            }

            return;
        }

        // Mouse no Editor para facilitar a configuração e o teste do mapa.
        if (Input.GetMouseButtonDown(0))
        {
            arrastando = !PointerSobreUI();
            ultimaPosicaoDoPonteiro = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && arrastando)
        {
            MoverCamera(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            arrastando = false;
        }
    }

    private bool PointerSobreUI(int pointerId = -1)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private void MoverCamera(Vector2 posicaoAtual)
    {
        float unidadesPorPixel = cameraDoMapa.orthographicSize * 2f / Screen.height;
        Vector2 deslocamentoTela = posicaoAtual - ultimaPosicaoDoPonteiro;
        Vector3 posicao = cameraDoMapa.transform.position;
        posicao.x -= deslocamentoTela.x * unidadesPorPixel;
        posicao.y -= deslocamentoTela.y * unidadesPorPixel;
        posicao.x = Mathf.Clamp(posicao.x, limiteMinimo.x, limiteMaximo.x);
        posicao.y = Mathf.Clamp(posicao.y, limiteMinimo.y, limiteMaximo.y);
        cameraDoMapa.transform.position = posicao;
        ultimaPosicaoDoPonteiro = posicaoAtual;
    }

    public void DefinirLimites(Vector2 minimo, Vector2 maximo)
    {
        limiteMinimo = Vector2.Min(minimo, maximo);
        limiteMaximo = Vector2.Max(minimo, maximo);
        if (cameraDoMapa != null)
        {
            Vector3 posicao = cameraDoMapa.transform.position;
            posicao.x = Mathf.Clamp(posicao.x, limiteMinimo.x, limiteMaximo.x);
            posicao.y = Mathf.Clamp(posicao.y, limiteMinimo.y, limiteMaximo.y);
            cameraDoMapa.transform.position = posicao;
        }
    }
}
