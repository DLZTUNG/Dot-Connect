using UnityEngine;

public class Cell : MonoBehaviour
{
    public Enum.CellState cellState;
    public Vector2Int gridPos;

    public void ShowSelectedVisual(bool isShow)
    {
        GameObject visual = transform.GetChild(0).gameObject;
        visual.SetActive(isShow);
    }
}
