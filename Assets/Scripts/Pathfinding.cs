using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    private static Dictionary<Vector2Int, Node> dictNodes = new Dictionary<Vector2Int, Node>();
    private static List<Vector2Int> cellsToSearch;
    private static List<Vector2Int> searchedCells;
    private static List<Vector2Int> finalPath;

    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // trên
        new Vector2Int(1, 0),  // phải
        new Vector2Int(0, -1),   // dưới
        new Vector2Int(-1, 0),  // trái
    };

    public static void CreateNodesByGrid(int gridWidth, int gridHeight)
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                dictNodes.Add(pos, new Node(pos));
            }
        }
    }
    public static void ChangeStateNode(Vector2Int gridPos, Enum.CellState cellState)
    {
        if (dictNodes.TryGetValue(gridPos, out Node node))
        {
            node.isWall = cellState == Enum.CellState.Dot || true || false;
        }
    }
    public static void ClearData()
    {
        dictNodes.Clear();
    }
    public static List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        // Reset lại dữ liệu trước khi tìm đường mới
        cellsToSearch = new List<Vector2Int>();
        searchedCells = new List<Vector2Int>();
        finalPath = new List<Vector2Int>();

        // Reset tất cả node (rất quan trọng khi tìm nhiều lần)
        foreach (var node in dictNodes.Values)
        {
            node.gCost = int.MaxValue;
            node.hCost = 0;
            node.fCost = int.MaxValue;
        }

        cellsToSearch.Add(startPos);

        dictNodes[startPos].gCost = 0;
        dictNodes[startPos].hCost = GetDistance(startPos, endPos);
        dictNodes[startPos].fCost = dictNodes[startPos].hCost;

        while (cellsToSearch.Count > 0)
        {
            // Tìm node có fCost nhỏ nhất
            Vector2Int current = cellsToSearch[0];
            foreach (Vector2Int pos in cellsToSearch)
            {
                Node node = dictNodes[pos];
                Node currentNode = dictNodes[current];
                if (node.fCost < currentNode.fCost ||
                    (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
                {
                    current = pos;
                }
            }

            cellsToSearch.Remove(current);
            searchedCells.Add(current);

            if (current == endPos)
            {
                // Tái tạo đường đi
                finalPath.Add(endPos);
                Vector2Int prev = dictNodes[endPos].connection;

                while (prev != startPos && prev != new Vector2Int(-1, -1))
                {
                    finalPath.Add(prev);
                    prev = dictNodes[prev].connection;
                }
                finalPath.Add(startPos);
                finalPath.Reverse(); // quan trọng: đảo ngược để từ start → end
                return finalPath;
            }

            SearchCellNeighbors(current, endPos);
        }

        Debug.Log("Không tìm thấy đường đi!");
        return null;
    }

    private static void SearchCellNeighbors(Vector2Int cellPos, Vector2Int endPos)
    {
        foreach (var dir in directions)
        {
            Vector2Int neighborPos = cellPos + dir;
            if (dictNodes.TryGetValue(neighborPos, out Node c) && !searchedCells.Contains(neighborPos) && !dictNodes[neighborPos].isWall)
            {
                int GcostToNeighbour = dictNodes[cellPos].gCost + GetDistance(cellPos, neighborPos);

                if (GcostToNeighbour < dictNodes[neighborPos].gCost)
                {
                    Node neighbourNode = dictNodes[neighborPos];

                    neighbourNode.connection = cellPos;
                    neighbourNode.gCost = GcostToNeighbour;
                    neighbourNode.hCost = GetDistance(neighborPos, endPos);
                    neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                    if (!cellsToSearch.Contains(neighborPos))
                    {
                        cellsToSearch.Add(neighborPos);
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


