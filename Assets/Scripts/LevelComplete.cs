using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelComplete : MonoBehaviour
{
    public void LoadLevelSelector()
    { // loads the level selector (used at end of level)
        SceneManager.LoadSceneAsync(1);
    }
    public void LoadMainMenu()
    { // loads the main menu (only used in level select screen)
        SceneManager.LoadSceneAsync(0);
    }
}
