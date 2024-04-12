using System;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMB<SceneLoader>
{
    public static SceneLoader GetInstance => GetOrCreateInstance();
    public static LoadSceneMode LoadSceneMode = LoadSceneMode.Additive;
    public static bool Unpause;

    public static Action onSceneLoad;
    public const string MAIN_MENU_SCENE = "Main";
    public const string GAME_PLAY_SCENE = "Game";


    #region STATIC METHODS
    public static void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public static void LoadScene(string sceneName, LoadSceneMode mode) => SceneManager.LoadScene(sceneName, mode);
    public static void LoadSceneAsync(string sceneName, LoadSceneMode mode) => SceneManager.LoadSceneAsync(sceneName, mode);
    public static void UnloadSceneAsync(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);
    public static void UnloadSceneAsync(Scene scene) => SceneManager.UnloadSceneAsync(scene);
    #endregion

    protected override void Awake()
    {
        if (transform.IsChildOf("SceneEssential"))
        {
            Log.QuickDebug($"SceneFader.Awake(): Scene: {SceneManager.GetActiveScene().name}");
            Instance = this;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
        }
        else
        {
            Log.Error($"SceneFader initialized from outside SceneEssential. FullName: {gameObject.GetFullName()}");
            Destroy(this);
        }
    }

    public void SceneInitialize()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
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
