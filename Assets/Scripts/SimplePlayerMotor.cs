using UnityEngine;

public class SimplePlayerMotor : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }
}
