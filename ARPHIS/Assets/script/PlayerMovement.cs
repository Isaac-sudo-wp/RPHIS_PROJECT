using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Skill Cooldowns")]
    public float skill1Cooldown = 10f;  // Punch
    public float skill2Cooldown = 20f;  // Kick
    public float skill3Cooldown = 30f;  // UpperCut

    private float maxStamina;
    private float currentStamina;
    private bool isRunning = false;
    private bool isExhausted = false;
    private bool isRunModeActive = false;
    private Vector3 velocity;
    private float actualRunTime = 0f;
    private float staminaDrainRate;
    private float rechargeRate;

    private float attackLockTimer = 0f;
    private bool isAttacking = false;
    private Coroutine weaponJumpWatcherCoroutine;

    private string currentPlayingLocomotionState = "";

    // ==========================================
    // 🔥 SKILL COOLDOWN VARIABLES
    // ==========================================
    private float skill1Timer = 0f;
    private float skill2Timer = 0f;
    private float skill3Timer = 0f;
    private bool skill1OnCooldown = false;
    private bool skill2OnCooldown = false;
    private bool skill3OnCooldown = false;

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

        // ========== UPDATE SKILL COOLDOWNS ==========
        UpdateSkillCooldowns();

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

        // ========== RUN LOGIC ==========
        bool shouldRun = isRunModeActive && isJoystickMoving && !isExhausted;

        // ========== STAMINA MANAGEMENT ==========
        if (shouldRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                isRunModeActive = false;
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

    // ==========================================
    // 🔥 SKILL COOLDOWN METHODS
    // ==========================================
    void UpdateSkillCooldowns()
    {
        if (skill1OnCooldown)
        {
            skill1Timer -= Time.deltaTime;
            if (skill1Timer <= 0)
            {
                skill1OnCooldown = false;
                skill1Timer = 0;
                Debug.Log("✅ Skill 1 (Punch) ready!");
            }
        }
        if (skill2OnCooldown)
        {
            skill2Timer -= Time.deltaTime;
            if (skill2Timer <= 0)
            {
                skill2OnCooldown = false;
                skill2Timer = 0;
                Debug.Log("✅ Skill 2 (Kick) ready!");
            }
        }
        if (skill3OnCooldown)
        {
            skill3Timer -= Time.deltaTime;
            if (skill3Timer <= 0)
            {
                skill3OnCooldown = false;
                skill3Timer = 0;
                Debug.Log("✅ Skill 3 (UpperCut) ready!");
            }
        }
    }

    public bool IsSkillReady(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return !skill1OnCooldown && !isAttacking;
            case 2: return !skill2OnCooldown && !isAttacking;
            case 3: return !skill3OnCooldown && !isAttacking;
            default: return false;
        }
    }

    public void UseSkill(int skillIndex)
    {
        if (!IsSkillReady(skillIndex)) 
        {
            Debug.Log($"⏳ Skill {skillIndex} on cooldown!");
            return;
        }
        
        switch (skillIndex)
        {
            case 1:
                skill1OnCooldown = true;
                skill1Timer = skill1Cooldown;
                PerformPunching();
                Debug.Log($"👊 Punch used! Cooldown: {skill1Cooldown}s");
                break;
            case 2:
                skill2OnCooldown = true;
                skill2Timer = skill2Cooldown;
                PerformKick();
                Debug.Log($"🦵 Kick used! Cooldown: {skill2Cooldown}s");
                break;
            case 3:
                skill3OnCooldown = true;
                skill3Timer = skill3Cooldown;
                PerformUpperCut();
                Debug.Log($"💪 UpperCut used! Cooldown: {skill3Cooldown}s");
                break;
        }
    }

    // ==========================================
    // 🔥 SKILL BUTTON WRAPPER METHODS
    // ==========================================
    public void UseSkill1() { UseSkill(1); }
    public void UseSkill2() { UseSkill(2); }
    public void UseSkill3() { UseSkill(3); }

    public float GetSkillCooldownProgress(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return skill1Timer / skill1Cooldown;
            case 2: return skill2Timer / skill2Cooldown;
            case 3: return skill3Timer / skill3Cooldown;
            default: return 0;
        }
    }

    public float GetSkillRemainingTime(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return skill1Timer;
            case 2: return skill2Timer;
            case 3: return skill3Timer;
            default: return 0;
        }
    }

    // ========== COMBAT METHODS ==========
    public void PerformPunching() 
    { 
        if (anim != null && !isAttacking) 
        { 
            StartCombatLock(1.8f); 
            anim.SetTrigger("PunchingTrigger"); 
        } 
    }
    
    public void PerformKick() 
    { 
        if (anim != null && !isAttacking) 
        { 
            StartCombatLock(1.5f); 
            anim.SetTrigger("KickTrigger"); 
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
        currentPlayingLocomotionState = "";
    }

    public void DealMeleeDamage()
    {
        Vector3 hitCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRange, enemyLayer);
        List<CharacterHealth> alreadyHit = new List<CharacterHealth>();

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
    // 🔥 RUN TOGGLE
    // ==========================================
    public void StartRunning() 
    { 
        if (!isExhausted && currentStamina > 0.5f && !isAttacking)
        {
            isRunModeActive = !isRunModeActive;
            
            if (anim != null)
            {
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
        // Toggle mode - do nothing on release
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