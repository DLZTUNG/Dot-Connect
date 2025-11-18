using UnityEngine;
using System.Collections.Generic;

public class HandleDraw : MonoBehaviour
{
    [SerializeField] private InputManager m_inputManager;
    [SerializeField] private GridManager m_gridManager;
    [SerializeField] private LineRenderer m_previewLinePrefab;
    [SerializeField] private GameObject m_linesParent;

    private Dot m_selectedDot;
    private List<Vector2Int> m_currentPath;
    private LineRenderer m_previewLine;
    private Dictionary<int, LineRenderer> m_permanentLines = new Dictionary<int, LineRenderer>();

    private void OnEnable()
    {
        m_inputManager.OnSelectDot += HandleSelectDot;
        m_inputManager.OnDragDot += HandleDragDot;
        m_inputManager.OnReleaseDot += HandleReleaseDot;
    }

    private void HandleSelectDot(Dot dot)
    {
        m_selectedDot = dot;
        Dot sameDot = m_gridManager.GetSameColorDot(m_selectedDot);

        //Animation, sau này xử lí ở lớp khác
        m_selectedDot.OnSelected();
        sameDot.OnSelected();

        m_currentPath = new List<Vector2Int> { dot.gridPos };

        CreatePreviewLine();
        UpdatePreviewLine();
    }

    private void HandleDragDot()
    {
        if (m_selectedDot == null || m_currentPath == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2Int hovered = m_gridManager.GetGridPosFromWorld(mouseWorld);

        Vector2Int currentEnd = m_currentPath[m_currentPath.Count - 1];

        if (hovered == currentEnd) return;

        bool isAdjacent = Mathf.Abs(hovered.x - currentEnd.x) + Mathf.Abs(hovered.y - currentEnd.y) == 1;

        if (isAdjacent && m_gridManager.IsCellValidForColor(hovered, m_selectedDot.colorId))
        {
            Cell hoveredCell = m_gridManager.GetCellAt(hovered);

            if (m_currentPath.Count > 1 && hovered == m_currentPath[m_currentPath.Count - 2])
            {
                Cell beforeCell = m_gridManager.GetCellAt(m_currentPath[m_currentPath.Count - 1]);
                beforeCell.ShowSelectedVisual(false);

                m_currentPath.RemoveAt(m_currentPath.Count - 1);
                Cell removeCell = m_gridManager.GetCellAt(m_currentPath[m_currentPath.Count - 1]);
                removeCell.ShowSelectedVisual(false);
            }
            else if (!m_currentPath.Contains(hovered) && !(m_gridManager.GetCellAt(hovered).cellState == Enum.CellState.Line))
            {
                m_currentPath.Add(hovered);
                hoveredCell.ShowSelectedVisual(true);
            }
            UpdatePreviewLine();
        }
    }

    private void HandleReleaseDot()
    {
        if (m_selectedDot == null || m_currentPath == null || m_currentPath.Count < 2)
        {
            ClearPreview();
            return;
        }

        Vector2Int endPos = m_currentPath[m_currentPath.Count - 1];
        Dot endDot = m_gridManager.GetDotAt(endPos);

        if (endDot != null && endDot.colorId == m_selectedDot.colorId)
        {
            int colorId = m_selectedDot.colorId;
            Color lineColor = m_gridManager.colorDict[colorId];

            // Xóa đường cũ nếu có
            if (m_permanentLines.TryGetValue(colorId, out LineRenderer old))
            {
                Destroy(old.gameObject);
                m_permanentLines.Remove(colorId);
            }

            // Cập nhật trạng thái
            foreach (Vector2Int pos in m_currentPath)
            {
                m_gridManager.GridState[pos.x, pos.y] = colorId;
                Cell cell = m_gridManager.GetCellAt(pos);
                if (cell != null)
                {
                    cell.cellState = Enum.CellState.Line;
                }
            }
            m_selectedDot.dotState = Enum.DotState.Connected;
            endDot.dotState = Enum.DotState.Connected;

            // Tạo đường permanent
            LineRenderer perm = Instantiate(m_previewLinePrefab, m_linesParent.transform);
            SetLinePositions(perm, m_currentPath);
            perm.startWidth = perm.endWidth = 0.3f;
            perm.material = new Material(perm.material);
            perm.material.color = lineColor;
            m_permanentLines[colorId] = perm;

            Debug.Log("Nối thành công hai dot màu: " + colorId);

            // Check win khi tất cả các dot thành trạng thái connected
        }
        else
        {
            Debug.Log("không hợp lệ");
        }
        ClearPreview();
    }

    private void CreatePreviewLine()
    {
        GameObject obj = Instantiate(m_previewLinePrefab.gameObject);
        m_previewLine = obj.GetComponent<LineRenderer>();
        m_previewLine.startWidth = m_previewLine.endWidth = 0.25f;

        m_previewLine.material = new Material(m_previewLine.material);
        m_previewLine.material.color = m_gridManager.colorDict[m_selectedDot.colorId];
    }

    private void UpdatePreviewLine()
    {
        if (m_previewLine == null || m_currentPath == null) return;
        SetLinePositions(m_previewLine, m_currentPath);
    }

    private void SetLinePositions(LineRenderer line, List<Vector2Int> path)
    {
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, m_gridManager.GetCellCenter(path[i]));
        }
    }

    private void ClearPreview()
    {
        if (m_previewLine != null)
        {
            Destroy(m_previewLine.gameObject);
            m_previewLine = null;
        }
        foreach (var pos in m_currentPath)
        {
            Cell cell = m_gridManager.GetCellAt(pos);
            cell.ShowSelectedVisual(false);
        }
        m_selectedDot = null;

        m_currentPath = null;
    }

    private void OnDisable()
    {
        m_inputManager.OnSelectDot -= HandleSelectDot;
        m_inputManager.OnDragDot -= HandleDragDot;
        m_inputManager.OnReleaseDot -= HandleReleaseDot;
    }
}