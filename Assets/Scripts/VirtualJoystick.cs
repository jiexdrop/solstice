using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Image knobImage;
    private Image joystickImage;
    private Vector3 inputVector;

    void Start()
    {
        joystickImage = GetComponent<Image>();
        knobImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickImage.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / joystickImage.rectTransform.sizeDelta.x);
            pos.y = (pos.y / joystickImage.rectTransform.sizeDelta.y);

            inputVector = new Vector3(pos.x * 2 + 1, 0, pos.y * 2 - 1);
            inputVector = (inputVector.magnitude > 1) ? inputVector.normalized : inputVector;

            // Move knob
            knobImage.rectTransform.anchoredPosition = new Vector3(inputVector.x * (joystickImage.rectTransform.sizeDelta.x/3),
                inputVector.z * (joystickImage.rectTransform.sizeDelta.x/3));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector3.zero;
        knobImage.rectTransform.anchoredPosition = Vector3.zero;
    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.z;
    }

}
