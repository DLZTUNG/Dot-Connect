using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera m_mainCamera;
    public void AdjustCam(int gridSizeX, int gridSizeY, float cellSize)
    {
        float xPos = (gridSizeX * cellSize - cellSize) / 2f;
        float yPos = (gridSizeY * cellSize - cellSize) / 2f;
        m_mainCamera.transform.position = new Vector3(xPos, yPos, -10f);
        m_mainCamera.orthographicSize = gridSizeX * 0.9f;
    }
}
