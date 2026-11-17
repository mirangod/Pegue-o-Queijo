using UnityEngine;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Rat rat;
    private int direcao;

    private void Awake()
    {
        direcao = gameObject.name == "Left Button" ? -1
            : gameObject.name == "Right Button" ? 1
            : 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            rat = player.GetComponent<Rat>();

        if (rat == null)
            Debug.LogError("MobileControls precisa de um objeto Player com um componente Rat.", this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rat == null)
            return;

        switch (gameObject.name)
        {
            case "Left Button":
                rat.SetMoveInput(-1, eventData.pointerId, true);
                break;
            case "Right Button":
                rat.SetMoveInput(1, eventData.pointerId, true);
                break;
            case "Jump Button":
                rat.JumpPressed();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rat == null)
            return;

        switch (gameObject.name)
        {
            case "Left Button":
                rat.SetMoveInput(-1, eventData.pointerId, false);
                break;
            case "Right Button":
                rat.SetMoveInput(1, eventData.pointerId, false);
                break;
            case "Jump Button":
                rat.JumpReleased();
                break;
        }
    }

    private void OnDisable()
    {
        if (rat == null)
            return;

        if (direcao != 0)
            rat.ClearMoveInput(direcao);
        else if (gameObject.name == "Jump Button")
            rat.JumpReleased();
    }
}
