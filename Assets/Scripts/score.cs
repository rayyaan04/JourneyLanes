using UnityEngine;
using TMPro;

public class PointsManager : MonoBehaviour
{
    public int points = 0;
    public TextMeshProUGUI pointsText; // calls the TMP gui and makes a variable called scoreText
    public TextMeshProUGUI pointsExplanationMessage;
    // used to display text shows how the user gained/lost points

    public void Start()
    { // sets the text on screen to Points = 0 initially
        pointsText.text = "Points = " + points.ToString();
        pointsExplanationMessage.text = ""; // makes message empty initially
    }

    public void IncreasePoints(int amountOfPoints, string message)
    { // adds points based on how much was given
        points = points + amountOfPoints;
        pointsText.text = "Points = " + points.ToString();
        pointsExplanationMessage.text = "+" + amountOfPoints.ToString() + " Points: " + message;
    }

    public void DecreasePoints(int amountOfPoints, string message)
    { // subtracts points based on how much was given
        points = points - amountOfPoints;
        pointsText.text = "Points = " + points.ToString();
        pointsExplanationMessage.text = "-" + amountOfPoints.ToString() + " Points: " + message;
    }
} 

    
