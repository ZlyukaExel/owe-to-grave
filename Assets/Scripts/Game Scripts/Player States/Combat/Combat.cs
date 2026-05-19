using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DefaultState))]
[RequireComponent(typeof(Vector2Reference))]
public class CombatState : State
{
    [HideInInspector]
    public bool inCombat,
        isCurrentlyAiming,
        isShooting,
        isAimingOrShooting;
    public int currentSet;
    public float outOfCombatTime = 3;
    private bool shootButtonPressed,
        aimButtonPressed,
        aimButtonClicked;
    private float aimButtonPressedTime;
    public float aimingWeight = 0,
        aimingWeightSpeedModifier = 3;
    private float zAxisSaver;
    private Animator crosshairAnimator;
    private float notInCombatTime = 0;
    private Vector2Reference aimingVector;

    [SerializeField]
    private LayerMask combatLayers;

    [SerializeField]
    private GameObject bulletPrefab;
    private readonly SpeedBuff aimingSlowdown =
            new("Aiming slowdown", "Slows down to shoot properly", null, -1, -0.3f),
        shootingSlowdown = new("Shoting slowdown", "Slows down to aim properly", null, -1, -0.15f);

    [SerializeField]
    private InputActionReference attackAction,
        swapWeaponsAction;
    private InputManager input;
    public bool isMelee;

    public override void Awake()
    {
        base.Awake();
        GetComponent<DefaultState>().onStateEnter.AddListener(() => SetListeners(true));
        GetComponent<DefaultState>().onStateExit.AddListener(() => SetListeners(false));

        aimingVector = GetComponent<Vector2Reference>();
    }

    private void Start()
    {
        input = l.input;

        if (pLinks)
        {
            Transform combatUi = pLinks.ui.Find("Mobile Ui/Ground Ui");
            crosshairAnimator = pLinks.ui.Find("Crosshair").GetComponent<Animator>();

            l.input.GetAction(swapWeaponsAction.action).onUp.AddListener(l.inventory.SwapWeapons);
        }

        l.netConfig.OnConfigChanged.AddListener(OnConfigChanged);

        // canStandTrigger = l
        //     .transform.Find("Armature/Point Of Scaling/Can Stand Trigger")
        //     .GetComponent<Trigger>();
        //
        // crouchButtonObj
        //     .GetComponent<EventTrigger>()
        //     .AddPointerUpAndDownListeners(OnCrouchButtonDown, OnCrouchButtonUp);
    }

    public override void EnterState()
    {
        inCombat = true;

        if (pLinks)
        {
            if (!pLinks.cameraController.isFirstPerson)
                pLinks.cameraController.positionOffset.x = 0.3f;
        }

        SetListeners(true);
        input.GetAction(attackAction.action)?.onUp.AddListener(OnShootButtonUp);

        l.animator.SetBool("inCombat", true);

        CharacterConfig config = l.netConfig.syncConfig;
        config.inCombat = true;
        l.netConfig.RequestConfigChange(config);

        currentSet = 1;

        base.EnterState();
    }

    public override void UpdateState()
    {
        l.movement.MovementUpdate();
        l.carTrigger.CarTriggerUpdate();

        // Aiming ray checks there's no obstacle in the way
        Quaternion yRotation = Quaternion.Euler(0, aimingVector.value.y, 0);
        Vector3 origin = transform.position + (yRotation * new Vector3(0.2f, 1.5f, 0));

        Vector2 angles = new(aimingVector.value.x, aimingVector.value.y);
        Vector3 direction = Quaternion.Euler(angles.x, angles.y, 0) * Vector3.forward * 0.75f;

        Debug.DrawRay(origin, direction, Color.red);
        bool wallInFrontOf = Physics.Raycast(
            origin,
            direction,
            out RaycastHit _,
            0.75f,
            combatLayers
        );
        bool aimOrAttackPressed = shootButtonPressed || aimButtonClicked || aimButtonPressed;
        bool canAttackMelee = shootButtonPressed && isMelee;
        bool canShoot = aimOrAttackPressed && !wallInFrontOf;
        isAimingOrShooting = canAttackMelee || canShoot;

        float target = isAimingOrShooting ? 1f : 0f;
        aimingWeight = Mathf.MoveTowards(
            aimingWeight,
            target,
            aimingWeightSpeedModifier * Time.deltaTime
        );

        if (aimButtonPressed)
            aimButtonPressedTime += Time.deltaTime;

        if (aimOrAttackPressed)
        {
            notInCombatTime = 0;
        }
        else
        {
            // Exit combat after 5 seconds
            notInCombatTime += Time.deltaTime;
            if (notInCombatTime > outOfCombatTime)
            {
                l.stateManager.SetState(EnumState.Default);
            }
        }

        if (isAimingOrShooting)
        {
            AimingAndShotingUpdate();
        }
        else
        {
            StopAim();
            StopShooting();
        }
    }

    public override void FixedUpdateState()
    {
        l.movement.MovementFixedUpdate();
    }

    public override void ExitState()
    {
        if (pLinks)
        {
            if (!pLinks.interactableTrigger.IsHolding())
                pLinks.cameraController.positionOffset.x = 0;
        }

        l.animator.SetBool("inCombat", false);
        CharacterConfig config = l.netConfig.syncConfig;
        config.inCombat = false;
        l.netConfig.RequestConfigChange(config);
        l.primaryWeapon?.onShot.RemoveListener(SpawnBullet);

        aimButtonClicked = aimButtonPressed = false;
        StopAim();

        // if (isCrouching)
        //     OnCrouchButtonDown();

        SetListeners(false);
        input.GetAction(attackAction.action)?.onUp.RemoveListener(OnShootButtonUp);

        inCombat = false;

        base.ExitState();
    }

    public void SetListeners(bool set)
    {
        if (set)
        {
            input.GetAction(attackAction.action)?.onDown.AddListener(OnShootButtonDown);
        }
        else
        {
            input.GetAction(attackAction.action)?.onDown.RemoveListener(OnShootButtonDown);
        }
    }

    private void AimingAndShotingUpdate()
    {
        l.animator.SetFloat("RotX", Mathf.DeltaAngle(0, aimingVector.value.x));

        if (!isCurrentlyAiming && !isMelee)
        {
            StartAim();
        }

        bool canAttack;
        if (isMelee)
            canAttack = shootButtonPressed;
        else
            canAttack = aimingWeight >= 0.9f && shootButtonPressed;

        l.primaryWeapon?.SetShooting(canAttack);
        l.animator.SetBool("isAttacking", canAttack);

        if (shootButtonPressed && !isShooting)
        {
            l.buffs.AddBuff(shootingSlowdown);
            isShooting = true;
        }
    }

    public void OnConfigChanged(CharacterConfig oldConfig, CharacterConfig newConfig)
    {
        l.netConfig.configManager.weapons[oldConfig.primaryWeaponId]
            ?.onShot.RemoveListener(SpawnBullet);

        isMelee = l.primaryWeapon ? l.primaryWeapon.properties.isMelee : true;
        if (isMelee)
        {
            aimButtonClicked = aimButtonPressed = false;
            StopAim();
        }

        l.primaryWeapon?.onShot.AddListener(SpawnBullet);
    }

    public void SpawnBullet(Vector3 spawnerPosition)
    {
        if (!isOwned)
            return;

        // Vector to the target
        Vector3 dirWithoutSpread;

        // Aim ray
        Ray aimRay = new(pLinks.playerCamera.position, pLinks.playerCamera.forward);

        // Get raycast hit
        if (Physics.Raycast(aimRay, out RaycastHit aimHit, Mathf.Infinity, combatLayers))
            dirWithoutSpread = aimHit.point - spawnerPosition;
        else
            dirWithoutSpread = aimRay.GetPoint(100) - spawnerPosition;

        // Add spread
        float currentSpread = l.primaryWeapon.properties.spread;
        currentSpread *= 1 + l.movement.currentSpeed / 8;

        // if (isCrouching)
        //     currentSpread /= 1.25f;

        float xSpread = Random.Range(-currentSpread, currentSpread);
        float ySpread = Random.Range(-currentSpread, currentSpread);
        Vector3 dirWithSpread = (
            dirWithoutSpread + new Vector3(xSpread, ySpread, 0) * dirWithoutSpread.magnitude / 100
        ).normalized;

        // Spawn bullet
        CmdSpawnBullet(spawnerPosition, dirWithSpread, l.primaryWeapon.properties);

        // mFireCD = true;
        // StartCoroutine(FireRate());

        // Not automatic pLinks.weapon makes a single shot
        // if (!isAutomatic)
        //     weaponAnim.SetBool("isAttacking", false);
    }

    [Command]
    private void CmdSpawnBullet(Vector3 position, Vector3 direction, WeaponProperties properties)
    {
        // Spawning bullet on server
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));

        // Ignore shooter
        bullet.GetComponent<Bullet>().Initiate(properties, netIdentity);
        NetworkServer.Spawn(bullet, connectionToClient);
    }

    private void OnShootButtonDown()
    {
        if (!inCombat)
        {
            l.stateManager.SetState(EnumState.Combat);
            l.animator.Update(0); // Force update to enter combat state immediate
        }

        shootButtonPressed = true;
    }

    private void OnShootButtonUp()
    {
        shootButtonPressed = false;
        StopShooting();
    }

    /*IEnumerator FireRate()
    {
        yield return new WaitForSeconds(mFireRate);
        mFireCD = false;
    }*/

    private void OnAimButtonDown()
    {
        if (!inCombat)
        {
            l.stateManager.SetState(EnumState.Combat);
        }

        if (aimButtonClicked) // Stop aim
        {
            aimButtonClicked = false;
            StopAim();
        }
        else // Start aim
        {
            aimButtonPressed = true;

            if (pLinks)
            {
                zAxisSaver = pLinks.cameraController.positionOffset.z;
            }
        }
    }

    private void OnAimButtonUp()
    {
        if (aimButtonPressed)
        {
            aimButtonPressed = false;
            aimButtonPressedTime = 0;

            if (aimButtonPressedTime > 0.15f) // Stop aim
                StopAim();
            else
                aimButtonClicked = true;
        }
    }

    private void StartAim()
    {
        l.movement.ActualMovement();
        l.buffs.AddBuff(aimingSlowdown);
        l.animator.SetBool("isAiming", true);

        if (pLinks)
        {
            crosshairAnimator.GetComponent<DisabledImageAndAnimation>().moveToEnd = true;
            crosshairAnimator.SetFloat("AnimSpeed", 1);
            if (crosshairAnimator.isActiveAndEnabled)
            {
                crosshairAnimator.Play("AimAppears");
            }

            pLinks.cameraController.horizontalSens /= 2;
            pLinks.cameraController.verticalSens /= 2;
            pLinks.cameraController.isAiming = true;
            if (!pLinks.cameraController.isFirstPerson)
            {
                zAxisSaver = pLinks.cameraController.positionOffset.z;
                pLinks.cameraController.positionOffset.z = -1.2f;
            }
            pLinks.interactableTrigger.SetCheckTrigger(false);
        }

        isCurrentlyAiming = true;
    }

    private void StopAim()
    {
        if (!isCurrentlyAiming)
            return;

        l.buffs.RemoveBuff(aimingSlowdown);

        l.animator.SetBool("isAiming", false);

        if (pLinks)
        {
            pLinks.cameraController.horizontalSens = PlayerPrefs.GetFloat(
                "HorizontalSensivity",
                20
            );
            pLinks.cameraController.verticalSens = PlayerPrefs.GetFloat("VerticalSensivity", 20);
            pLinks.cameraController.isAiming = pLinks.cameraController.isAiming = false;
            if (!pLinks.cameraController.isFirstPerson)
                pLinks.cameraController.positionOffset.z = zAxisSaver;

            crosshairAnimator.GetComponent<DisabledImageAndAnimation>().moveToEnd = false;
            crosshairAnimator.SetFloat("AnimSpeed", -1);
            if (crosshairAnimator.isActiveAndEnabled)
            {
                crosshairAnimator.Play("AimAppears");
            }

            pLinks.interactableTrigger.SetCheckTrigger(true);
            //#endif
        }

        isCurrentlyAiming = isAimingOrShooting = false;
        l.movement.ActualMovement();
    }

    private void StopShooting()
    {
        if (!isShooting)
            return;

        l.buffs.RemoveBuff(shootingSlowdown);

        l.primaryWeapon?.SetShooting(false);
        l.animator.SetBool("isAttacking", false);

        isShooting = false;
    }

    // private void OnCrouchButtonDown()
    // {
    //     if (!isCrouching)
    //     {
    //         //characterCollider.height = 1.5f;
    //         //characterCollider.center = new Vector3(0, -0.31f, 0);

    //         isCrouching = true;

    //         pLinks.animator.SetBool("inCover", true);
    //         pLinks.movement.StartSlide();
    //         pLinks.cameraController.positionOffset.y = -0.5f;

    //         dashButton.SetActive(false);
    //     }
    //     else
    //     {
    //         if (!canStandTrigger.isTriggered())
    //         {
    //             //characterCollider.height = 2.12f;
    //             //characterCollider.center = Vector3.zero;

    //             isCrouching = false;
    //             pLinks.movement.isSliding = false;

    //             pLinks.animator.SetBool("inCover", false);
    //             pLinks.animator.SetBool("isSliding", false);
    //             pLinks.cameraController.positionOffset.y = 0;

    //             if (pLinks.cameraController.isFirstPerson)
    //                 pLinks.cameraController.positionOffset = Vector3.zero;
    //             else
    //                 pLinks.cameraController.positionOffset.x = 0.7f;

    //             dashButton.SetActive(true);
    //         }
    //     }
    // }

    // private void OnCrouchButtonUp() { }
}
