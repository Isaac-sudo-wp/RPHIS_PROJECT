using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Joystick joystick; 
    public Transform camTransform; 
    public Animator anim; 

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    [Header("Jump & Run Settings")]
    public float jumpHeight = 1.5f; // Adjust this to jump higher
    private bool isRunning = false;
    private bool isCooldown = false;
    private float runTimer = 15f;
    private float cooldownTimer = 0f;

    private Vector3 velocity;

    void Update()
    {
        if (joystick == null || controller == null || camTransform == null || anim == null) return;

        // Cooldown & Run Timer Logic (Same as before)
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) isCooldown = false;
        }

        if (isRunning)
        {
            runTimer -= Time.deltaTime;
            if (runTimer <= 0) StopRunning();
        }
        else if (!isCooldown && runTimer < 15f)
        {
            runTimer += Time.deltaTime;
        }

        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0; right.y = 0;

        Vector3 moveDirection = forward.normalized * z + right.normalized * x;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (moveDirection.magnitude >= 0.1f)
        {
            anim.SetBool("isWalking", !isRunning);
            anim.SetBool("isRunning", isRunning);

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * rotationSpeed);
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }

        // Apply Gravity
        if (controller.isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f; // Stick to the ground
            anim.SetBool("isGrounded", true);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Call this from btnJump
    public void Jump()
    {
        if (controller.isGrounded)
        {
            // Physics formula for jump height: v = sqrt(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("jumpTrigger");
        }
    }

    public void StartRunning() { if (!isCooldown && runTimer > 0) isRunning = true; }
    public void StopRunning() { isRunning = false; if (runTimer <= 0) { isCooldown = true; cooldownTimer = 30f; runTimer = 15f; } }
}