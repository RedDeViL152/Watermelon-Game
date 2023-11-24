using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderCommand : MonoBehaviour
{
    public void PlayGame() => SceneLoader.GetInstance.StartGame();
    public  void MainMenu() => SceneLoader.GetInstance.OpenMainMenu();
}
