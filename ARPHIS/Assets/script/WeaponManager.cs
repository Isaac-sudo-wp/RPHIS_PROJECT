using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponType { Fists, Knife, Pistol }
    [Header("Current Weapon State")]
    public WeaponType currentWeapon = WeaponType.Fists;

    [Header("UI Main Weapon Display")]
    public Image activeWeaponIconDisplay;
    public Sprite knifeSprite;
    public Sprite punchSprite;
    public Sprite pistolSprite;

    [Header("Dedicated UI Crosshair Target")]
    public GameObject imgTargetUI;

    [Header("Pistol Shooting Setup")]
    public Transform pistolBarrelTip;
    public float pistolRange = 50f;

    [Header("VFX Gunshot Effects")]
    public GameObject muzzleFlashPrefab;
    public float effectDestroyDelay = 0.1f;

    [Header("SFX Gunshot Audio Setup")]
    public AudioSource weaponAudioSource;
    public AudioClip pistolShotSound;

    [Header("Combat Dashboard Layout Swapping")]
    public Image[] dashboardButtonImages;
    public Sprite[] knifeCombatSprites;
    public Sprite[] punchCombatSprites;
    public Sprite[] pistolCombatSprites;

    [Header("Knife/Pistol Attack Button Routing")]
    public Button[] attackButtons;

    [Header("Player Components")]
    public Transform visualModelMesh;
    public Transform masterCapsule;
    public Animator playerAnimator;

    [Header("3D Weapon Mesh Toggles")]
    public GameObject physicalKnife3D;
    public GameObject physicalPistol3D;

    [Header("Animation Tuning")]
    public float drawAnimationSpeed = 1.5f;
    public float knifeShowDelay = 0.25f;
    public float knifeHideDelay = 0.35f;
    public float pistolShowDelay = 0.25f;

    [Header("Manual Forward Force Settings")]
    public float[] forwardForces = new float[] { 8f, 12f, 10f, 15f };

    [Header("Damage & Hitbox Settings")]
    public int punchDamage = 10;
    public int knifeDamage = 25;
    public int gunDamage = 40;
    public float meleeRange = 3f;
    public Transform meleeAttackPoint;
    public LayerMask enemyLayer;

    [Header("Skill Cooldown UI")]
    public Image[] skillCooldownImages;
    public TMP_Text[] skillCooldownTexts;

    [HideInInspector] public bool isHoldingKnifeStatus = false;
    [HideInInspector] public bool isHoldingPistolStatus = false;

    private Coroutine currentTransitionCoroutine;
    private Coroutine attackSequenceCoroutine;
    private bool internalAttackLock = false;
    private bool isWeaponTransitionActive = false;
    private RectTransform targetRectTransform;
    private Sprite originalPunchSprite;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);

        if (imgTargetUI != null)
        {
            imgTargetUI.SetActive(false);
            targetRectTransform = imgTargetUI.GetComponent<RectTransform>();
        }

        if (playerAnimator != null) playerAnimator.applyRootMotion = false;

        if (imgPunchDisplayLocked())
            originalPunchSprite = dashboardButtonImages[0].sprite;

        if (weaponAudioSource == null)
            weaponAudioSource = GetComponent<AudioSource>();

        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();

        UpdateSkillCooldownUI();
    }

    private bool imgPunchDisplayLocked()
    {
        return dashboardButtonImages != null && dashboardButtonImages.Length > 0 && dashboardButtonImages[0] != null;
    }

    void Update()
    {
        if (!internalAttackLock && currentTransitionCoroutine == null)
        {
            if (playerAnimator != null && playerAnimator.speed != 1f)
                playerAnimator.speed = 1f;
        }

        if (currentWeapon == WeaponType.Pistol && imgTargetUI != null && imgTargetUI.activeSelf && pistolBarrelTip != null)
        {
            Vector3 targetWorldPosition = pistolBarrelTip.position + (pistolBarrelTip.forward * 10f);
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(targetWorldPosition);

            if (targetRectTransform != null && screenPoint.x > 0 && screenPoint.y > 0)
                targetRectTransform.position = screenPoint;
        }

        UpdateSkillCooldownUI();
    }

    void OnEnable()
    {
        if (attackButtons != null && attackButtons.Length >= 4)
        {
            attackButtons[0].onClick.RemoveAllListeners();
            attackButtons[1].onClick.RemoveAllListeners();
            attackButtons[2].onClick.RemoveAllListeners();
            attackButtons[3].onClick.RemoveAllListeners();

            attackButtons[0].onClick.AddListener(() => OnAttackButtonPressed(1));
            attackButtons[1].onClick.AddListener(() => OnAttackButtonPressed(2));
            attackButtons[2].onClick.AddListener(() => OnAttackButtonPressed(3));
            attackButtons[3].onClick.AddListener(() => OnAttackButtonPressed(4));
        }
    }

    public bool IsTransitioningWeapon()
    {
        return isWeaponTransitionActive;
    }

    public void ToggleEquippedWeapon()
    {
        WeaponType previousWeapon = currentWeapon;

        int nextWeaponIndex = ((int)currentWeapon + 1) % 3;
        currentWeapon = (WeaponType)nextWeaponIndex;

        isHoldingKnifeStatus = (currentWeapon == WeaponType.Knife);
        isHoldingPistolStatus = (currentWeapon == WeaponType.Pistol);

        if (currentWeapon == WeaponType.Knife) EquipKnife();
        else if (currentWeapon == WeaponType.Pistol) EquipPistol();
        else EquipFists(previousWeapon);
    }

    private void EquipKnife()
    {
        if (activeWeaponIconDisplay != null && knifeSprite != null) activeWeaponIconDisplay.sprite = knifeSprite;
        SwapDashboardSprites(knifeCombatSprites);

        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);
        if (imgTargetUI != null) imgTargetUI.SetActive(false);

        if (playerAnimator != null)
        {
            if (currentTransitionCoroutine != null) StopCoroutine(currentTransitionCoroutine);
            currentTransitionCoroutine = StartCoroutine(ExecuteDrawKnifeSequence());
        }
    }

    IEnumerator ExecuteDrawKnifeSequence()
    {
        isWeaponTransitionActive = true;
        float adjustedTotalDuration = 0.75f / drawAnimationSpeed;
        float adjustedKnifeDelay = knifeShowDelay / drawAnimationSpeed;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingWeapon", false);
            playerAnimator.speed = drawAnimationSpeed;
            playerAnimator.CrossFadeInFixedTime("DrawKnife", 0.2f, 0, 0f);
        }

        yield return new WaitForSeconds(adjustedKnifeDelay);
        if (physicalKnife3D != null) physicalKnife3D.SetActive(true);

        float remainingTime = Mathf.Max(0f, adjustedTotalDuration - adjustedKnifeDelay);
        yield return new WaitForSeconds(remainingTime);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingWeapon", true);
            playerAnimator.speed = 1f;
            playerAnimator.CrossFadeInFixedTime("Knife Idle", 0.2f, 0, 0f);
        }
        isWeaponTransitionActive = false;
        currentTransitionCoroutine = null;
    }

    private void EquipPistol()
    {
        if (activeWeaponIconDisplay != null && pistolSprite != null) activeWeaponIconDisplay.sprite = pistolSprite;
        if (pistolCombatSprites != null && pistolCombatSprites.Length > 0) SwapDashboardSprites(pistolCombatSprites);

        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);

        if (playerAnimator != null)
        {
            if (currentTransitionCoroutine != null) StopCoroutine(currentTransitionCoroutine);
            currentTransitionCoroutine = StartCoroutine(ExecuteDrawPistolSequence());
        }
    }

    IEnumerator ExecuteDrawPistolSequence()
    {
        isWeaponTransitionActive = true;
        float adjustedTotalDuration = 0.75f / drawAnimationSpeed;
        float adjustedPistolDelay = pistolShowDelay / drawAnimationSpeed;

        if (playerAnimator != null)
        {
            playerAnimator.speed = drawAnimationSpeed;
            playerAnimator.CrossFadeInFixedTime("Drawing Pistol", 0.1f, 0, 0f);
        }

        yield return new WaitForSeconds(adjustedPistolDelay);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(true);

        float remainingTime = Mathf.Max(0f, adjustedTotalDuration - adjustedPistolDelay);
        yield return new WaitForSeconds(remainingTime);

        if (imgTargetUI != null) imgTargetUI.SetActive(true);

        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.CrossFadeInFixedTime("Pistol Idle", 0.2f, 0, 0f);
        }

        yield return new WaitForSeconds(0.1f);
        isWeaponTransitionActive = false;
        currentTransitionCoroutine = null;
    }

    private void EquipFists(WeaponType previousWeapon)
    {
        if (activeWeaponIconDisplay != null && punchSprite != null) activeWeaponIconDisplay.sprite = punchSprite;
        SwapDashboardSprites(punchCombatSprites);

        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);
        if (imgTargetUI != null) imgTargetUI.SetActive(false);

        if (playerAnimator != null)
        {
            if (currentTransitionCoroutine != null) StopCoroutine(currentTransitionCoroutine);
            currentTransitionCoroutine = StartCoroutine(ExecuteSheathSequence(previousWeapon));
        }
    }

    IEnumerator ExecuteSheathSequence(WeaponType previousWeapon)
    {
        isWeaponTransitionActive = true;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingWeapon", false);
            playerAnimator.speed = 1f;

            if (previousWeapon == WeaponType.Pistol)
                playerAnimator.CrossFadeInFixedTime("Idle", 0.15f, 0, 0f);
            else
                playerAnimator.CrossFadeInFixedTime("SheathKnife", 0.15f, 0, 0f);
        }

        yield return new WaitForSeconds(knifeHideDelay / drawAnimationSpeed);

        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);

        yield return new WaitForSeconds(0.15f);

        if (playerAnimator != null) playerAnimator.CrossFadeInFixedTime("Idle", 0.15f, 0, 0f);
        isWeaponTransitionActive = false;
        currentTransitionCoroutine = null;
    }

    private void SwapDashboardSprites(Sprite[] targetSpriteSet)
    {
        if (dashboardButtonImages == null || targetSpriteSet == null) return;
        int limit = Mathf.Min(dashboardButtonImages.Length, targetSpriteSet.Length);
        for (int i = 0; i < limit; i++)
        {
            if (dashboardButtonImages[i] != null && targetSpriteSet[i] != null)
                dashboardButtonImages[i].sprite = targetSpriteSet[i];
        }
    }

    private void OnAttackButtonPressed(int attackIndex)
    {
        if (currentWeapon == WeaponType.Pistol)
        {
            ShootPistol();
            return;
        }

        if (currentWeapon == WeaponType.Fists)
        {
            if (internalAttackLock) return;
            DealMeleeDamage(punchDamage);
        }

        if (currentWeapon == WeaponType.Knife)
        {
            if (internalAttackLock) return;
            DealMeleeDamage(knifeDamage);

            if (playerAnimator != null)
            {
                playerAnimator.speed = 1f;
                playerAnimator.ResetTrigger("ExecuteKnifeAttack");
                playerAnimator.SetInteger("KnifeAttackIndex", attackIndex);
                playerAnimator.SetTrigger("ExecuteKnifeAttack");

                if (attackSequenceCoroutine != null) StopCoroutine(attackSequenceCoroutine);
                attackSequenceCoroutine = StartCoroutine(ExecuteFixedLungeSequence(forwardForces[Mathf.Clamp(attackIndex - 1, 0, 3)]));
            }
        }
    }

    private void ShootPistol()
    {
        if (pistolBarrelTip == null) return;

        if (weaponAudioSource != null && pistolShotSound != null)
            weaponAudioSource.PlayOneShot(pistolShotSound);

        if (muzzleFlashPrefab != null)
        {
            GameObject flashInstance = Instantiate(muzzleFlashPrefab, pistolBarrelTip.position, pistolBarrelTip.rotation);
            flashInstance.transform.parent = pistolBarrelTip;
            flashInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Destroy(flashInstance, effectDestroyDelay);
        }

        RaycastHit hit;
        if (Physics.SphereCast(pistolBarrelTip.position, 0.5f, pistolBarrelTip.forward, out hit, pistolRange))
        {
            Debug.Log("💥 Bala tumama sa: " + hit.collider.name);
            CharacterHealth enemyHealth = hit.collider.GetComponent<CharacterHealth>();
            if (enemyHealth != null) enemyHealth.TakeDamage(gunDamage);
        }
    }

    IEnumerator ExecuteFixedLungeSequence(float launchForce)
    {
        internalAttackLock = true;
        CharacterController cc = masterCapsule != null ? masterCapsule.GetComponent<CharacterController>() : null;
        float elapsed = 0f;

        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            if (cc != null && cc.enabled && masterCapsule != null)
                cc.Move(masterCapsule.forward * launchForce * Time.deltaTime);
            yield return null;
        }

        if (playerAnimator != null) playerAnimator.speed = 0f;
        yield return new WaitForSeconds(0.15f);

        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.CrossFadeInFixedTime("Knife Idle", 0.15f, 0, 0f);
        }
        internalAttackLock = false;
    }

    private void DealMeleeDamage(int damageAmount)
    {
        if (meleeAttackPoint == null) return;

        Collider[] hitEnemies = Physics.OverlapSphere(meleeAttackPoint.position, meleeRange, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            CharacterHealth enemyHealth = enemy.GetComponent<CharacterHealth>();
            if (enemyHealth != null) enemyHealth.TakeDamage(damageAmount);
        }
    }

    public void UpdateSkillCooldownUI()
    {
        if (playerMovement == null) return;

        for (int i = 0; i < skillCooldownImages.Length && i < 3; i++)
        {
            if (skillCooldownImages[i] == null) continue;

            int skillIndex = i + 1;
            bool isReady = playerMovement.IsSkillReady(skillIndex);
            float progress = playerMovement.GetSkillCooldownProgress(skillIndex);
            float remaining = playerMovement.GetSkillRemainingTime(skillIndex);

            if (isReady)
            {
                skillCooldownImages[i].fillAmount = 0f;
                skillCooldownImages[i].color = new Color(0f, 0f, 0f, 0f);

                if (skillCooldownTexts != null && skillCooldownTexts.Length > i && skillCooldownTexts[i] != null)
                    skillCooldownTexts[i].text = "";
            }
            else
            {
                skillCooldownImages[i].fillAmount = 1f - progress;
                skillCooldownImages[i].color = new Color(0f, 0f, 0f, 0.6f);

                if (skillCooldownTexts != null && skillCooldownTexts.Length > i && skillCooldownTexts[i] != null)
                    skillCooldownTexts[i].text = Mathf.CeilToInt(remaining).ToString();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeRange);
        }
    }
}