using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class LinePreview : MonoBehaviour
{
    [SerializeField] private GameObject m_lineColliderPrb;
    [SerializeField] private GameObject m_lineColliderParent;
    private List<GameObject> m_listColliderObj = new List<GameObject>();

    public Vector3[] arrPosHadRemove = new Vector3[0];
    public List<Vector3> listPosHadRemoveOnWorld = new List<Vector3>();
    public List<Vector2Int> listPosHadRemoveOnGrid = new List<Vector2Int>();
    public List<Vector2Int> listPosOfLineOnGrid = new List<Vector2Int>();
    public List<Vector3> listPosOfLineOnWorld = new List<Vector3>();
    public int lineColorId;
    private void Start()
    {
        SetColliderToListPos();
    }
    private void SetColliderToListPos()
    {
        foreach (var pos in listPosOfLineOnWorld)
        {
            GameObject lineCollider = Instantiate(m_lineColliderPrb, m_lineColliderParent.transform);
            lineCollider.transform.position = pos;
            m_listColliderObj.Add(lineCollider);
        }
    }
    public Vector2Int GetGridPosFromWorldPos(Vector3 worldPos)
    {
        int idx = listPosOfLineOnWorld.IndexOf(worldPos);
        return listPosOfLineOnGrid[idx];
    }
    public void RemoveLineCollider(int posIdx)
    {
        int count = m_listColliderObj.Count;
        for (int i = posIdx; i < count; i++)
        {
            Destroy(m_listColliderObj[i]);
            listPosHadRemoveOnWorld.Add(listPosOfLineOnWorld[i]);
            listPosHadRemoveOnGrid.Add(listPosOfLineOnGrid[i]);
        }

        Vector3[] resultArr = listPosHadRemoveOnWorld.ToArray();
        arrPosHadRemove = resultArr;

        listPosOfLineOnGrid.RemoveRange(posIdx, count - posIdx);
        listPosOfLineOnWorld.RemoveRange(posIdx, count - posIdx);
        m_listColliderObj.RemoveRange(posIdx, count - posIdx);
    }

    public int GetGridPosIdxFromGridPos(Vector2Int gridPos)
    {
        return listPosOfLineOnGrid.IndexOf(gridPos);
    }
}
