using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        Debug.Log("back button");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
        //SceneManager.LoadScene("MainMenu");

    }
}
