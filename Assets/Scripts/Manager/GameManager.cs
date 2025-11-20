using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void OnRestart()
    {
        SceneManager.LoadScene(0);
        Pathfinding.ClearData();
    }
}
