using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualSystem : MonoBehaviour
{
    [Header("Parents:")]
    [SerializeField] private GameObject m_cellPreviewsParent;
    [SerializeField] private GameObject m_linesParent;
    [Header("Prefabs:")]
    [SerializeField] private GameObject m_cursorPreviewPrb;
    [SerializeField] private GameObject m_cellPreviewPrb;
    [SerializeField] private LineRenderer m_previewLinePrb;
    private LineRenderer m_previewLine;
    private GameObject m_cursorPreviewObj;
    private Dictionary<Vector2Int, GameObject> m_dictCellPreviewObj = new Dictionary<Vector2Int, GameObject>();
    private bool m_isCursorVisible;

    public Dictionary<int, LineRenderer> permanentLines = new Dictionary<int, LineRenderer>();

    private void Start()
    {
        m_cursorPreviewObj = Instantiate(m_cursorPreviewPrb, transform);
        m_cursorPreviewObj.SetActive(false);
    }

    public void GenerateCellVisualGrid(Transform cellTrans, Vector2Int gridPos)
    {
        GameObject cellPreviewObj = Instantiate(m_cellPreviewPrb, m_cellPreviewsParent.transform);
        cellPreviewObj.transform.position = cellTrans.position;

        m_dictCellPreviewObj.Add(gridPos, cellPreviewObj);
        cellPreviewObj.SetActive(false);
    }
    public void ShowCursorVisual(bool isShow, Color color)
    {
        m_isCursorVisible = isShow;
        Color temp = color;
        temp.a = 0.2f;
        m_cursorPreviewObj.GetComponent<SpriteRenderer>().color = temp;
        m_cursorPreviewObj.SetActive(isShow);
    }

    private void Update()
    {
        if (!m_isCursorVisible) return;
        UpdateCursorPosition();
    }

    [System.Obsolete]
    private LinePreview FindLinePreviewWithColorId(int colorId)
    {
        LinePreview[] all = FindObjectsOfType<LinePreview>();

        foreach (var line in all)
        {
            if (line.lineColorId == colorId)
                return line;
        }
        return null;
    }

    private void UpdateCursorPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        m_cursorPreviewObj.transform.position = mousePos;
    }

    private void AddPosToLineData(LineRenderer line, List<Vector2Int> pathOnGrid, List<Vector3> pathOnWorld, int colorId)
    {
        var lineScript = line.gameObject.GetComponent<LinePreview>();

        if (lineScript.listPosOfLineOnGrid.Count > 0 || lineScript.listPosOfLineOnWorld.Count > 0) return;

        int countPos = pathOnGrid.Count;
        lineScript.lineColorId = colorId;

        foreach (Vector2Int pos in pathOnGrid)
            lineScript.listPosOfLineOnGrid.Add(pos);
        foreach (Vector3 pos in pathOnWorld)
            lineScript.listPosOfLineOnWorld.Add(pos);
    }
    private void SetLinePositions(LineRenderer line, List<Vector3> path)
    {
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i]);
        }
    }
    private void UpdateLineWhenRemove(LineRenderer line, int newCount, int count)
    {
        Vector3[] pts = new Vector3[count];
        line.GetPositions(pts);

        Vector3[] newPts = new Vector3[newCount];
        for (int i = 0; i < newCount; i++)
            newPts[i] = pts[i];

        line.positionCount = newCount;
        line.SetPositions(newPts);
    }

    public void UpdatePathPreview(List<Vector2Int> currentPath, Color pathColor)
    {
        foreach (var preview in m_dictCellPreviewObj.Values)
        {
            preview.SetActive(false);
        }
        if (pathColor == Color.white) return;
        for (int i = 0; i < currentPath.Count; i++)
        {
            Vector2Int pos = currentPath[i];
            if (m_dictCellPreviewObj.TryGetValue(pos, out GameObject cellPreview))
            {
                Color tempColor = pathColor;
                tempColor.a = 0.3f;
                cellPreview.GetComponent<SpriteRenderer>().color = tempColor;
                cellPreview.SetActive(true);
            }
        }
    }
    public void CreatePreviewLine(Color color)
    {
        GameObject obj = Instantiate(m_previewLinePrb.gameObject);
        m_previewLine = obj.GetComponent<LineRenderer>();
        m_previewLine.startWidth = m_previewLine.endWidth = 0.2f;

        m_previewLine.material = new Material(m_previewLine.material);
        m_previewLine.material.color = color;
    }
    public void CreateLinePermanent(List<Vector2Int> currPathOnGrid, List<Vector3> currPathOnWorld, int colorId, Color color)
    {
        LineRenderer perm = Instantiate(m_previewLinePrb, m_linesParent.transform);

        AddPosToLineData(perm, currPathOnGrid, currPathOnWorld, colorId);
        SetLinePositions(perm, currPathOnWorld);
        perm.startWidth = perm.endWidth = 0.2f;
        perm.material = new Material(perm.material);
        perm.material.color = color;
        permanentLines[colorId] = perm;
    }
    public void UpdatePreviewLine(List<Vector3> currPath)
    {
        if (m_previewLine == null || currPath == null) return;
        SetLinePositions(m_previewLine, currPath);
    }

    [System.Obsolete]
    public void RemoveLinePermByIdx(int idx, int colorId, bool isCut)
    {
        LineRenderer line = FindLinePreviewWithColorId(colorId).GetComponent<LineRenderer>();
        int count = line.positionCount;
        if (idx < 0) return;

        int newCount;
        if (isCut)
        {
            newCount = idx; //Cắt thì giữ lại từ idx trở về trước
            UpdateLineWhenRemove(line, newCount, count);
            //Update collider 
            line.gameObject.GetComponent<LinePreview>().RemoveLineCollider(idx);
        }
        else //Redraw thì xoá các vị trí sau hover để draw tiếp từ hover
        {
            //redraw có trường hợp là hover ở cuối, thì không cần xoá gì cả
            if (idx >= count - 1) return;

            newCount = idx + 1;
            UpdateLineWhenRemove(line, newCount, count);
        }
    }
    public void ClearPreview()
    {
        if (m_previewLine != null)
        {
            Destroy(m_previewLine.gameObject);
            m_previewLine = null;
        }
    }
}
