using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector3 moveVector = Vector3.zero;
    CharacterController characterController;

    public float moveSpeed = 5f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        // Move relative to where the player is facing
        Vector3 move = transform.right * x + transform.forward * z;

        // Preserve vertical velocity for jumping/gravity
        moveVector.x = move.x * moveSpeed;
        moveVector.z = move.z * moveSpeed;

        if (characterController.isGrounded && moveVector.y < 0)
        {
            moveVector.y = -2f;
        }

        if (characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            moveVector.y = jumpSpeed;
        }

        moveVector.y -= gravity * Time.deltaTime;

        characterController.Move(moveVector * Time.deltaTime);
    }
}