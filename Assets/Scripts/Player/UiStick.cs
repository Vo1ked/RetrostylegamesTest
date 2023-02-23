using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiStick : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IDragHandler, IPauseHandler
{
    public Action<Vector2> OnPositionChanged = (Vector2) => { };

    [SerializeField] private float maxRange = 50;
    [SerializeField] private RectTransform stick;

    private bool isPause;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPause)
            return;

        Vector2 localPosition;
        ConvertTolocalPosition(eventData.position, out localPosition);
        SetStickPosition(localPosition);

        OnPositionChanged.Invoke(localPosition.normalized);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPosition();
        OnPositionChanged.Invoke(Vector2.zero);

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPause)
            return;
        Vector2 localPosition;
        ConvertTolocalPosition(eventData.position, out localPosition);
        SetStickPosition(localPosition);

        OnPositionChanged.Invoke(localPosition.normalized);
    }

    public void OnPause(bool IsPause)
    {
        OnPointerUp(null);
    }

    private void ConvertTolocalPosition(Vector2 screenPosition, out Vector2 localPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, screenPosition, Camera.current, out localPosition);
    }

    private void SetStickPosition(Vector2 position)
    {
        var distance = Vector2.Distance(Vector2.zero, position);
        var normalizedPosition = position.normalized;

        if (distance > maxRange)
        {

            stick.localPosition = new Vector2(maxRange * normalizedPosition.x, maxRange * normalizedPosition.y);
            return;
        }
        else
        {
            stick.localPosition = position;
        }

    }

    private void ResetPosition()
    {
        stick.localPosition = Vector2.zero;
    }
}
