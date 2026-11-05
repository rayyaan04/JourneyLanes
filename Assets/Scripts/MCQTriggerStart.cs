using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MCQTriggerStart : MonoBehaviour
{
    public GameObject multipleChoiceQuestionUI;
    public TMP_Text resultMessage;
    public Button buttonA, buttonB, buttonC, buttonD;


    private void Start()
    {
        multipleChoiceQuestionUI.SetActive(false); // hides the MCQ panel on start
        resultMessage.gameObject.SetActive(false);// hides the resultm essage on start

        // attaches the SelectAnswer() function to the OnClick() event to each button
        // this sends the button click input as selectedAnswer in the function
        buttonA.onClick.AddListener(() => SelectAnswer(buttonA));
        buttonB.onClick.AddListener(() => SelectAnswer(buttonB));
        buttonC.onClick.AddListener(() => SelectAnswer(buttonC));
        buttonD.onClick.AddListener(() => SelectAnswer(buttonD));
    }
    private void OnTriggerEnter(Collider carCollider)
    {
        if (carCollider.CompareTag("Player")) // makes sure only the car can activate this
        {
            multipleChoiceQuestionUI.SetActive(true); // makes the MCQ panel active
            Time.timeScale = 0f; // pauses the game
        }
    }
    void SelectAnswer(Button selectedAnswer)
    {
        if (selectedAnswer == buttonB)
        {
            resultMessage.text = "Correct answer.";
            FindFirstObjectByType<PointsManager>().IncreasePoints(100, "Correct MCQ answer");
            resultMessage.gameObject.SetActive(true);
            StartCoroutine(ResumeGameCoroutine());

        }
        else if (selectedAnswer != buttonB)
        {
            resultMessage.text = "Incorrect. The correct answer is No Stopping.";
            resultMessage.gameObject.SetActive(true);
            StartCoroutine(ResumeGameCoroutine());
        }

    }
    IEnumerator ResumeGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(2f);  // now unpauses after a delay even when game timescale is 0.
        multipleChoiceQuestionUI.SetActive(false);
        Time.timeScale = 1;
    }
}