using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    private static Dictionary<Vector2Int, Node> m_dictNodes = new Dictionary<Vector2Int, Node>();
    private static List<Vector2Int> m_cellsToSearch;
    private static List<Vector2Int> m_searchedCells;
    private static List<Vector2Int> m_finalPath;

    private static readonly Vector2Int[] m_directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // trên
        new Vector2Int(1, 0),  // phải
        new Vector2Int(0, -1),   // dưới
        new Vector2Int(-1, 0),  // trái
    };

    public static void CreateNodesByGrid(int gridWidth, int gridHeight)
    {
        m_dictNodes.Clear();
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                m_dictNodes.Add(pos, new Node(pos));
            }
        }
    }
    public static void ChangeStateNode(Vector2Int gridPos, Enum.CellState cellState)
    {
        if (m_dictNodes.TryGetValue(gridPos, out Node node))
        {
            node.isWall = cellState == Enum.CellState.Dot;
        }
    }
    public static List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        // Reset lại dữ liệu trước khi tìm đường mới
        m_cellsToSearch = new List<Vector2Int>();
        m_searchedCells = new List<Vector2Int>();
        m_finalPath = new List<Vector2Int>();

        // Reset tất cả node (rất quan trọng khi tìm nhiều lần)
        foreach (var node in m_dictNodes.Values)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.fCost = int.MaxValue;
            node.connection = new Vector2Int(-1, -1);
        }

        m_cellsToSearch.Add(startPos);

        m_dictNodes[startPos].gCost = 0;
        m_dictNodes[startPos].hCost = GetDistance(startPos, endPos);
        m_dictNodes[startPos].fCost = m_dictNodes[startPos].hCost;

        while (m_cellsToSearch.Count > 0)
        {
            // Tìm node có fCost nhỏ nhất
            Vector2Int current = m_cellsToSearch[0];
            foreach (Vector2Int pos in m_cellsToSearch)
            {
                Node node = m_dictNodes[pos];
                Node currentNode = m_dictNodes[current];
                if (node.fCost < currentNode.fCost ||
                    (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
                {
                    current = pos;
                }
            }

            m_cellsToSearch.Remove(current);
            m_searchedCells.Add(current);

            if (current == endPos)
            {
                // Tái tạo đường đi
                m_finalPath.Add(endPos);
                Vector2Int prev = m_dictNodes[endPos].connection;

                while (prev != startPos && prev != new Vector2Int(-1, -1))
                {
                    m_finalPath.Add(prev);
                    prev = m_dictNodes[prev].connection;
                }
                m_finalPath.Add(startPos);
                m_finalPath.Reverse(); // quan trọng: đảo ngược để từ start → end
                return m_finalPath;
            }

            SearchCellNeighbors(current, endPos);
        }
        return null;
    }

    private static void SearchCellNeighbors(Vector2Int cellPos, Vector2Int endPos)
    {
        foreach (var dir in m_directions)
        {
            Vector2Int neighborPos = cellPos + dir;
            if (m_dictNodes.TryGetValue(neighborPos, out Node c) && !m_searchedCells.Contains(neighborPos) && !m_dictNodes[neighborPos].isWall)
            {
                int GcostToNeighbour = m_dictNodes[cellPos].gCost + GetDistance(cellPos, neighborPos);

                if (GcostToNeighbour < m_dictNodes[neighborPos].gCost)
                {
                    Node neighbourNode = m_dictNodes[neighborPos];

                    neighbourNode.connection = cellPos;
                    neighbourNode.gCost = GcostToNeighbour;
                    neighbourNode.hCost = GetDistance(neighborPos, endPos);
                    neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                    if (!m_cellsToSearch.Contains(neighborPos))
                    {
                        m_cellsToSearch.Add(neighborPos);
                    }
                }
            }

        }
    }

    public static int GetDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return (Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y)) * 10;
    }

    private class Node
    {
        public Vector2Int position;
        public int fCost;
        public int gCost;
        public int hCost;
        public Vector2Int connection;
        public bool isWall;
        public Node(Vector2Int pos)
        {
            position = pos;
        }
    }
}


