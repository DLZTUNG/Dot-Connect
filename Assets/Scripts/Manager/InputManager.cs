using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public event Action<Dot> OnSelectDot;
    public event Action<Vector2Int, int> OnSelectLine;
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        Dot dotSelected = null;
        LinePreview lineSelected = null;

        Vector3 posLineHit = Vector3.zero;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            //Kiểm tra có Dot không, nếu có thì break luôn không cần xét đến line
            if (hit.collider.TryGetComponent<Dot>(out Dot dot))
            {
                dotSelected = dot;
                break;
            }

            //Kiểm tra có Line không
            if (lineSelected == null && hit.collider.GetComponentInParent<LinePreview>())
            {
                lineSelected = hit.collider.GetComponentInParent<LinePreview>();
                posLineHit = hit.collider.transform.position;
            }
        }

        if (dotSelected != null)
        {
            OnSelectDot?.Invoke(dotSelected);
            return;
        }

        if (lineSelected != null)
        {
            Vector2Int gridPos = lineSelected.GetGridPosFromWorldPos(posLineHit);
            int gridPosIdx = lineSelected.GetGridPosIdxFromGridPos(gridPos);
            OnSelectLine?.Invoke(gridPos, gridPosIdx);
            return;
        }
    }

}
