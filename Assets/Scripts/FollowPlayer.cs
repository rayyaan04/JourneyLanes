using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // references the transform module for player
    public Vector3 offset; // creates a vector that modifies the position of the camera so it is slightly 
                           // behind the player, otherwise it would just be inside

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset; // updates the cameras position to be the players position + the offset
    }

    // more "fluid" cmaera movemnt script
    //public GameObject player;
    //public GameObject target;
    //public float speed = 2f;

    //private void FixedUpdate()
    //{
    //    this.transform.LookAt(player.transform);
    //    float carMove = Mathf.Abs(Vector3.Distance(this.transform.position, target.transform.position) * speed);
    //    this.transform.position = Vector3.MoveTowards(this.transform.position, target.transform.position, carMove * Time.deltaTime);
//}
}
