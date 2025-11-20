using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public event Action<Dot> OnSelectDot;
    public event Action<Vector3> OnSelectLine;
    public event Action OnDragDot;
    public event Action OnReleaseDot;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckMousePosition();
        }
        if (Input.GetMouseButton(0))
        {
            OnDragDot?.Invoke();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnReleaseDot?.Invoke();
        }
    }

    public void CheckMousePosition()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject.TryGetComponent<Dot>(out Dot dot))
        {
            OnSelectDot?.Invoke(dot);
        }
    }
}
