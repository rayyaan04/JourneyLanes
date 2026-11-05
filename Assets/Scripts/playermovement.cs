using UnityEngine;
using TMPro;

public class playermovement : MonoBehaviour
{
    public Rigidbody rb;
    public float acceleration = 425f; 
    public float turnSpeed = 95f;
    public float maxSpeed = 15f; // max speed in units per second
    public float speed;
    Vector3 lastPosition = Vector3.zero;
    public TextMeshProUGUI speedometer;

    void FixedUpdate()
    {
        speed = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;
        speedometer.text = "Speed: " + speed * 50;
        float moveInput = 0f;
        float turnInput = 0f;

       // float speed = rb.linearVelocity.magnitude;
        // Debug.Log("Current Speed: " + speed);

        if (Input.GetKey("w"))
        { // starts forward movement when w is pressed
            moveInput = 1f;
        }
        else if (Input.GetKey("s"))
        { // starts backward movement when s is pressed
            moveInput = -0.8f;
        }

        
        if (Input.GetKey("a"))
        { // starts the turning to the left when a is pressed
            turnInput = -0.7f;
        }
        else if (Input.GetKey("d"))
        { // starts the turning to the right when d is pressed
            turnInput = 0.7f;
        }

        LimitSpeed();

        // calculation to add force when move input goes above 0 based on the forward vector
        rb.AddForce(transform.forward * moveInput * acceleration * Time.deltaTime);
        
        if (moveInput != 0) // Checks if the car is actually moving (prevents car from rotating on the spot)
        { // multiplies the turn speed by the vector which is applied to the rotate function in Transform
            transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
        }
        if (rb.position.y < -1.5f)
        { // Calls EndGame() function if the y position of car is below -1.5
            FindFirstObjectByType<GameManager>().EndGame();
        }

        void LimitSpeed()
        { // initialise the carVelocity variable
            Vector3 carVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
            
            
            if (carVelocity.magnitude > maxSpeed)
            { // normalise the Vector3 velocity so it can be multiplied by a float
                rb.linearVelocity = carVelocity.normalized * maxSpeed;
                
            }
            
        }
    }
}