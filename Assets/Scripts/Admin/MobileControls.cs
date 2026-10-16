using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MobileControls : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onPress;
    public UnityEvent onRelease;

    private RatAdmin ratAdmin;
    private Rat rat;
    private string ratName;

    private void Awake()
    {
        rat = GameObject.FindGameObjectWithTag("Player").GetComponent<Rat>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (gameObject.name)
        {
            case "Left Button":
                rat.MoveLeft();
                break;
            case "Right Button":
                rat.MoveRight();
                break;
            case "Jump Button":
                rat.StartJumpHold();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (gameObject.name)
        {
            case "Left Button":
            case "Right Button":
                rat.StopMoving();
                break;
            case "Jump Button":
                rat.StopJumpHold();
                break;
        }
    }
}
