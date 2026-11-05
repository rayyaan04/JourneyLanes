using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
    // Used to change states in the game - start/stop game, restart game, display uis (game over, score), transition to other scenes
{
    bool gameHasEnded = false;
    public TMP_Text endingText;
    public GameObject completeLevelUI;

    public void CompleteLevel()
    { 
        completeLevelUI.SetActive(true); // Activates the level complete UI
    }
    public void EndGame ()
    {
        if (gameHasEnded == false) // means that the game end detection only happens one time, isntead of many times at once
                                   // as the detection is in the Update() function, which is checked every frame
        {
            gameHasEnded = true;
            endingText.text = "You crashed! Restarting level..";
            Invoke("Restart", 2f); // Calls restart function with a delay 
        }
    }
    void Restart()
    { // Restarts the current scene from the beginning
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
