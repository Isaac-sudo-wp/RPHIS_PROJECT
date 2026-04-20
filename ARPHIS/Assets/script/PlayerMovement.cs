using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Joystick joystick; 
    public Transform camTransform; 
    public Animator anim; // Added for your animations

    public float speed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    void Update()
    {
        // Added anim to the null check so the console stays clean
        if (joystick == null || controller == null || camTransform == null || anim == null) return;

        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0; right.y = 0;

        Vector3 moveDirection = forward.normalized * z + right.normalized * x;

        if (moveDirection.magnitude >= 0.1f)
        {
            // ADDED: This triggers your Walking animation state
            anim.SetBool("isWalking", true);

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * rotationSpeed);
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else 
        {
            // ADDED: This returns your character to the Idle state
            anim.SetBool("isWalking", false);
        }

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}