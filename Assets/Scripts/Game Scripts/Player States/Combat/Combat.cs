using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(DefaultState))]
[RequireComponent(typeof(Vector2Reference))]
public class CombatState : State
{
    [HideInInspector]
    public bool inCombat,
        isAiming,
        isShooting,
        isAimingOrShooting;
    public int currentSet;
    private bool shootButtonPressed,
        aimButtonPressed,
        aimButtonClicked;
    private float aimButtonPressedTime;
    public float aimingWeight = 0,
        aimingWeightSpeedModifier = 3;
    private float zAxisSaver;
    private GameObject aimFiller,
        aimFiller2;
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
        aimAction;
    private InputManager input;

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
            aimFiller = combatUi.Find("Aim Button/Filler").gameObject;
            aimFiller2 = combatUi.Find("Aim Button 2/Filler").gameObject;
            crosshairAnimator = pLinks.ui.Find("Crosshair").GetComponent<Animator>();
        }

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
                pLinks.cameraController.positionOffset.x = 0.7f;
        }

        SetListeners(true);
        input.GetAction(attackAction.action)?.onUp.AddListener(OnShootButtonUp);
        input.GetAction(aimAction.action)?.onUp.AddListener(OnAimButtonUp);

        l.animator.SetBool("inCombat", true);

        CharacterConfig config = new(l.netConfig.config) { inCombat = true };
        l.netConfig.CmdSetConfig(config);

        l.weapon.onShot.AddListener(SpawnBullet);

        currentSet = 1;

        base.EnterState();
    }

    public override void UpdateState()
    {
        l.movement.MovementUpdate();
        l.carTrigger.CarTriggerUpdate();

        float target = isAiming ? 1f : 0f;
        aimingWeight = Mathf.MoveTowards(
            aimingWeight,
            target,
            aimingWeightSpeedModifier * Time.deltaTime
        );

        // Aiming ray checks there's no obstacle in the way
        Vector3 origin = l.transform.position + new Vector3(-0.2f, 1.25f, 0);
        Vector2 angles = new(aimingVector.value.x, aimingVector.value.y);
        Vector3 direction = Quaternion.Euler(angles.x, angles.y, 0) * Vector3.forward * 0.75f;

        Debug.DrawRay(origin, direction, Color.red);
        isAimingOrShooting =
            (shootButtonPressed || aimButtonClicked || aimButtonPressed)
            && !Physics.Raycast(origin, direction, out RaycastHit _, 0.75f, combatLayers);

        if (aimButtonPressed)
            aimButtonPressedTime += Time.deltaTime;

        if (isAimingOrShooting)
        {
            AimingAndShotingUpdate();
            notInCombatTime = 0;
        }
        else
        {
            StopAim();
            StopShooting();

            // Exit combat after 5 seconds
            notInCombatTime += Time.deltaTime;
            if (notInCombatTime > 5)
            {
                l.stateManager.SetState(EnumState.Default);
            }
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
            pLinks.animator.SetBool("inCombat", false);
            CharacterConfig config = new(pLinks.netConfig.config) { inCombat = false };
            pLinks.netConfig.CmdSetConfig(config);
            pLinks.weapon.onShot.RemoveListener(SpawnBullet);
        }

        aimButtonClicked = aimButtonPressed = false;
        StopAim();

        // if (isCrouching)
        //     OnCrouchButtonDown();

        SetListeners(false);
        input.GetAction(attackAction.action)?.onUp.RemoveListener(OnShootButtonUp);
        input.GetAction(aimAction.action)?.onUp.RemoveListener(OnAimButtonUp);

        inCombat = false;

        base.ExitState();
    }

    public void SetListeners(bool set)
    {
        if (set)
        {
            input.GetAction(attackAction.action)?.onDown.AddListener(OnShootButtonDown);
            input.GetAction(aimAction.action)?.onDown.AddListener(OnAimButtonDown);
        }
        else
        {
            input.GetAction(attackAction.action)?.onDown.RemoveListener(OnShootButtonDown);
            input.GetAction(aimAction.action)?.onDown.RemoveListener(OnAimButtonDown);
        }
    }

    private void AimingAndShotingUpdate()
    {
        l.animator.SetFloat("RotX", Mathf.DeltaAngle(0, aimingVector.value.x));

        // Start aiming
        if (!isAiming)
        {
            StartAim();
        }

        l.weapon.SetShooting(aimingWeight >= 0.9f && shootButtonPressed);

        // Slow down when shooting
        if (shootButtonPressed && !isShooting)
        {
            l.buffs.AddBuff(shootingSlowdown);
            isShooting = true;
        }
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
        float currentSpread = l.weapon.properties.spread;
        currentSpread *= 1 + l.movement.currentSpeed / 8;

        // if (isCrouching)
        //     currentSpread /= 1.25f;

        float xSpread = Random.Range(-currentSpread, currentSpread);
        float ySpread = Random.Range(-currentSpread, currentSpread);
        Vector3 dirWithSpread = (
            dirWithoutSpread + new Vector3(xSpread, ySpread, 0) * dirWithoutSpread.magnitude / 100
        ).normalized;

        // Spawn bullet
        CmdSpawnBullet(spawnerPosition, dirWithSpread, l.weapon.properties);

        // mFireCD = true;
        // StartCoroutine(FireRate());

        // Not automatic pLinks.weapon makes a single shot
        // if (!isAutomatic)
        //     weaponAnim.SetBool("isShooting", false);
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
            if (isAiming)
                StopAim();

            aimButtonClicked = false;
        }
        else // Start aim
        {
            aimButtonPressed = true;

            if (pLinks)
            {
                zAxisSaver = pLinks.cameraController.positionOffset.z;

                //#if UNITY_ANDROID
                aimFiller?.SetActive(true);
                aimFiller2?.SetActive(true);
                //#endif
            }
        }
    }

    private void OnAimButtonUp()
    {
        if (aimButtonPressed)
        {
            if (aimButtonPressedTime > 0.15f) // Stop aim
            {
                if (isAiming)
                    StopAim();
            }
            else
                aimButtonClicked = true;
        }

        aimButtonPressed = false;
        aimButtonPressedTime = 0;
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

        isAiming = true;
    }

    private void StopAim()
    {
        if (!isAiming)
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

            aimFiller.SetActive(false);
            aimFiller2.SetActive(false);

            pLinks.interactableTrigger.SetCheckTrigger(true);
            //#endif
        }

        isAiming = isAimingOrShooting = false;
        l.movement.ActualMovement();
    }

    private void StopShooting()
    {
        if (!isShooting)
            return;

        l.buffs.RemoveBuff(shootingSlowdown);

        l.weapon.SetShooting(false);

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
