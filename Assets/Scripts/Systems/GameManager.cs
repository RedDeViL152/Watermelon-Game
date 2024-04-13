using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : SingletonMB<GameManager>
{
    #region Properties/Settings

    [FoldoutGroup("References"), SerializeField] Player m_player;
    [FoldoutGroup("References"), SerializeField] GameObject m_hud;
    [FoldoutGroup("References"), SerializeField] GameObject m_pauseMenu;
    [FoldoutGroup("References"), SerializeField] GameObject m_gameoverCanvas;
    #endregion

    #region Assessor/ Mutators
    public Player Player => m_player;
    public GameObject HUD => m_hud;
    public GameObject PauseMenu => m_pauseMenu;
    public GameObject GameOverCanvas => m_gameoverCanvas;
    #endregion

    #region Methods
    public static void StartNewGame()
    {
        
    }

    public static void EndGame()
    {

    }

    public static void SaveGame()
    {

    }

    public static void LoadGame()
    {

    }
    public static void PauseGame()
    {

    }

    public static void UnpauseGame()
    {

    }
    public static void DisableHUD()
    {
        
    }
    public static void EnableHUD() 
    {
        
    }
    #endregion

}
