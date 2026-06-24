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
    [Tooltip("I-drag dito ang 'CameraPivot' object mo galing sa hierarchy.")]
    public Transform cameraPivotTransform;
    [Tooltip("Normal camera center offset kung walang baril.")]
    public Vector3 normalCameraLocalOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("Over-the-shoulder camera offset kapag naka-pistol mode.")]
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
    [Tooltip("How far forward the attack reaches.")]
    public float attackRange = 1f;
    [Tooltip("How much damage a melee attack does.")]
    public int attackDamage = 10;
    [Tooltip("Set this to your 'Enemy' layer so you only hit enemies!")]
    public LayerMask enemyLayer;

    private float maxStamina;
    private float currentStamina;
    private bool isRunning = false;
    private bool isExhausted = false;
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

        // Add this line to the very top of your Start function
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

        // I-save ang default na posisyon ng camera pivot kung hindi pa manually na-set
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
            // 1. Swabe at automatic na i-blend ang CameraPivot papunta sa Over-the-shoulder look (Pangalawang larawan)
            if (cameraPivotTransform != null)
            {
                cameraPivotTransform.localPosition = Vector3.Lerp(cameraPivotTransform.localPosition, pistolAimCameraOffset, Time.deltaTime * 5f);
            }

            // 2. ROTATION SYNC: Kusa at automatic na haharap si player kung saan nakatingin ang camera kapag iginalaw ito
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
            // Ibalik ang CameraPivot sa normal nitong gitnang posisyon kapag binitawan ang baril
            if (cameraPivotTransform != null)
            {
                cameraPivotTransform.localPosition = Vector3.Lerp(cameraPivotTransform.localPosition, normalCameraLocalOffset, Time.deltaTime * 5f);
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

        // ========== MOVEMENT EXECUTION & CODE-BASED CROSSFADE ==========
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

        // ANTI-LAG RE-ALIGNMENT
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

    // ========== THIS IS THE ANIMATION EVENT FUNCTION ==========
    public void DealMeleeDamage()
    {
        // 1. Calculate a point slightly in front of the player's chest
        Vector3 hitCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;

        // 2. Create an invisible sphere that finds anything on the Enemy Layer
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRange, enemyLayer);

        // 3. Keep track of who we already hit so we don't damage them twice!
        System.Collections.Generic.List<CharacterHealth> alreadyHit = new System.Collections.Generic.List<CharacterHealth>();

        // 4. Apply damage to everyone caught in the sphere
        foreach (Collider enemy in hitEnemies)
        {
            CharacterHealth enemyHealth = enemy.GetComponent<CharacterHealth>();

            // If they have health, AND we haven't damaged them yet this punch...
            if (enemyHealth != null && !alreadyHit.Contains(enemyHealth))
            {
                enemyHealth.TakeDamage(attackDamage);
                alreadyHit.Add(enemyHealth); // Add them to the list so they don't get double-hit
            }
        }
    }

    // This draws a red wireframe bubble in the Scene view so you can visually see the size of your punch!
    void OnDrawGizmosSelected()
    {
        Vector3 hitCenter = transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRange);
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
                bool runningState = (isRunning && !isExhausted);
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

    public void StartRunning() { if (!isExhausted && currentStamina > 0.5f && !isAttacking) isRunning = true; }
    public void StopRunning() { if (isRunning) isRunning = false; }
    public bool IsRunning() { return isRunning && !isExhausted; }
    public float GetStaminaPercent() { return currentStamina / maxStamina; }
}