using System.Collections.Generic;
using UnityEngine;


public static class BFSFinding
{
    private static readonly Vector2Int[] m_directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // lên
        new Vector2Int(1, 0),  // phải
        new Vector2Int(0, -1), // dưới
        new Vector2Int(-1, 0)  // trái
    };

    private static Dictionary<Vector2Int, Vector2Int> m_cameFrom = new Dictionary<Vector2Int, Vector2Int>();

    private static Queue<Vector2Int> m_frontier = new Queue<Vector2Int>();

    private static HashSet<Vector2Int> m_visited = new HashSet<Vector2Int>();

    public static List<Vector2Int> FindPath(
        Vector2Int startPos,
        Vector2Int endPos,
        int gridWidth,
        int gridHeight,
        System.Func<Vector2Int, bool> canDraw)
    {
        // Reset dữ liệu
        m_cameFrom.Clear();
        m_frontier.Clear();
        m_visited.Clear();

        m_frontier.Enqueue(startPos);
        m_visited.Add(startPos);
        m_cameFrom[startPos] = new Vector2Int(-1, -1); // điểm bắt đầu không có cha

        while (m_frontier.Count > 0)
        {
            Vector2Int current = m_frontier.Dequeue();

            // Tìm thấy đích
            if (current == endPos)
            {
                return ReconstructPath(startPos, endPos);
            }

            // Kiểm tra 4 ô lân cận
            foreach (Vector2Int dir in m_directions)
            {
                Vector2Int next = current + dir;

                // Kiểm tra giới hạn lưới
                if (next.x < 0 || next.x >= gridWidth || next.y < 0 || next.y >= gridHeight)
                    continue;

                // Nếu chưa thăm và đi được
                if (!m_visited.Contains(next) && canDraw(next))
                {
                    m_frontier.Enqueue(next);
                    m_visited.Add(next);
                    m_cameFrom[next] = current; // ghi lại cha
                }
            }
        }
        return null;
    }

    //Tạo ngược lại đường đi từ end về
    private static List<Vector2Int> ReconstructPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = end;

        while (current != start)
        {
            path.Add(current);
            if (!m_cameFrom.TryGetValue(current, out current) || current == new Vector2Int(-1, -1))
                return null; // lỗi bất ngờ
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}