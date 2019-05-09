using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool GamePaused = false;

    public GameObject pauseMenuUI;
    public GameObject volumeMenuUI;
    public GameObject standardMenuUI;
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(GamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
	}

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        volumeMenuUI.SetActive(false);
        standardMenuUI.SetActive(true);
        Time.timeScale = 1f;
        GamePaused = false;
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        standardMenuUI.SetActive(false);
        Time.timeScale = 0f;    //Freeze game
        GamePaused = true;
    }
    //SceneManager.LoadScene("Start Menu");

    public void QuitGame()
    {
        Application.Quit();
    }
}
