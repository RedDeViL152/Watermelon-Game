using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string GAME_PLAY_SCENE = "Game";


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GAME_PLAY_SCENE);
    }
    public void OpenSetting()
    {

    }
    public void OpenInfo()
    {

    }
}
