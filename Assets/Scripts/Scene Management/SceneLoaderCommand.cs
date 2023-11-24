using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderCommand : MonoBehaviour
{
    public void Initialization() => SceneLoader.GetInstance.SceneInitialize(); // DO NOT USE UNLESS YOU ARE INITIALIZING EVERY SINGLETON
    public void PlayGame() => SceneLoader.GetInstance.StartGame();
    public  void MainMenu() => SceneLoader.GetInstance.OpenMainMenu(); 
}
