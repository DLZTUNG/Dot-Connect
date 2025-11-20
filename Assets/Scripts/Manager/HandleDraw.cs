using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandleDraw : MonoBehaviour
{
    [Header("Manager:")]
    [SerializeField] private InputManager m_inputManager;
    [SerializeField] private GridManager m_gridManager;
    [SerializeField] private VisualSystem m_visualSystem;
    [SerializeField] private Dot m_selectedDot;
    [SerializeField] private List<Vector2Int> m_currentPath;

    private void OnEnable()
    {
        m_inputManager.OnSelectDot += HandleSelectDot;
        m_inputManager.OnDragDot += HandleDragDot;
        m_inputManager.OnReleaseDot += HandleReleaseDot;
    }

    private void HandleSelectDot(Dot dot)
    {
        m_selectedDot = dot;
        Color dotColor = m_gridManager.colorDict[m_selectedDot.colorId];
        Dot sameDot = m_gridManager.GetSameColorDot(m_selectedDot);
        //Animation, sau này xử lí ở lớp khác
        m_selectedDot.OnSelected();
        sameDot.OnSelected();

        if (m_selectedDot.connection.Count > 0 || sameDot.connection.Count > 0) //Có thể một trong hai dot đã nối nhưng dở
        {
            ResetDot(m_selectedDot);
            ResetDot(sameDot);
        }
        m_currentPath = new List<Vector2Int> { dot.gridPos };

        m_visualSystem.CreatePreviewLine(dotColor);
        m_visualSystem.UpdatePreviewLine(GetListCenterPosOfPath(m_currentPath));
        m_visualSystem.UpdatePathPreview(m_currentPath, m_gridManager.colorDict[m_selectedDot.colorId]);
        m_visualSystem.ShowCursorVisual(true, dotColor);
    }

    private void HandleDragDot()
    {
        if (m_selectedDot == null || m_currentPath == null) return;
        int colorId = m_selectedDot.colorId;
        Color currColor = m_gridManager.colorDict[colorId];

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2Int hovered = m_gridManager.GetGridPosFromWorld(mouseWorld);
        Vector2Int currentEnd = m_currentPath[m_currentPath.Count - 1];

        if (!m_gridManager.IsHoveredValid(hovered) || hovered == currentEnd) return;

        bool isAdjacent = Mathf.Abs(hovered.x - currentEnd.x) + Mathf.Abs(hovered.y - currentEnd.y) == 1;

        //Xử lí khi nối từng ô
        if (isAdjacent && m_gridManager.IsCellValidForColor(hovered, colorId))
        {
            if (m_currentPath.Count > 1 && hovered == m_currentPath[m_currentPath.Count - 2]) //Hover vào ô trước đó, tương đương draw ngược lại
            {
                m_currentPath.RemoveAt(m_currentPath.Count - 1);
            }
            else if (!m_currentPath.Contains(hovered) && m_gridManager.GetCellAt(hovered).cellState != Enum.CellState.Line)
            {
                m_currentPath.Add(hovered);
            }
        }
        //Xử lí khi cắt line màu khác
        else if (!m_gridManager.IsCellValidForColor(hovered, colorId))
        {
            if (m_gridManager.GetDotAt(hovered)) return;
            //Có thể cắt qua line của 1 dot hoặc line đã nối 2 dot
            Dot dotCheck1 = m_gridManager.GetDotByPosInConnection(hovered); //Dot chứa line đã cắt
            if (dotCheck1 == null) return;
            Dot dotCheck2 = m_gridManager.GetSameColorDot(dotCheck1); //Lấy dot same

            ResetDot(dotCheck1);
            ResetDot(dotCheck2);
            m_currentPath.Add(hovered);
        }

        //Xử lí khi hover sang ô cách xa ô hiện tại, dùng kết hợp pathfinding để linh hoạt
        else if (!isAdjacent && m_gridManager.IsCellValidForColor(hovered, colorId))
        {
            Vector2Int closestPos = m_currentPath[0];
            int minDistance = Pathfinding.GetDistance(m_currentPath[0], hovered);
            int closestIndex = 0;

            for (int i = 1; i < m_currentPath.Count; i++)
            {
                int dist = Pathfinding.GetDistance(m_currentPath[i], hovered);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPos = m_currentPath[i];
                    closestIndex = i;
                }
            }

            if (minDistance == 0) return;

            List<Vector2Int> newPathSegment = Pathfinding.FindPath(closestPos, hovered);

            if (newPathSegment != null && newPathSegment.Count > 1)
            {
                // Kiểm tra segment mới valid (không overlap với path cũ trừ closest)
                bool isValid = true;
                for (int i = 1; i < newPathSegment.Count; i++)
                {
                    Vector2Int pos = newPathSegment[i];
                    if (m_currentPath.Contains(pos) || !m_gridManager.IsCellValidForColor(pos, colorId) || m_gridManager.GetCellAt(pos).cellState != Enum.CellState.None)
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                {
                    // Backtrack: Remove các ô sau closestIndex
                    if (closestIndex < m_currentPath.Count - 1)
                    {
                        m_currentPath.RemoveRange(closestIndex + 1, m_currentPath.Count - closestIndex - 1);
                    }

                    // Append segment mới (bỏ closestPos vì đã có)
                    m_currentPath.AddRange(newPathSegment.GetRange(1, newPathSegment.Count - 1));
                }
            }
        }

        m_visualSystem.UpdatePreviewLine(GetListCenterPosOfPath(m_currentPath));
        m_visualSystem.UpdatePathPreview(m_currentPath, currColor);
        m_visualSystem.ShowCursorVisual(true, currColor);
    }

    private void HandleReleaseDot()
    {
        if (m_selectedDot == null || m_currentPath == null || m_currentPath.Count < 2)
        {
            ClearData();
            return;
        }

        Vector2Int endPos = m_currentPath[m_currentPath.Count - 1];
        Dot endDot = m_gridManager.GetDotAt(endPos);

        int colorId = m_selectedDot.colorId;
        Color lineColor = m_gridManager.colorDict[colorId];

        if (m_visualSystem.permanentLines.TryGetValue(colorId, out LineRenderer old))
        {
            Destroy(old.gameObject);
            m_visualSystem.permanentLines.Remove(colorId);
        }
        // Cập nhật trạng thái các cell đã nối
        foreach (Vector2Int pos in m_currentPath)
        {
            m_gridManager.GridState[pos.x, pos.y] = colorId;
            Cell cell = m_gridManager.GetCellAt(pos);
            if (cell != null)
            {
                cell.cellState = Enum.CellState.Line;
            }
        }

        //Cập nhật state và connection hiện tại của dot selected
        m_selectedDot.dotState = Enum.DotState.Connected;
        m_selectedDot.connection.AddRange(m_currentPath);

        if (endDot != null && endDot.colorId == m_selectedDot.colorId)
        {
            //Nếu nối đúng thì set state và connection cho dot còn lại
            endDot.dotState = Enum.DotState.Connected;
            endDot.connection.AddRange(m_currentPath);
            endDot.connection.Reverse();

            Debug.Log("Nối thành công hai dot màu: " + colorId);

            // Check win khi tất cả các dot thành trạng thái connected
        }
        else
        {
            Debug.Log("không hợp lệ");
        }
        m_visualSystem.CreateLinePermanent(GetListCenterPosOfPath(m_currentPath), colorId, lineColor);
        m_visualSystem.UpdatePathPreview(m_currentPath, Color.white);
        m_visualSystem.ShowCursorVisual(false, Color.white);
        ClearData();
    }

    private void ResetDot(Dot dotReset)
    {
        int colorId = dotReset.colorId;
        var connection = dotReset.connection;

        dotReset.dotState = Enum.DotState.Idle;

        if (connection.Count > 0)
        {
            for (int i = 0; i < connection.Count; i++)
            {
                var pos = connection[i];
                if (pos == dotReset.gridPos || pos == m_gridManager.GetSameColorDot(dotReset).gridPos)
                {
                    m_gridManager.GetCellAt(pos).cellState = Enum.CellState.Dot;
                    m_gridManager.GridState[pos.x, pos.y] = colorId;
                    continue;
                }
                m_gridManager.GetCellAt(pos).cellState = Enum.CellState.None;
                m_gridManager.GridState[pos.x, pos.y] = 0;
            }
            dotReset.connection.Clear();
        }

        if (m_visualSystem.permanentLines.TryGetValue(colorId, out LineRenderer old))
        {
            Destroy(old.gameObject);
            m_visualSystem.permanentLines.Remove(colorId);
        }
    }

    private List<Vector3> GetListCenterPosOfPath(List<Vector2Int> path)
    {
        List<Vector3> resultList = new List<Vector3>();
        foreach (var pos in path)
            resultList.Add(m_gridManager.GetCellCenter(pos));

        return resultList;
    }
    private void ClearData()
    {
        m_visualSystem.ClearPreview();
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