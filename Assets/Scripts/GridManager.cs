using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    [Header("Managers:")]
    [SerializeField] private CameraController m_cameraController;
    [Header("Object:")]
    [SerializeField] private GameObject m_gridParent;
    [SerializeField] private GameObject m_dotsParent;
    [Header("Prefabs:")]
    [SerializeField] private GameObject m_dotPrb;
    [SerializeField] private GameObject m_cellPrb;

    [Header("Grid Settings:")]
    [SerializeField] private int m_gridSizeX;
    [SerializeField] private int m_gridSizeY;
    [SerializeField] private float m_cellSize;
    [Header("Dot Settings:")]
    [SerializeField] private int m_dotPairCount;

    private Transform[,] m_gridCells;
    private List<Cell> m_cells = new List<Cell>();
    private List<Dot> m_dots = new List<Dot>();
    private int[,] m_gridState;

    public int[,] GridState { get => m_gridState; set => m_gridState = value; }
    public Dictionary<int, Color> colorDict = new Dictionary<int, Color>()
    {
        {0, Color.red },
        {1, Color.green },
        {2, Color.blue },
        {3, Color.yellow },
        {4, Color.cyan },
        {5, Color.magenta }
    };

    private void Start()
    {
        m_dotPairCount = Random.Range(4, 7);
        Initialize();
    }

    private void Initialize()
    {
        CreateGrid();
        SetGridState();
        var snakePath = GenerateSnakePath();
        GetSegmentAndGenerateDot(snakePath);
    }

    private void SetGridState()
    {
        m_gridState = new int[m_gridSizeX, m_gridSizeY];
        for (int x = 0; x < m_gridSizeX; x++)
        {
            for (int y = 0; y < m_gridSizeY; y++)
            {
                m_gridState[x, y] = 0; // unoccupied
            }
        }
    }
    private void GetSegmentAndGenerateDot(List<Vector2Int> snakePath)
    {
        List<List<Vector2Int>> segments = new List<List<Vector2Int>>();
        /*do
        {
            segments = RandomSplitPath(snakePath, m_dotPairCount);
        } while (IsValidSegments(segments) == false);*/

        // ĐANG BỊ LỖI
        segments = RandomSplitPath(snakePath, m_dotPairCount);

        GenerateDot(segments);
    }
    private void LateUpdate()
    {
        m_cameraController.AdjustCam(m_gridSizeX, m_gridSizeY, m_cellSize);
    }

    private void CreateGrid()
    {
        m_gridCells = new Transform[m_gridSizeX, m_gridSizeY];

        for (int x = 0; x < m_gridSizeX; x++)
        {
            for (int y = 0; y < m_gridSizeY; y++)
            {
                GameObject cell = Instantiate(m_cellPrb);
                cell.name = $"Cell_{x}_{y}";
                cell.transform.position = new Vector2(x * m_cellSize, y * m_cellSize);
                cell.transform.parent = m_gridParent.transform;
                m_gridCells[x, y] = cell.transform;

                var cellScript = cell.GetComponent<Cell>();
                cellScript.gridPos = new Vector2Int(x, y);
                cellScript.cellState = Enum.CellState.UnOccupied;
                m_cells.Add(cellScript);
            }
        }
    }
    private List<Vector2Int> GenerateSnakePath()
    {
        List<Vector2Int> path = new List<Vector2Int>();

        for (int y = 0; y < m_gridSizeY; y++)
        {
            if (y % 2 == 0)
            {
                for (int x = 0; x < m_gridSizeX; x++)
                    path.Add(new Vector2Int(x, y));
            }
            else
            {
                for (int x = m_gridSizeX - 1; x >= 0; x--)
                    path.Add(new Vector2Int(x, y));
            }
        }

        return path;
    }

    List<List<Vector2Int>> RandomSplitPath(List<Vector2Int> fullPath, int lineCount)
    {
        int total = fullPath.Count;

        int minLen = 4;

        int minRequired = lineCount * minLen;

        if (minRequired > total)
        {
            return null;
        }

        List<int> lengths = new();

        int remaining = total - minRequired;

        System.Random rng = new System.Random();

        for (int i = 0; i < lineCount; i++)
        {

            if (i == lineCount - 1)
            {
                lengths.Add(minLen + remaining);
            }
            else
            {
                int extra = rng.Next(0, remaining + 1); // random 0 → remaining
                lengths.Add(minLen + extra);
                remaining -= extra;
            }
        }

        // Shuffle list độ dài
        lengths = lengths.OrderBy(_ => rng.Next()).ToList();

        List<List<Vector2Int>> segments = new();
        int index = 0;

        foreach (var len in lengths)
        {
            segments.Add(fullPath.GetRange(index, len));
            index += len;
        }

        return segments;
    }

    private bool IsValidSegments(List<List<Vector2Int>> segments)
    {
        foreach (var segment in segments)
        {
            if (Vector2Int.Distance(segment[0], segment[segment.Count - 1]) < 3)
                return false;
        }
        return true;
    }
    private void GenerateDot(List<List<Vector2Int>> segments)
    {
        int colorId = 0;
        foreach (var segment in segments)
        {
            var dotCompStart = CreateDot(segment[0], colorId);
            m_dots.Add(dotCompStart);
            var dotCompEnd = CreateDot(segment[segment.Count - 1], colorId);
            m_dots.Add(dotCompEnd);

            colorId++;
        }
    }

    private Dot CreateDot(Vector2Int gridPos, int colorId)
    {
        for (int i = 0; i < m_gridSizeX; i++)
        {
            for (int j = 0; j < m_gridSizeY; j++)
            {
                if (m_gridState[i, j] == 0 && i == gridPos.x && j == gridPos.y)
                {
                    m_gridState[i, j] = colorId; // occupied by colorid
                    break;
                }
            }
        }

        GameObject dotObj = Instantiate(m_dotPrb);
        dotObj.name = $"Dot_color {colorId}";
        dotObj.transform.position = new Vector2(gridPos.x * m_cellSize, gridPos.y * m_cellSize);
        dotObj.transform.parent = m_dotsParent.transform;
        Dot dotComp = dotObj.GetComponent<Dot>();
        if (dotComp != null)
        {
            dotComp.gridPos = gridPos;
            dotComp.colorId = colorId;
            dotComp.dotState = Enum.DotState.Idle;

            Color color = colorDict[colorId];
            dotComp.SetColor(color);
        }
        foreach (var cell in m_cells)
        {
            if (cell.gridPos == gridPos && cell.cellState == Enum.CellState.UnOccupied)
            {
                cell.cellState = Enum.CellState.Occupied;
                break;
            }
        }
        return dotComp;
    }

    public Vector2Int GetGridPosFromWorld(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / m_cellSize);
        int y = Mathf.RoundToInt(worldPos.y / m_cellSize);
        if (x >= 0 && x < m_gridSizeX && y >= 0 && y < m_gridSizeY)
            return new Vector2Int(x, y);
        return new Vector2Int(-1, -1);
    }

    public Vector3 GetCellCenter(Vector2Int pos)
    {
        return m_gridCells[pos.x, pos.y].position;
    }

    public Dot GetDotAt(Vector2Int pos)
    {
        foreach (var dot in m_dots)
        {
            if (dot.gridPos == pos)
                return dot;
        }
        return null;
    }

    public Cell GetCellAt(Vector2Int pos)
    {
        return m_gridCells[pos.x, pos.y].GetComponent<Cell>();
    }

    public bool IsCellValidForColor(Vector2Int pos, int colorId)
    {
        int state = m_gridState[pos.x, pos.y];
        return state == 0 || state == colorId;
    }

    public Dot GetSameColorDot(Dot dotCheck)
    {
        foreach (var dot in m_dots)
        {
            if (dotCheck.colorId == dot.colorId && dotCheck != dot)
            {
                return dot;
            }
        }
        return null;
    }
    private void OnDrawGizmos()
    {
        if (m_gridCells == null) return;

        Gizmos.color = Color.red;
        for (int x = 0; x < m_gridSizeX; x++)
        {
            for (int y = 0; y < m_gridSizeY; y++)
            {
                Gizmos.DrawWireCube(m_gridCells[x, y].position, Vector3.one * m_cellSize);
            }
        }
    }
}