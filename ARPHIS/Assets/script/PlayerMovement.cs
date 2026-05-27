using UnityEngine;
using UnityEngine.UI; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    public Joystick joystick; 
    public Transform camTransform; 
    public Animator anim; 

    [Header("UI Elements")]
    public Slider staminaSlider; 

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 15f; // Increased slightly for snappier mobile turns
    public float gravity = -9.81f;
    
    [Header("Jump Settings")]
    public float jumpHeight = 1.5f; 

    [Header("Stamina Settings (Time in Seconds)")]
    [Tooltip("How many seconds you can run continuously")]
    public float runTimeInSeconds = 15f;        
    
    [Tooltip("How many seconds to fully recharge stamina from empty")]
    public float rechargeTimeInSeconds = 60f;   
    
    private float maxStamina;
    private float currentStamina; 
    private bool isRunning = false;
    private bool isExhausted = false; 
    private Vector3 velocity;
    private float actualRunTime = 0f;
    private float staminaDrainRate;
    private float rechargeRate;

    void Start()
    {
        // Calculate stamina values based on desired times
        maxStamina = runTimeInSeconds;                    
        staminaDrainRate = 1f;                            
        rechargeRate = maxStamina / rechargeTimeInSeconds; 
        
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        
        // Display current settings
        Debug.Log($"=== STAMINA SYSTEM ===");
        Debug.Log($"▶ Run Duration: {runTimeInSeconds} seconds");
        Debug.Log($"▶ Recharge Duration: {rechargeTimeInSeconds} seconds");
        Debug.Log($"▶ Total Stamina: {maxStamina}");
        Debug.Log($"▶ Drain Rate: {staminaDrainRate} stamina/sec");
        Debug.Log($"▶ Recharge Rate: {rechargeRate:F2} stamina/sec");
        Debug.Log($"=====================");
    }

    void Update()
    {
        // Check if all required components are assigned
        if (joystick == null || controller == null || camTransform == null || anim == null) 
            return;

        // Get joystick input
        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        
        // Optimized micro-magnitude threshold for immediate joystick responsiveness
        bool isJoystickMoving = (Mathf.Abs(x) > 0.05f || Mathf.Abs(z) > 0.05f);

        // ========== STAMINA MANAGEMENT ==========
        
        // Drain stamina when running
        if (isRunning && isJoystickMoving && !isExhausted)
        {
            actualRunTime += Time.deltaTime;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            
            // Debug every second of running
            if (Mathf.FloorToInt(actualRunTime) > Mathf.FloorToInt(actualRunTime - Time.deltaTime))
            {
                float remainingSeconds = currentStamina / staminaDrainRate;
                Debug.Log($"🏃 Running: {Mathf.FloorToInt(actualRunTime)} sec | Stamina: {currentStamina:F0}/{maxStamina} | Remaining: {remainingSeconds:F0} sec");
            }
            
            // Check if stamina is exhausted
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true; 
                isRunning = false;
                Debug.Log($"⚠️ STAMINA EXHAUSTED after {actualRunTime:F0} seconds! Need {rechargeTimeInSeconds} seconds to fully recharge.");
                actualRunTime = 0f;
            }
        }
        // Recharge stamina when not running
        else if (!isRunning && currentStamina < maxStamina)
        {
            currentStamina += rechargeRate * Time.deltaTime;
            
            // Debug when fully recharged
            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isExhausted = false;
                Debug.Log($"✅ STAMINA FULLY RECHARGED! Ready to run again.");
            }
        }
        else if (!isRunning)
        {
            // Reset run timer when not running
            actualRunTime = 0f;
        }

        // Clamp stamina value
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // Update stamina UI slider
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }

        // ========== MOVEMENT CALCULATION ==========
        
        // Get camera directions for movement
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate move direction
        Vector3 moveDirection = (forward * z) + (right * x);
        
        // Determine current speed (walk or run)
        float currentSpeed = (isRunning && !isExhausted && isJoystickMoving) ? runSpeed : walkSpeed;

        // ========== MOVEMENT EXECUTION ==========
        
        if (isJoystickMoving)
        {
            // Update animations instantly based on movement type
            bool runningState = (currentSpeed == runSpeed);
            anim.SetBool("isWalking", !runningState);
            anim.SetBool("isRunning", runningState);

            // Rotate player towards movement direction
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Idle - no movement
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }

        // Execute primary horizontal movement vectors continuously
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // ========== GRAVITY (FIXED TO PREVENT SKY-LAUNCH BUG) ==========
        
        if (controller.isGrounded)
        {
            anim.SetBool("isGrounded", true);
            
            // Forces a stable downforce so he snaps down curbs smoothly, 
            // but prevents gravity from compounding to infinity while touching meshes
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            anim.SetBool("isGrounded", false);
            
            // Accumulate descending velocity vector forces
            velocity.y += gravity * Time.deltaTime;
            
            // TERMINAL VELOCITY CAP: Clamps gravity buildup so collision math never clips or bounces
            if (velocity.y < -20f)
            {
                velocity.y = -20f;
            }
        }
        
        // Execute vertical movement calculation vector safely
        controller.Move(velocity * Time.deltaTime);
    }

    // ========== PUBLIC METHODS FOR BUTTONS ==========
    
    public void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("jumpTrigger");
            Debug.Log("🦘 Jump executed!");
        }
    }

    public void StartRunning()
    {
        if (!isExhausted && currentStamina > 0.5f)
        {
            isRunning = true;
            float remainingSeconds = currentStamina / staminaDrainRate;
            Debug.Log($"▶ RUN STARTED! Will last approximately {remainingSeconds:F0} more seconds");
        }
        else if (isExhausted)
        {
            Debug.Log($"❌ CANNOT RUN! Stamina exhausted. Need to wait {rechargeTimeInSeconds} seconds to fully recharge.");
        }
        else if (currentStamina <= 0.5f)
        {
            Debug.Log($"❌ CANNOT RUN! Stamina too low ({currentStamina:F0}/{maxStamina}). Let it recharge.");
        }
    }

    public void StopRunning()
    {
        if (isRunning)
        {
            isRunning = false;
            float remainingStamina = currentStamina;
            Debug.Log($"⏹️ RUN STOPPED. Stamina remaining: {remainingStamina:F0}/{maxStamina}");
        }
    }
    
    public bool IsRunning()
    {
        return isRunning && !isExhausted;
    }
    
    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
    
    public float GetRemainingRunTime()
    {
        return currentStamina / staminaDrainRate;
    }
}