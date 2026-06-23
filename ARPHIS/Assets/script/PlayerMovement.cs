using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    public Joystick joystick;
    public Transform camTransform;
    public Animator anim;

    [Header("Camera Pivot Setup (For OTS Aiming)")]
    public Transform cameraPivotTransform;
    public Vector3 normalCameraLocalOffset = new Vector3(0f, 0f, 0f);
    public Vector3 pistolAimCameraOffset = new Vector3(0.75f, 0.2f, -0.5f);

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

    [Header("Combat Settings")]
    public float attackRange = 1f;
    public int attackDamage = 10;
    public LayerMask enemyLayer;

    private float maxStamina;
    private float currentStamina;
    private bool isRunning = false;
    private bool isExhausted = false;
    private bool isRunModeActive = false; // NEW: Tracks if run button is toggled ON
    private Vector3 velocity;
    private float actualRunTime = 0f;
    private float staminaDrainRate;
    private float rechargeRate;

    private float attackLockTimer = 0f;
    private bool isAttacking = false;
    private Coroutine weaponJumpWatcherCoroutine;

    private string currentPlayingLocomotionState = "";

    void Start()
    {
        Debug.Log("PLAYER SPAWNED AT: " + transform.position);
        maxStamina = runTimeInSeconds;
        staminaDrainRate = 1f;
        rechargeRate = maxStamina / rechargeTimeInSeconds;
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        if (cameraPivotTransform != null && normalCameraLocalOffset == Vector3.zero)
        {
            normalCameraLocalOffset = cameraPivotTransform.localPosition;
        }
    }

    void Update()
    {
        if (joystick == null || controller == null || camTransform == null || anim == null)
            return;

        if (isAttacking)
        {
            attackLockTimer -= Time.deltaTime;
            if (attackLockTimer <= 0f)
            {
                isAttacking = false;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
                currentPlayingLocomotionState = "";
            }
        }

        WeaponManager wpManager = GetComponent<WeaponManager>();
        if (wpManager == null) wpManager = FindObjectOfType<WeaponManager>();

        if (wpManager != null && wpManager.IsTransitioningWeapon())
        {
            if (!isAttacking)
            {
                controller.Move(Vector3.zero * Time.deltaTime);
            }
            currentPlayingLocomotionState = "";
            ApplyGravityPhysicsOnly();
            return;
        }

        // ========== DYNAMIC CAMERA POSITION & PLAYER ROTATION SYNC ==========
        if (wpManager != null && wpManager.currentWeapon == WeaponManager.WeaponType.Pistol)
        {
            if (cameraPivotTransform != null)
            {
                cameraPivotTransform.localPosition = Vector3.Lerp(cameraPivotTransform.localPosition, pistolAimCameraOffset, Time.deltaTime * 5f);
            }

            Vector3 cameraForwardDirection = camTransform.forward;
            cameraForwardDirection.y = 0f;
            if (cameraForwardDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetCamRotation = Quaternion.LookRotation(cameraForwardDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetCamRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            if (cameraPivotTransform != null)
            {
                cameraPivotTransform.localPosition = Vector3.Lerp(cameraPivotTransform.localPosition, normalCameraLocalOffset, Time.deltaTime * 5f);
            }
        }

        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        bool isJoystickMoving = (Mathf.Abs(x) > 0.05f || Mathf.Abs(z) > 0.05f);

        // ========== RUN LOGIC: Only run if BOTH joystick is moving AND run mode is active ==========
        bool shouldRun = isRunModeActive && isJoystickMoving && !isExhausted;

        // ========== STAMINA MANAGEMENT ==========
        if (shouldRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                isRunModeActive = false; // Turn off run mode when exhausted
                Debug.Log("⚠️ Stamina exhausted! Run mode cancelled.");
            }
        }
        else if (!isRunModeActive && currentStamina < maxStamina)
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
        float currentSpeed = shouldRun ? runSpeed : walkSpeed;

        // ========== MOVEMENT EXECUTION ==========
        if (isJoystickMoving && !isAttacking)
        {
            bool runningState = (currentSpeed == runSpeed);

            if (wpManager != null && wpManager.currentWeapon == WeaponManager.WeaponType.Pistol)
            {
                string targetState = runningState ? "Pistol Run" : "Pistol Walk";
                if (currentPlayingLocomotionState != targetState)
                {
                    currentPlayingLocomotionState = targetState;
                    anim.CrossFadeInFixedTime(targetState, 0.15f, 0, 0f);
                }
            }
            else
            {
                if (currentPlayingLocomotionState != (runningState ? "Run" : "Walk"))
                {
                    anim.SetBool("isWalking", !runningState);
                    anim.SetBool("isRunning", runningState);
                    currentPlayingLocomotionState = runningState ? "Run" : "Walk";
                }

                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * rotationSpeed);
                }
            }
        }
        else if (!isAttacking)
        {
            if (wpManager != null && wpManager.currentWeapon == WeaponManager.WeaponType.Pistol)
            {
                if (currentPlayingLocomotionState != "Pistol Idle")
                {
                    currentPlayingLocomotionState = "Pistol Idle";
                    anim.CrossFadeInFixedTime("Pistol Idle", 0.15f, 0, 0f);
                }
            }
            else
            {
                if (currentPlayingLocomotionState != "Idle")
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isRunning", false);
                    currentPlayingLocomotionState = "Idle";
                }
            }
        }

        if (!isAttacking)
        {
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        if (anim != null)
        {
            anim.transform.localPosition = new Vector3(0f, anim.transform.localPosition.y, 0f);
        }

        ApplyGravityPhysicsOnly();
    }

    private void ApplyGravityPhysicsOnly()
    {
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

    // ========== COMBAT METHODS ==========
    public void PerformPunching() { if (anim != null && !isAttacking) { StartCombatLock(1.8f); anim.SetTrigger("PunchingTrigger"); } }
    public void PerformKick() { if (anim != null && !isAttacking) { StartCombatLock(1.5f); anim.SetTrigger("KickTrigger"); } }
    public void PerformComboPunch() { if (anim != null && !isAttacking) { StartCombatLock(1.8f); anim.SetTrigger("ComboPunch"); } }
    public void PerformUpperCut() { if (anim != null && !isAttacking) { StartCombatLock(1.8f); anim.SetTrigger("UppercutTrigger"); } }
    public void PerformSideKick() { if (anim != null && !isAttacking) { StartCombatLock(1.5f); anim.SetTrigger("SideKickTrigger"); } }

    private void StartCombatLock(float durationSeconds)
    {
        isAttacking = true;
        attackLockTimer = durationSeconds;
        anim.SetBool("isWalking", false);
        anim.SetBool("isRunning", false);
        currentPlayingLocomotionState = "";
    }

    public void DealMeleeDamage()
    {
        Vector3 hitCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRange, enemyLayer);
        System.Collections.Generic.List<CharacterHealth> alreadyHit = new System.Collections.Generic.List<CharacterHealth>();

        foreach (Collider enemy in hitEnemies)
        {
            CharacterHealth enemyHealth = enemy.GetComponent<CharacterHealth>();
            if (enemyHealth != null && !alreadyHit.Contains(enemyHealth))
            {
                enemyHealth.TakeDamage(attackDamage);
                alreadyHit.Add(enemyHealth);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 hitCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRange);
    }

    // ==========================================
    // 🔥 RUN TOGGLE - TURNS RUN MODE ON/OFF
    // ==========================================
    public void StartRunning() 
    { 
        // Toggle run mode ON/OFF
        if (!isExhausted && currentStamina > 0.5f && !isAttacking)
        {
            isRunModeActive = !isRunModeActive; // Toggle the mode
            
            // Update animator
            if (anim != null)
            {
                // Only show running animation if actually running (joystick moving)
                if (isRunModeActive)
                {
                    Debug.Log($"🏃 Run mode ACTIVATED! Press again to cancel.");
                }
                else
                {
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isWalking", false);
                    Debug.Log($"🚶 Run mode CANCELLED! Stamina saved.");
                }
            }
        }
        else if (isExhausted)
        {
            Debug.Log("❌ Cannot run - Stamina exhausted! Waiting for recharge...");
            isRunModeActive = false;
            if (anim != null) anim.SetBool("isRunning", false);
        }
        else if (currentStamina <= 0.5f)
        {
            Debug.Log($"❌ Cannot run - Stamina too low! ({currentStamina:F1}/{maxStamina})");
            isRunModeActive = false;
            if (anim != null) anim.SetBool("isRunning", false);
        }
    }

    public void StopRunning() 
    { 
        // Called on Pointer Up (button release)
        // For TOGGLE mode, we do nothing here.
    }

    public void Jump()
    {
        if (controller.isGrounded && !isAttacking)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            WeaponManager wpManager = GetComponent<WeaponManager>();
            if (wpManager == null) wpManager = FindObjectOfType<WeaponManager>();

            if (wpManager != null)
            {
                if (wpManager.currentWeapon == WeaponManager.WeaponType.Pistol)
                {
                    anim.CrossFadeInFixedTime("Pistol Jump", 0.1f, 0, 0f);
                    currentPlayingLocomotionState = "Pistol Jump";
                    if (weaponJumpWatcherCoroutine != null) StopCoroutine(weaponJumpWatcherCoroutine);
                    weaponJumpWatcherCoroutine = StartCoroutine(WatchWeaponLandingRoutine(wpManager));
                }
                else if (wpManager.currentWeapon == WeaponManager.WeaponType.Knife)
                {
                    anim.CrossFadeInFixedTime("Knife Jump", 0.1f, 0, 0f);
                    currentPlayingLocomotionState = "Knife Jump";
                    if (weaponJumpWatcherCoroutine != null) StopCoroutine(weaponJumpWatcherCoroutine);
                    weaponJumpWatcherCoroutine = StartCoroutine(WatchWeaponLandingRoutine(wpManager));
                }
                else
                {
                    anim.SetTrigger("jumpTrigger");
                    currentPlayingLocomotionState = "Jump";
                }
            }
        }
    }

    private IEnumerator WatchWeaponLandingRoutine(WeaponManager wpManager)
    {
        yield return new WaitForSeconds(0.15f);

        while (!controller.isGrounded)
        {
            yield return null;
        }

        if (wpManager != null)
        {
            float x = joystick != null ? joystick.Horizontal : 0f;
            float z = joystick != null ? joystick.Vertical : 0f;
            bool isMoving = (Mathf.Abs(x) > 0.05f || Mathf.Abs(z) > 0.05f);

            string statePrefix = (wpManager.currentWeapon == WeaponManager.WeaponType.Pistol) ? "Pistol" : "Knife";

            if (isMoving)
            {
                bool runningState = (isRunModeActive && !isExhausted);
                string targetState = runningState ? statePrefix + " Run" : statePrefix + " Walk";
                currentPlayingLocomotionState = targetState;
                anim.CrossFadeInFixedTime(targetState, 0.15f, 0, 0f);
            }
            else
            {
                currentPlayingLocomotionState = statePrefix + " Idle";
                anim.CrossFadeInFixedTime(statePrefix + " Idle", 0.15f, 0, 0f);
            }
        }
        weaponJumpWatcherCoroutine = null;
    }

    public bool IsRunning() { return isRunModeActive && !isExhausted; }
    public float GetStaminaPercent() { return currentStamina / maxStamina; }
}