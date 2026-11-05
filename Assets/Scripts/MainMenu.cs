using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    { // loads scene in index 1 (the level select screen)
        SceneManager.LoadSceneAsync(1);
    }
    public void EndGame()
    { 
        Application.Quit(); // closes the game
    }
}
