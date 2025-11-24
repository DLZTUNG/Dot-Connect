using UnityEngine;
using System.Collections.Generic;

public class HandleDraw : MonoBehaviour
{
    [Header("Manager:")]
    [SerializeField] private InputManager m_inputManager;
    [SerializeField] private GridManager m_gridManager;
    [SerializeField] private VisualSystem m_visualSystem;
    [SerializeField] private Dot m_selectedDot;
    [SerializeField] private List<Vector2Int> m_currentPath;

    [System.Obsolete]
    private void OnEnable()
    {
        m_inputManager.OnSelectDot += HandleSelectDot;
        m_inputManager.OnSelectLine += HandleSelectLine;
        m_inputManager.OnDragDot += HandleDragLine;
        m_inputManager.OnReleaseDot += HandleReleaseLine;
    }

    private void HandleSelectDot(Dot dot)
    {
        m_selectedDot = dot;
        Color dotColor = m_gridManager.colorDict[m_selectedDot.colorId];
        Dot sameDot = m_gridManager.GetSameColorDot(m_selectedDot);

        m_selectedDot.OnSelected();
        sameDot.OnSelected();

        if (m_selectedDot.connection.Count > 0 || sameDot.connection.Count > 0) //Có thể một trong hai dot đã nối nhưng dở
        {
            ResetDot(m_selectedDot, false);
            ResetDot(sameDot, false);
        }
        m_currentPath = new List<Vector2Int> { dot.gridPos };

        m_visualSystem.CreatePreviewLine(dotColor);
        m_visualSystem.UpdatePreviewLine(GetListCenterPosOfPath(m_currentPath));
        m_visualSystem.UpdatePathPreview(m_currentPath, dotColor);
        m_visualSystem.ShowCursorVisual(true, dotColor);
    }

    [System.Obsolete]
    private void HandleSelectLine(Vector2Int gridPos, int gridPosIdx)
    {
        m_selectedDot = m_gridManager.GetDotByPosInConnection(gridPos);
        Color lineColor = m_gridManager.colorDict[m_selectedDot.colorId];
        Dot sameDot = m_gridManager.GetSameColorDot(m_selectedDot);

        sameDot.OnSelected();

        for (int i = gridPosIdx + 1; i < m_selectedDot.connection.Count; i++)
            ResetCell(m_selectedDot.connection[i]);

        m_selectedDot.connection.RemoveRange(gridPosIdx + 1, m_selectedDot.connection.Count - 1 - gridPosIdx);

        m_currentPath = new List<Vector2Int>();
        m_currentPath.AddRange(m_selectedDot.connection);

        //Xoá visual line cũ
        m_visualSystem.RemoveLinePermByIdx(gridPosIdx, m_selectedDot.colorId, false);

        m_visualSystem.CreatePreviewLine(lineColor);
        m_visualSystem.UpdatePreviewLine(GetListCenterPosOfPath(m_currentPath));
        m_visualSystem.UpdatePathPreview(m_currentPath, lineColor);
        m_visualSystem.ShowCursorVisual(true, lineColor);
    }

    [System.Obsolete]
    private void HandleDragLine()
    {
        if (m_selectedDot == null || m_currentPath == null) return;
        int colorId = m_selectedDot.colorId;
        Color currColor = m_gridManager.colorDict[colorId];

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2Int hovered = m_gridManager.GetGridPosFromWorld(mouseWorld);
        Vector2Int currentEnd = m_currentPath[m_currentPath.Count - 1];

        if (!m_gridManager.IsHoveredValid(hovered) || hovered == currentEnd) return;
        if (m_currentPath.Count > 1 && m_gridManager.GetDotAt(currentEnd)) return;

        bool isAdjacent = Mathf.Abs(hovered.x - currentEnd.x) + Mathf.Abs(hovered.y - currentEnd.y) == 1;

        //Xử lí trước tất cả để phục hồi line bị cắt

        //Xử lí khi nối từng ô
        if (isAdjacent && m_gridManager.IsCellValidForColor(hovered, colorId)) //Liền kề và vẽ được màu
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
        else if (isAdjacent && !m_gridManager.IsCellValidForColor(hovered, colorId)) //Không vẽ được màu
        {
            if (m_gridManager.GetDotAt(hovered)) return;
            //Có thể cắt qua line của 1 dot hoặc line đã nối 2 dot
            Dot dotStart = m_gridManager.GetDotByPosInConnection(hovered);
            Dot dotEnd = m_gridManager.GetSameColorDot(dotStart);

            //Gọi cut để remove phần bị cut
            CutAnotherLine(dotStart.connection, hovered, dotStart.colorId);

            if (dotEnd.connection.Count > 0) //Trường hợp line nối hoàn thiện nên reset dot end
                ResetDot(dotEnd, true);

            m_currentPath.Add(hovered);
        }

        //Xử lí khi hover sang ô cách xa ô hiện tại, dùng kết hợp bfs để linh hoạt
        else if (!isAdjacent && m_gridManager.IsCellValidForColor(hovered, colorId)) //không liền kề và vẽ được màu
        {
            // =================================================================
            // Bước 1: Tìm điểm gần nhất trên đường đi hiện tại với hovered
            // =================================================================
            int closestIndex = 0;
            float minDistSqr = float.MaxValue;
            Vector2Int closestPos = m_currentPath[0];

            for (int i = 0; i < m_currentPath.Count; i++)
            {
                Vector2Int p = m_currentPath[i];
                float distSqr = (p.x - hovered.x) * (p.x - hovered.x) + (p.y - hovered.y) * (p.y - hovered.y);
                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    closestPos = p;
                    closestIndex = i;
                }
            }

            // Nếu chính xác là điểm đã có trên đường → không làm gì
            if (minDistSqr == 0) return;
            var newSegment = BFSFinding.FindPath(
                startPos: closestPos,
                endPos: hovered,
                gridWidth: m_gridManager.GridSizeX,
                gridHeight: m_gridManager.GridSizeY,
                canDraw: pos =>
                {
                    var cell = m_gridManager.GetCellAt(pos);
                    bool cellHaveDot = m_gridManager.GetDotAt(pos);
                    // Chỉ đi được nếu:
                    // - Không phải tường (Dot)
                    // - Là ô trống HOẶC là chính 2 đầu màu của mình (nếu có)
                    // - Hoặc là các ô đã nằm trên đường đi hiện tại của mình (cho phép "overwrite" đường cũ)
                    // - Không được là một ô đã có line và có dot
                    // Chỉnh sửa điều kiện để giải quyết việc đi chéo
                    bool isOnCurrentPath = m_currentPath.Contains(pos);
                    return cell.cellState != Enum.CellState.Dot &&
                           (cell.cellState == Enum.CellState.None ||
                            isOnCurrentPath) && !(cell.cellState == Enum.CellState.Line && cellHaveDot);
                });

            if (newSegment == null || newSegment.Count <= 1) return;

            // =================================================================
            // Bước 3: Kiểm tra toàn bộ đoạn đường mới có hợp lệ không
            // (không được đi vào đường của màu khác, không trùng đầu màu khác, v.v.)
            // =================================================================
            for (int i = 1; i < newSegment.Count - 1; i++) // bỏ 2 đầu vì đã kiểm tra rồi
            {
                Vector2Int pos = newSegment[i];

                // Không được đi vào ô đã có màu khác (trừ đường cũ của mình)
                if (!m_currentPath.Contains(pos) &&
                    !m_gridManager.IsCellValidForColor(pos, colorId))
                {
                    return; // không hợp lệ → hủy
                }
            }

            // =================================================================
            // Bước 4: Áp dụng reroute (cắt đuôi cũ + nối đoạn mới)
            // =================================================================
            // Cắt từ closestIndex + 1 trở đi
            if (closestIndex < m_currentPath.Count - 1)
            {
                m_currentPath.RemoveRange(closestIndex + 1, m_currentPath.Count - closestIndex - 1);
            }

            // Nối đoạn mới vào (bỏ điểm đầu vì đã có)
            for (int i = 1; i < newSegment.Count; i++)
            {
                m_currentPath.Add(newSegment[i]);
            }
        }

        m_visualSystem.UpdatePreviewLine(GetListCenterPosOfPath(m_currentPath));
        m_visualSystem.UpdatePathPreview(m_currentPath, currColor);
        m_visualSystem.ShowCursorVisual(true, currColor);
    }

    private void HandleReleaseLine()
    {
        if (m_selectedDot == null || m_currentPath == null || m_currentPath.Count < 1)
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
        m_selectedDot.dotState = Enum.DotState.StartConnection;
        m_selectedDot.connection.AddRange(m_currentPath);

        if (endDot != null && endDot.colorId == m_selectedDot.colorId)
        {
            //Nếu nối đúng thì set state và connection cho dot còn lại
            endDot.dotState = Enum.DotState.EndConnection;
            endDot.connection.AddRange(m_currentPath);
            endDot.connection.Reverse();
            //Nối thành công

            // Check win khi tất cả các dot thành trạng thái connected
        }
        /*else
        {
            Debug.Log("không hợp lệ");
        }*/
        m_visualSystem.CreateLinePermanent(m_currentPath, GetListCenterPosOfPath(m_currentPath), colorId, lineColor);
        m_visualSystem.UpdatePathPreview(m_currentPath, Color.white);
        m_visualSystem.ShowCursorVisual(false, Color.white);
        ClearData();
    }

    private void ResetDot(Dot dotReset, bool isCut)
    {
        //Reset hai trường hợp
        //- Bị cắt, thì dot truyền vào là dot end sẽ reset hết trạng thái
        //- Reset khi chạm vào line đã nối hợp lệ để nối lại
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
        if (!isCut)
            if (m_visualSystem.permanentLines.TryGetValue(colorId, out LineRenderer old))
            {
                Destroy(old.gameObject);
                m_visualSystem.permanentLines.Remove(colorId);
            }
    }

    [System.Obsolete]
    private void CutAnotherLine(List<Vector2Int> connection, Vector2Int cutPos, int colorId)
    {
        int gridPosIdx = connection.IndexOf(cutPos);

        //Cập nhật state các ô đã xoá
        for (int i = gridPosIdx; i < connection.Count; i++)
            ResetCell(connection[i]);

        //Xoá trong list
        connection.RemoveRange(gridPosIdx, connection.Count - gridPosIdx);
        //Xoá trên visual
        m_visualSystem.RemoveLinePermByIdx(gridPosIdx, colorId, true);
    }

    private void ResetCell(Vector2Int pos)
    {
        m_gridManager.GetCellAt(pos).cellState = Enum.CellState.None;
        m_gridManager.GridState[pos.x, pos.y] = 0;
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

    [System.Obsolete]
    private void OnDisable()
    {
        m_inputManager.OnSelectDot -= HandleSelectDot;
        m_inputManager.OnSelectLine -= HandleSelectLine;
        m_inputManager.OnDragDot -= HandleDragLine;
        m_inputManager.OnReleaseDot -= HandleReleaseLine;
    }
}