using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameInputController : MonoBehaviour
{
    public InputField inputField;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void ToLevel()
    {
        string name = inputField.text;
        if (name.Length != 0)
        {
            LevelController.playerName = name;
            SceneManager.LoadScene("Scenes/MenuScene");
        }
    }

    public void GoBack()
    {
        Application.Quit();
    }
}
