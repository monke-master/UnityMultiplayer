using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    
    public void Show()
    {
        if (gameObject.activeInHierarchy)
        {
            Hide();
            return;
        }
        
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void Continue()
    {
        Hide();
    }

    public void Restart()
    {
        Hide();
        LevelController.RestartLevel();
    }
    
    public void ToMenu()
    {
        Time.timeScale = 1;
        LevelController.ToMenu();
    }
}
