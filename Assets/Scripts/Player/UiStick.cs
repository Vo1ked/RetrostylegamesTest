using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiStick : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IDragHandler, IPauseHandler
{
    public Action<Vector2> OnPositionChanged = (Vector2) => { };

    [SerializeField] private float _maxRange = 50;
    [SerializeField] private RectTransform _stick;

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

        if (distance > _maxRange)
        {

            _stick.localPosition = new Vector2(_maxRange * normalizedPosition.x, _maxRange * normalizedPosition.y);
            return;
        }
        else
        {
            _stick.localPosition = position;
        }

    }

    private void ResetPosition()
    {
        _stick.localPosition = Vector2.zero;
    }
}
