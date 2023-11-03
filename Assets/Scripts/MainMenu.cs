using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        //SceneManager.LoadSceneAsync("test1"); //loads next scene
        Debug.Log("PLAY GAME!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //loads next scene
    }
    public void  GameOptions()
    {
        Debug.Log("OPTIONS MENU");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        //SceneManager.LoadSceneAsync("Options");
    }
    public void QuitGame()
    {
        Debug.Log("QUIT GAME!");
        Application.Quit();
    }
    //public AudioSource audio;
    //public void PlayButtonAudio()
    //{
    //    audio.Play();
    //}
}
