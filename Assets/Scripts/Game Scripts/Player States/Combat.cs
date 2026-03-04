using UnityEngine;
using UnityEngine.EventSystems;

public class Combat : State
{
    public bool isCrouching;

    [HideInInspector]
    public bool isAiming,
        isShooting,
        isAimingOrShooting;
    public int currentSet;
    private bool shootButtonPressed,
        aimButtonPressed,
        aimButtonClicked;
    private float aimButtonPressedTime;
    public float aimingWeight = 0;
    private float zAxisSaver;
    private GameObject aimFiller,
        aimFiller2,
        crouchButtonObj,
        dashButton;
    protected Trigger canStandTrigger;
    private Animator crosshairAnimator;
    private float notInCombatTime = 0;

    public Combat(Links links)
        : base(links)
    {
        SetUi();

        InputManager.Instance.GetAction(KeyCode.Mouse0)?.onDown.AddListener(OnShootButtonDown);
        InputManager.Instance.GetAction(KeyCode.Mouse0)?.onUp.AddListener(OnShootButtonUp);
        InputManager.Instance.GetAction(KeyCode.Mouse1)?.onDown.AddListener(OnAimButtonDown);
        InputManager.Instance.GetAction(KeyCode.Mouse1)?.onUp.AddListener(OnAimButtonUp);

        if (InputManager.Instance.GetKey(KeyCode.Mouse0))
        {
            OnShootButtonDown();
        }

        if (InputManager.Instance.GetKey(KeyCode.Mouse1))
        {
            OnAimButtonDown();
        }

        // Enter combat state
        // Take a l.weapon

        if (!l.cameraController.isFirstPerson)
            l.cameraController.positionOffset.x = 0.7f;

        l.animator.SetBool("inCombat", true);
        l.weapon.Activate(true);

        // Enable combat buttons
        crouchButtonObj.SetActive(true);

        currentSet = 1;
    }

    public void SetUi()
    {
        Transform combatUi = l.ui.Find("Ground Ui");

        crouchButtonObj = combatUi.Find("Crouch Button").gameObject;
        aimFiller = combatUi.Find("Aim Button/Filler").gameObject;
        aimFiller2 = combatUi.Find("Aim Button 2/Filler").gameObject;
        crosshairAnimator = combatUi.Find("Crosshair").GetComponent<Animator>();
        dashButton = combatUi.Find("Run Button").gameObject;

        canStandTrigger = l
            .transform.Find("Armature/Point Of Scaling/Can Stand Trigger")
            .GetComponent<Trigger>();

        crouchButtonObj
            .GetComponent<EventTrigger>()
            .AddPointerUpAndDownListeners(OnCrouchButtonDown, OnCrouchButtonUp);
    }

    public override void UpdateState()
    {
        l.movement.MovementUpdate();
        l.carTrigger.CarTriggerUpdate();

        float target = isAiming ? 1f : 0f;
        aimingWeight = Mathf.MoveTowards(aimingWeight, target, 3 * Time.deltaTime);

        // Aiming ray checks there's no obstacle in the way
        Vector3 origin = l.transform.position + new Vector3(0.2f, 1.25f, 0);
        Vector3 direction = l.cameraPivot.forward * 0.75f;
        Debug.DrawRay(origin, direction, Color.red);
        isAimingOrShooting =
            (shootButtonPressed || aimButtonClicked || aimButtonPressed)
            && !Physics.Raycast(
                origin,
                direction,
                out RaycastHit _,
                0.75f,
                l.humanoid.combatLayers
            );

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
                ExitCombat();
            }
        }
    }

    public override void FixedUpdateState()
    {
        l.movement.MovementFixedUpdate();
    }

    public override void ExitState()
    {
        l.weapon.Activate(false);

        InputManager.Instance.GetAction(KeyCode.Mouse0)?.onDown.RemoveListener(OnShootButtonDown);
        InputManager.Instance.GetAction(KeyCode.Mouse0)?.onUp.RemoveListener(OnShootButtonUp);
        InputManager.Instance.GetAction(KeyCode.Mouse1)?.onDown.RemoveListener(OnAimButtonDown);
        InputManager.Instance.GetAction(KeyCode.Mouse1)?.onUp.RemoveListener(OnAimButtonUp);
    }

    private void AimingAndShotingUpdate()
    {
        l.animator.SetFloat("RotX", Mathf.DeltaAngle(0, l.playerCamera.eulerAngles.x));

        // Start aiming
        if (!isAiming)
        {
            StartAim();
        }

        // Slow down when shooting
        l.weapon.SetShooting(aimingWeight >= 0.9f && shootButtonPressed);

        if (shootButtonPressed && !isShooting)
        {
            l.movement.speedModifier /= 1.2f;
            isShooting = true;
        }
    }

    private void ExitCombat()
    {
        // Remove current l.weapon
        l.cameraController.positionOffset.x = 0;

        aimButtonClicked = aimButtonPressed = false;
        StopAim();

        if (isCrouching)
            OnCrouchButtonDown();

        l.animator.SetBool("inCombat", false);
        l.weapon.Activate(false);
        l.humanoid.SetState(new Default(l));
    }

    public void SpawnBullet(Vector3 spawnerPosition)
    {
        // Vector to the target
        Vector3 dirWithoutSpread;

        // Aim ray
        Ray aimRay = new(l.playerCamera.position, l.playerCamera.forward);

        // Get raycast hit
        if (Physics.Raycast(aimRay, out RaycastHit aimHit, Mathf.Infinity, l.humanoid.combatLayers))
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
        l.networkCommands.RequestSpawnBullet(spawnerPosition, dirWithSpread, l.weapon.properties);

        // mFireCD = true;
        // StartCoroutine(FireRate());

        // Not automatic l.weapon makes a single shot
        // if (!isAutomatic)
        //     weaponAnim.SetBool("isShooting", false);
    }

    private void OnShootButtonDown()
    {
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
        if (aimButtonClicked) // Stop aim
        {
            if (isAiming)
                StopAim();

            aimButtonClicked = false;

            //#if UNITY_ANDROID
            aimFiller.SetActive(false);
            aimFiller2.SetActive(false);
            //#endif
        }
        else // Start aim
        {
            aimButtonPressed = true;

            zAxisSaver = l.cameraController.positionOffset.z;

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

                //#if UNITY_ANDROID
                aimFiller.SetActive(false);
                aimFiller2.SetActive(false);
                //#endif
            }
            else
                aimButtonClicked = true;
        }

        aimButtonPressed = false;
        aimButtonPressedTime = 0;
    }

    private void StartAim()
    {
        l.movement.speedModifier /= 1.4f;

        crosshairAnimator.SetFloat("AnimSpeed", 1);
        crosshairAnimator.Play("AimAppears");
        l.animator.SetBool("isAiming", true);

        l.cameraController.horizontalSens /= 2;
        l.cameraController.verticalSens /= 2;

        l.cameraController.isAiming = true;

        if (!l.cameraController.isFirstPerson)
        {
            zAxisSaver = l.cameraController.positionOffset.z;
            l.cameraController.positionOffset.z = -1.2f;
        }

        isAiming = true;
    }

    private void StopAim()
    {
        if (!isAiming)
            return;

        l.movement.speedModifier *= 1.4f;

        crosshairAnimator.SetFloat("AnimSpeed", -1);
        crosshairAnimator.Play("AimAppears");
        l.animator.SetBool("isAiming", false);

        l.cameraController.horizontalSens = PlayerPrefs.GetFloat("HorizontalSensivity", 20);
        l.cameraController.verticalSens = PlayerPrefs.GetFloat("VerticalSensivity", 20);

        l.cameraController.isAiming = l.cameraController.isAiming = false;

        if (!l.cameraController.isFirstPerson)
            l.cameraController.positionOffset.z = zAxisSaver;

        isAiming = isAimingOrShooting = false;
    }

    private void StopShooting()
    {
        if (!isShooting)
            return;

        l.movement.speedModifier *= 1.2f;

        l.weapon.SetShooting(false);

        isShooting = false;
    }

    private void OnCrouchButtonDown()
    {
        if (!isCrouching)
        {
            //characterCollider.height = 1.5f;
            //characterCollider.center = new Vector3(0, -0.31f, 0);

            isCrouching = true;

            l.animator.SetBool("inCover", true);
            l.movement.StartSlide();
            l.cameraController.positionOffset.y = -0.5f;

            dashButton.SetActive(false);
        }
        else
        {
            if (!canStandTrigger.isTriggered())
            {
                //characterCollider.height = 2.12f;
                //characterCollider.center = Vector3.zero;

                isCrouching = false;
                l.movement.isSliding = false;

                l.animator.SetBool("inCover", false);
                l.animator.SetBool("isSliding", false);
                l.cameraController.positionOffset.y = 0;

                if (l.cameraController.isFirstPerson)
                    l.cameraController.positionOffset = Vector3.zero;
                else
                    l.cameraController.positionOffset.x = 0.7f;

                dashButton.SetActive(true);
            }
        }
    }

    private void OnCrouchButtonUp() { }
}
