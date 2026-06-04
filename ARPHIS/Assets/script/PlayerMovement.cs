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
    public float rotationSpeed = 10f; 
    public float gravity = -9.81f;
    
    [Header("Jump Settings")]
    public float jumpHeight = 1.5f; 

    [Header("Stamina Settings")]
    public float runTimeInSeconds = 15f;        
    public float rechargeTimeInSeconds = 60f;   
    
    private float maxStamina;
    private float currentStamina; 
    private bool isRunning = false;
    private bool isExhausted = false; 
    private Vector3 velocity;
    private float actualRunTime = 0f;
    private float staminaDrainRate;
    private float rechargeRate;

    // SOLID COMBAT LOCK
    private float attackLockTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        maxStamina = runTimeInSeconds;                    
        staminaDrainRate = 1f;                            
        rechargeRate = maxStamina / rechargeTimeInSeconds; 
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        if (joystick == null || controller == null || camTransform == null || anim == null) 
            return;

        // Count down attack lock
        if (isAttacking)
        {
            attackLockTimer -= Time.deltaTime;
            if (attackLockTimer <= 0f)
            {
                isAttacking = false;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
            }
        }

        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        bool isJoystickMoving = (Mathf.Abs(x) > 0.05f || Mathf.Abs(z) > 0.05f);

        // ========== STAMINA MANAGEMENT ==========
        if (isRunning && isJoystickMoving && !isExhausted)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true; 
                isRunning = false;
            }
        }
        else if (!isRunning && currentStamina < maxStamina)
        {
            currentStamina += rechargeRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        if (staminaSlider != null) staminaSlider.value = currentStamina;

        // ========== MOVEMENT CALCULATION ==========
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDirection = (forward * z) + (right * x);
        float currentSpeed = (isRunning && !isExhausted && isJoystickMoving) ? runSpeed : walkSpeed;

        // ========== MOVEMENT EXECUTION ==========
        if (isJoystickMoving && !isAttacking)
        {
            bool runningState = (currentSpeed == runSpeed);
            anim.SetBool("isWalking", !runningState);
            anim.SetBool("isRunning", runningState);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * rotationSpeed);
            }
        }
        else if (!isAttacking)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
        }

        if (!isAttacking)
        {
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // ========== GRAVITY ==========
        if (controller.isGrounded)
        {
            anim.SetBool("isGrounded", true);
            if (velocity.y < 0) velocity.y = -2f;
        }
        else
        {
            anim.SetBool("isGrounded", false);
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < -20f) velocity.y = -20f;
        }
        
        controller.Move(velocity * Time.deltaTime);
    }

    // ========== COMBAT TRIGGER METHODS ==========

    public void PerformPunching()
    {
        if (anim != null && !isAttacking)
        {
            StartCombatLock(1.8f);
            anim.SetTrigger("PunchingTrigger"); 
            Debug.Log("Punch executed!");
        }
    }

    public void PerformKick()
    {
        if (anim != null && !isAttacking)
        {
            StartCombatLock(1.5f);
            anim.SetTrigger("KickTrigger");
            Debug.Log("Kick executed!");
        }
    }

    public void PerformComboPunch()
    {
        if (anim != null && !isAttacking)
        {
            StartCombatLock(1.8f); 
            anim.SetTrigger("ComboPunch"); 
        }
    }

    public void PerformUpperCut()
    {
        if (anim != null && !isAttacking)
        {
            StartCombatLock(1.8f); 
            anim.SetTrigger("UppercutTrigger");
        }
    }

    public void PerformSideKick()
    {
        if (anim != null && !isAttacking)
        {
            StartCombatLock(1.5f);
            anim.SetTrigger("SideKickTrigger"); 
        }
    }

    private void StartCombatLock(float durationSeconds)
    {
        isAttacking = true;
        attackLockTimer = durationSeconds;
        
        anim.SetBool("isWalking", false);
        anim.SetBool("isRunning", false);
    }

    public void Jump()
    {
        if (controller.isGrounded && !isAttacking)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("jumpTrigger");
        }
    }

    public void StartRunning() 
    { 
        if (!isExhausted && currentStamina > 0.5f && !isAttacking) 
            isRunning = true; 
    }
    
    public void StopRunning() 
    { 
        if (isRunning) 
            isRunning = false; 
    }
    
    public bool IsRunning() { return isRunning && !isExhausted; }
    public float GetStaminaPercent() { return currentStamina / maxStamina; }
}