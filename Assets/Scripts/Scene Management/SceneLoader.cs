using EasyUnityInternals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMB<SceneLoader>
{
    public static SceneLoader GetInstance => GetOrCreateInstance();
    public const string MAIN_MENU_SCENE = "Main";
    public const string GAME_PLAY_SCENE = "Game";


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GAME_PLAY_SCENE);
    }
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }
}
