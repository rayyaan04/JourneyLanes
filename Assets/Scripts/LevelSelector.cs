using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void LoadLevelOne()
    {// loads scene found in index 2 - which is level01
        SceneManager.LoadSceneAsync(2);
    }
    public void LoadLevelTwo()
    { // loads scene found in index 3 - which is level02
        SceneManager.LoadSceneAsync(3);
    }

}
