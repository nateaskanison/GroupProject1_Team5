using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    public void ToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Update is called once per frame
    public void QuitGame()
    {
        Application.Quit();
    }
}
