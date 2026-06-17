using UnityEngine;
using UnityEngine.UI;
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
    [Tooltip("I-drag dito ang 'imgTarget' GameObject galing sa Canvas.")]
    public GameObject imgTargetUI; 

    [Header("Pistol Shooting Setup")]
    [Tooltip("I-drag dito ang 'BarrelTip' empty gameobject na nasa dulo ng nguso ng baril.")]
    public Transform pistolBarrelTip;
    [Tooltip("Gaano kalayo ang abot ng bala ng pistol.")]
    public float pistolRange = 50f;

    [Header("VFX Gunshot Effects")]
    [Tooltip("I-drag dito ang Prefab ng iyong Gunshot Effect o Muzzle Flash Particle.")]
    public GameObject muzzleFlashPrefab;
    [Tooltip("Gaano katagal bago burahin ang effect sa screen pagkatapos pumutok.")]
    public float effectDestroyDelay = 0.1f;

    [Header("SFX Gunshot Audio Setup")]
    [Tooltip("I-drag dito ang AudioSource component na nasa Capsule mo.")]
    public AudioSource weaponAudioSource;
    [Tooltip("I-drag dito ang na-download mong Gunshot Audio Clip asset.")]
    public AudioClip pistolShotSound;

    [Header("Combat Dashboard Layout Swapping")]
    public Image[] dashboardButtonImages;
    public Sprite[] knifeCombatSprites;
    public Sprite[] punchCombatSprites;
    public Sprite[] pistolCombatSprites; 

    [Header("Knife/Pistol Attack Button Routing")]
    public Button[] attackButtons;

    [Header("Player Components (Tripo Compatibility)")]
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

    [HideInInspector] public bool isHoldingKnifeStatus = false;
    [HideInInspector] public bool isHoldingPistolStatus = false; 

    private Coroutine currentTransitionCoroutine;
    private Coroutine attackSequenceCoroutine;
    private bool internalAttackLock = false; 
    private bool isWeaponTransitionActive = false; 
    private RectTransform targetRectTransform;

    private Sprite originalPunchSprite;

    void Start()
    {
        // 🔥 SAFE INITIALIZATION RESET: Code-based visibility override sa unang frame
        if (physicalKnife3D != null) physicalKnife3D.SetActive(false);
        if (physicalPistol3D != null) physicalPistol3D.SetActive(false);
        
        if (imgTargetUI != null) 
        {
            imgTargetUI.SetActive(false);
            targetRectTransform = imgTargetUI.GetComponent<RectTransform>();
        }
        
        if (playerAnimator != null) playerAnimator.applyRootMotion = false;

        if (imgPunchDisplayLocked())
        {
            originalPunchSprite = dashboardButtonImages[0].sprite; 
        }

        if (weaponAudioSource == null)
        {
            weaponAudioSource = GetComponent<AudioSource>();
        }
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
            {
                playerAnimator.speed = 1f;
            }
        }

        // SWABE AT AUTOMATIC NA CROSSHAIR MESH TRACKING
        if (currentWeapon == WeaponType.Pistol && imgTargetUI != null && imgTargetUI.activeSelf && pistolBarrelTip != null)
        {
            Vector3 targetWorldPosition = pistolBarrelTip.position + (pistolBarrelTip.forward * 10f);
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(targetWorldPosition);
            
            if (targetRectTransform != null && screenPoint.x > 0 && screenPoint.y > 0)
            {
                targetRectTransform.position = screenPoint;
            }
        }
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

        if (imgTargetUI != null)
        {
            imgTargetUI.SetActive(true);
        }

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
        float adjustedTotalDuration = 0.75f / drawAnimationSpeed;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isHoldingWeapon", false);
            playerAnimator.speed = 1f; 

            if (previousWeapon == WeaponType.Pistol)
            {
                playerAnimator.CrossFadeInFixedTime("Idle", 0.15f, 0, 0f);
            }
            else
            {
                playerAnimator.CrossFadeInFixedTime("SheathKnife", 0.15f, 0, 0f);
            }
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

        if (currentWeapon == WeaponType.Knife)
        {
            if (internalAttackLock) return; 
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

    // REAL-TIME SHOOTING ENGINE WITH EFFECT SCALE RESCUE
    private void ShootPistol()
    {
        if (pistolBarrelTip == null) return;

        // Playback audio clip execution
        if (weaponAudioSource != null && pistolShotSound != null)
        {
            weaponAudioSource.PlayOneShot(pistolShotSound);
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flashInstance = Instantiate(muzzleFlashPrefab, pistolBarrelTip.position, pistolBarrelTip.rotation);
            flashInstance.transform.parent = pistolBarrelTip;
            
            // 🔥 HARD SCALE RE-ALIGNMENT: Pinupwersa ang saktong sukat ng nguso para hindi sumabog na parang kanyon
            flashInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            Destroy(flashInstance, effectDestroyDelay);
        }

        RaycastHit hit;
        if (Physics.Raycast(pistolBarrelTip.position, pistolBarrelTip.forward, out hit, pistolRange))
        {
            Debug.Log("💥 Bala tumama sa: " + hit.collider.name);
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
            if (cc != null && cc.enabled && masterCapsule != null) cc.Move(masterCapsule.forward * launchForce * Time.deltaTime);
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
}