using System.Collections.Generic;
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
    public Dictionary<int, LineRenderer> permanentLines = new Dictionary<int, LineRenderer>();
    private bool m_isCursorVisible;

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

    private void UpdateCursorPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        m_cursorPreviewObj.transform.position = mousePos;
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
    public void CreateLinePermanent(List<Vector3> currPath, int colorId, Color color)
    {
        LineRenderer perm = Instantiate(m_previewLinePrb, m_linesParent.transform);
        SetLinePositions(perm, currPath);
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
    public void SetLinePositions(LineRenderer line, List<Vector3> path)
    {
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i]);
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
