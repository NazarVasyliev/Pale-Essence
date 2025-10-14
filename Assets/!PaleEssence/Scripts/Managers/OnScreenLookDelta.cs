using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem;

[AddComponentMenu("Input/On-Screen Look")]
public class OnScreenLookDelta : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 m_Delta = Vector2.zero;
    private int m_PointerId = -1;
    private Vector2 m_StartPos;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath = "<Mouse>/delta";

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (m_PointerId != -1) return;
        m_PointerId = data.pointerId;
        m_StartPos = data.position;
    }

    public void OnDrag(PointerEventData data)
    {
        if (data.pointerId != m_PointerId) return;
        Vector2 currentDelta = data.position - m_StartPos;
        m_StartPos = data.position;
        SendValueToControl(currentDelta);
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (data.pointerId != m_PointerId) return;
        SendValueToControl(Vector2.zero);
        m_PointerId = -1;
    }

}
