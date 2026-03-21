using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField]
    private LayerMask combatLayers;

    [SerializeField]
    private GameObject bulletPrefab;
    private InputAction attackAction,
        aimAction;

    public void Start()
    {
        attackAction = InputSystem.actions.FindAction("Attack");
        aimAction = InputSystem.actions.FindAction("Aim");

        PlayerInput.Instance.GetAction(attackAction)?.onDown.AddListener(OnShootButtonDown);
        PlayerInput.Instance.GetAction(aimAction)?.onDown.AddListener(OnAimButtonDown);

        Transform combatUi = pLinks.ui.Find("Ground Ui");

        aimFiller = combatUi.Find("Aim Button/Filler").gameObject;
        aimFiller2 = combatUi.Find("Aim Button 2/Filler").gameObject;
        crosshairAnimator = combatUi.Find("Crosshair").GetComponent<Animator>();

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

        PlayerInput.Instance.GetAction(attackAction)?.onUp.AddListener(OnShootButtonUp);
        PlayerInput.Instance.GetAction(aimAction)?.onUp.AddListener(OnAimButtonUp);

        if (!pLinks.cameraController.isFirstPerson)
            pLinks.cameraController.positionOffset.x = 0.7f;

        pLinks.animator.SetBool("inCombat", true);

        CharacterConfig config = new(pLinks.netConfig.config) { inCombat = true };
        pLinks.netConfig.CmdSetConfig(config);

        pLinks.weapon.onShot.AddListener(SpawnBullet);

        currentSet = 1;
    }

    public override void UpdateState()
    {
        pLinks.movement.MovementUpdate();
        pLinks.carTrigger.CarTriggerUpdate();

        float target = isAiming ? 1f : 0f;
        aimingWeight = Mathf.MoveTowards(
            aimingWeight,
            target,
            aimingWeightSpeedModifier * Time.deltaTime
        );

        // Aiming ray checks there's no obstacle in the way
        Vector3 origin = pLinks.transform.position + new Vector3(0.2f, 1.25f, 0);
        Vector3 direction = pLinks.cameraPivot.forward * 0.75f;
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
                pLinks.stateManager.SetState(EnumState.Default);
            }
        }
    }

    public override void FixedUpdateState()
    {
        pLinks.movement.MovementFixedUpdate();
    }

    public override void ExitState()
    {
        pLinks.cameraController.positionOffset.x = 0;

        aimButtonClicked = aimButtonPressed = false;
        StopAim();

        // if (isCrouching)
        //     OnCrouchButtonDown();

        pLinks.animator.SetBool("inCombat", false);

        CharacterConfig config = new(pLinks.netConfig.config) { inCombat = false };
        pLinks.netConfig.CmdSetConfig(config);
        pLinks.weapon.onShot.RemoveListener(SpawnBullet);

        PlayerInput.Instance.GetAction(attackAction)?.onUp.RemoveListener(OnShootButtonUp);
        PlayerInput.Instance.GetAction(aimAction)?.onUp.RemoveListener(OnAimButtonUp);

        inCombat = false;
    }

    private void AimingAndShotingUpdate()
    {
        pLinks.animator.SetFloat("RotX", Mathf.DeltaAngle(0, pLinks.playerCamera.eulerAngles.x));

        // Start aiming
        if (!isAiming)
        {
            StartAim();
        }

        // Slow down when shooting
        pLinks.weapon.SetShooting(aimingWeight >= 0.9f && shootButtonPressed);

        if (shootButtonPressed && !isShooting)
        {
            pLinks.movement.speedModifier /= 1.2f;
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
        float currentSpread = pLinks.weapon.properties.spread;
        currentSpread *= 1 + pLinks.movement.currentSpeed / 8;

        // if (isCrouching)
        //     currentSpread /= 1.25f;

        float xSpread = Random.Range(-currentSpread, currentSpread);
        float ySpread = Random.Range(-currentSpread, currentSpread);
        Vector3 dirWithSpread = (
            dirWithoutSpread + new Vector3(xSpread, ySpread, 0) * dirWithoutSpread.magnitude / 100
        ).normalized;

        // Spawn bullet
        CmdSpawnBullet(spawnerPosition, dirWithSpread, pLinks.weapon.properties);

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
            pLinks.stateManager.SetState(EnumState.Combat);
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
            pLinks.stateManager.SetState(EnumState.Combat);
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

            zAxisSaver = pLinks.cameraController.positionOffset.z;

            //#if UNITY_ANDROID
            aimFiller.SetActive(true);
            aimFiller2.SetActive(true);
            //#endif
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
        pLinks.movement.ActualMovement();

        pLinks.movement.speedModifier /= 1.4f;

        crosshairAnimator.SetFloat("AnimSpeed", 1);
        crosshairAnimator.Play("AimAppears");
        pLinks.animator.SetBool("isAiming", true);

        pLinks.cameraController.horizontalSens /= 2;
        pLinks.cameraController.verticalSens /= 2;

        pLinks.cameraController.isAiming = true;

        if (!pLinks.cameraController.isFirstPerson)
        {
            zAxisSaver = pLinks.cameraController.positionOffset.z;
            pLinks.cameraController.positionOffset.z = -1.2f;
        }

        isAiming = true;
    }

    private void StopAim()
    {
        if (!isAiming)
            return;

        pLinks.movement.speedModifier *= 1.4f;

        crosshairAnimator.SetFloat("AnimSpeed", -1);
        crosshairAnimator.Play("AimAppears");
        pLinks.animator.SetBool("isAiming", false);

        pLinks.cameraController.horizontalSens = PlayerPrefs.GetFloat("HorizontalSensivity", 20);
        pLinks.cameraController.verticalSens = PlayerPrefs.GetFloat("VerticalSensivity", 20);

        pLinks.cameraController.isAiming = pLinks.cameraController.isAiming = false;

        if (!pLinks.cameraController.isFirstPerson)
            pLinks.cameraController.positionOffset.z = zAxisSaver;

        isAiming = isAimingOrShooting = false;
        pLinks.movement.ActualMovement();

        //#if UNITY_ANDROID
        aimFiller.SetActive(false);
        aimFiller2.SetActive(false);
        //#endif
    }

    private void StopShooting()
    {
        if (!isShooting)
            return;

        pLinks.movement.speedModifier *= 1.2f;

        pLinks.weapon.SetShooting(false);

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
