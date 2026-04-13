using UnityEngine;

public class playermovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator anim;

    public float speed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    private Vector3 velocity;

    void Update()
    {
        // 1. Get WASD or Arrow Key Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x, 0, z).normalized;

        if (move.magnitude >= 0.1f)
        {
            // 2. Rotate to face movement direction
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            // 3. Move forward
            controller.Move(move * speed * Time.deltaTime);

            // 4. Update Animator parameter
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        // 5. Apply Gravity so you stay on the ground
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}