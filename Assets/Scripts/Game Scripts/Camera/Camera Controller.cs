using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
    public bool isFirstPerson { get; private set; }

    [HideInInspector]
    public bool isAiming;
    public float angleX,
        angleY,
        minPitch = -80,
        maxPitch = 90,
        yPitch = 180,
        horizontalSens = 10,
        verticalSens = 10;
    public CameraMode cameraMode = CameraMode.Player;
    public Vector3 positionOffset = new(0, 0, -3);
    public LayerMask excudeLayers;
    private bool autoRotationEnabled,
        isRestoringCamera;
    private CharacterConfigManager characterConfig;
    private Transform character;
    private int previousLayer;
    private float horAdj = 0.3f,
        vertAdj = 0.5f,
        timeWithoutCameraMovement;

    [SerializeField]
    private float cameraSpeed = 2,
        collisionRadius = 0.1f,
        minDistance = -1,
        maxDistance = -7;

    private Transform currentPivot,
        thirdPersonCameraPivot,
        firstPersonCameraPivot;
    private Vector2Reference aimingVector;
    private Transform cameraPivot => transform.parent;
    public UnityAction<float> OnDistChanged;
    private PlayerLinks l;

    private void Start()
    {
        UpdateSettings();
        PlayerInput.Instance.OnZoom += Zoom;
    }

    public void ChangeTarget(
        Transform character,
        PlayerLinks links,
        Transform firstPersonCameraPivot,
        Transform thirdPersonCameraPivot
    )
    {
        // Return to the previous layer
        if (this.character)
        {
            ChangeCollidersToLayer(this.character, previousLayer);
        }
        // Set new layer to colliders and save previous one
        this.character = character;
        previousLayer = this.character.Find("Armature/mixamorig:Hips").gameObject.layer;
        int currentPlayerLayer = LayerMask.NameToLayer("Current Player");
        ChangeCollidersToLayer(this.character, currentPlayerLayer);

        aimingVector = character.GetComponent<Vector2Reference>();
        l = links;
        SetCameraPivots(firstPersonCameraPivot, thirdPersonCameraPivot);
    }

    private void ChangeCollidersToLayer(Transform character, int layerId)
    {
        foreach (
            var collider in character
                .Find("Armature/mixamorig:Hips")
                .GetComponentsInChildren<Collider>(true)
        )
        {
            collider.gameObject.layer = layerId;
        }
    }

    private void LateUpdate()
    {
        if (!cameraPivot || !currentPivot)
            return;

        cameraPivot.position = currentPivot.position;

        float distance = Vector3.Distance(transform.position, cameraPivot.position);
        OnDistChanged.Invoke(distance);

        // Player visibility
        if (isFirstPerson)
            SetMaterialsAlpha(0);
        else if (!isAiming)
        {
            float camDistFactor = Mathf.InverseLerp(0.5f, 2, distance);
            SetMaterialsAlpha(camDistFactor);
        }

        angleX -= PlayerInput.Instance.MouseVertical * verticalSens;
        angleY += PlayerInput.Instance.MouseHorizontal * horizontalSens;

        // Camera angle borders
        angleX = Mathf.Clamp(angleX, minPitch, maxPitch);

        //Camera type switch
        switch (cameraMode)
        {
            case CameraMode.Player:
            {
                CameraMode_Player();
                break;
            }
            case CameraMode.Car:
                {
                    CameraMode_Car();
                    break;
                }

                //case CameraType.CopyHeadRotation:
                {
                    //CameraMove_CopyHeadRotation(); break;
                }
        }

        MoveCamera();
    }

    void OnDestroy()
    {
        PlayerInput.Instance.OnZoom -= Zoom;
    }

    private Car carScript;
    public Car CarScript
    {
        set
        {
            carScript = value;
            if (carScript == null)
                cameraMode = CameraMode.Player;
            else
                cameraMode = CameraMode.Car;
        }
    }

    private void CameraMode_Player()
    {
        // Cam slowly rotates towards direction if no movement
        if (!isFirstPerson && !mouseMoved && autoRotationEnabled)
        {
            if (
                l.movement.isMoving
                && !(l.stateManager.state is CombatState combat && combat.isAimingOrShooting)
            )
            {
                if (timeWithoutCameraMovement > 5)
                {
                    angleX = Mathf.LerpAngle(
                        angleX,
                        character.eulerAngles.x,
                        vertAdj * Time.unscaledDeltaTime
                    );
                    angleY = Mathf.LerpAngle(
                        angleY,
                        character.eulerAngles.y,
                        horAdj * Time.unscaledDeltaTime
                    );
                }
                else
                    timeWithoutCameraMovement += Time.unscaledDeltaTime;
            }
        }
        else
            timeWithoutCameraMovement = 0;

        if (yPitch < 180)
        {
            float teu = transform.eulerAngles.y; // Transform world rotation
            if (Mathf.DeltaAngle(angleY, teu) > yPitch)
                angleY = teu - yPitch;
            if (Mathf.DeltaAngle(angleY, teu) < -yPitch)
                angleY = teu + yPitch;
        }

        // Rotate camera & triggers
        cameraPivot.rotation = Quaternion.Euler(angleX, angleY, 0);

        l.minimap.angleY = angleY;
        if (aimingVector)
            aimingVector.value = new(angleX, angleY);
    }

    private void CameraMode_Car()
    {
        // Cam slowly returns to zero rotation
        if (!mouseMoved && autoRotationEnabled) // If mouse is not moved
        {
            if (timeWithoutCameraMovement > 3)
            {
                angleX = Mathf.LerpAngle(angleX, 0, Time.unscaledDeltaTime);

                if (isFirstPerson)
                    angleY = Mathf.LerpAngle(
                        angleY,
                        PlayerInput.Instance.Horizontal * 20,
                        Time.unscaledDeltaTime * 2
                    );
                else if (carScript.isMoving)
                    angleY = Mathf.LerpAngle(
                        angleY,
                        character.eulerAngles.y + (carScript.speed < -1 ? 180 : 0),
                        Time.unscaledDeltaTime
                    );
            }
            else if (carScript.isMoving)
                timeWithoutCameraMovement += Time.unscaledDeltaTime;
        }
        else
        {
            timeWithoutCameraMovement = 0;
        }

        if (isFirstPerson)
        {
            cameraPivot.rotation = character.rotation * Quaternion.Euler(angleX, angleY, 0);
            l.minimap.angleY = character.eulerAngles.y + angleY;
        }
        else
        {
            cameraPivot.rotation = Quaternion.Euler(angleX, angleY, 0);
            l.minimap.angleY = angleY;
        }
    }

    bool mouseMoved =>
        (PlayerInput.Instance.MouseHorizontal != 0) && (PlayerInput.Instance.MouseVertical != 0);

    private void MoveCamera()
    {
        // Change camera position smoothly
        if (isRestoringCamera)
        {
            Vector3 desiredPosition = positionOffset;

            // Prevent collision enter
            Vector3 direction = transform.position - cameraPivot.position;

            // Getting point of contact in local coordinates
            if (
                Physics.SphereCast(
                    cameraPivot.position,
                    collisionRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    direction.magnitude,
                    ~excudeLayers
                )
            )
            {
                desiredPosition = cameraPivot.InverseTransformPoint(
                    hit.point + hit.normal * collisionRadius
                );
            }

            // Smoothly moving camera to the point of contact ignoring collision
            if (Vector3.Distance(desiredPosition, transform.localPosition) > 0.1f)
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    desiredPosition,
                    Time.unscaledDeltaTime * cameraSpeed * 3
                );
            else
            {
                // If destination reached, camera restored
                transform.localPosition = desiredPosition;
                isRestoringCamera = false;
            }
        }
        else
        {
            if (Vector3.Distance(positionOffset, transform.localPosition) > 0.0001f)
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    positionOffset,
                    Time.unscaledDeltaTime * cameraSpeed
                );
            else
                transform.localPosition = positionOffset;

            // Prevent collision enter
            Vector3 direction = transform.position - cameraPivot.position;

            // Teleport camera to the point of contact
            if (
                Physics.SphereCast(
                    cameraPivot.position,
                    collisionRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    direction.magnitude,
                    ~excudeLayers
                )
            )
            {
                // print(hit.collider);
                transform.position = hit.point + hit.normal * collisionRadius;
            }
        }
    }

    private void ChangeCurrentCameraPivot(Transform newPivot)
    {
        Vector3 currentPosition = transform.position;
        currentPivot = newPivot;
        cameraPivot.position = currentPivot.position;
        transform.position = currentPosition;
        isRestoringCamera = true;
    }

    private void SetCameraPivots(Transform firstPersonPivot, Transform thirdPersonPivot)
    {
        firstPersonCameraPivot = firstPersonPivot;
        thirdPersonCameraPivot = thirdPersonPivot;

        UpdateCameraPivot();

        characterConfig = firstPersonCameraPivot.parent.GetComponent<CharacterConfigManager>();
    }

    public void ChangeCameraRange(float minDistance, float maxDistance)
    {
        float zFactor = positionOffset.z;
        if (zFactor != 0)
        {
            zFactor = Mathf.InverseLerp(this.minDistance, this.maxDistance, positionOffset.z);
            zFactor = Mathf.Lerp(minDistance, maxDistance, zFactor);
            positionOffset.z = zFactor;
        }

        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    private void Zoom(float delta)
    {
        if (isAiming)
            return;

        if (isFirstPerson)
        {
            // Can't zoom more if in first person
            if (delta >= 0)
                return;

            // Switch to third person
            positionOffset.z = minDistance;
        }
        else
            positionOffset.z += delta * cameraSpeed;

        positionOffset.z = Mathf.Clamp(positionOffset.z, maxDistance, 0);
        float zFactor = Mathf.InverseLerp(minDistance, maxDistance, positionOffset.z);
        l.minimap.cameraSize = Mathf.Lerp(70, 100, zFactor);

        // Switch to first person
        if (positionOffset.z > minDistance)
        {
            positionOffset = Vector3.zero;

            if (!isFirstPerson)
            {
                isFirstPerson = true;
                UpdateCameraPivot();

                characterConfig
                    .GetWeapon()
                    .GetComponent<Weapon>()
                    .GetFlash()
                    .gameObject.SetActive(false);

                if (cameraMode == CameraMode.Car)
                    angleY -= character.eulerAngles.y;
            }
        }
        // Switch to third person
        else if (isFirstPerson)
        {
            isFirstPerson = false;
            UpdateCameraPivot();

            characterConfig
                .GetWeapon()
                .GetComponent<Weapon>()
                .GetFlash()
                .gameObject.SetActive(true);

            if (cameraMode == CameraMode.Car)
                angleY = cameraPivot.eulerAngles.y;

            if (l.stateManager.state is CombatState)
                positionOffset.x = 0.7f;
        }

        if (positionOffset.z < maxDistance)
        {
            positionOffset.z = maxDistance;
        }
    }

    private void UpdateCameraPivot() =>
        ChangeCurrentCameraPivot(isFirstPerson ? firstPersonCameraPivot : thirdPersonCameraPivot);

    public void UpdateSettings()
    {
        autoRotationEnabled = PlayerPrefs.GetInt("AutoRotationEnabled", 1) == 1;
        horAdj = PlayerPrefs.GetFloat("HorizontalAdjustment", 10) * 0.03f;
        vertAdj = PlayerPrefs.GetFloat("VerticalAdjustment", 10) * 0.01f;
        horizontalSens = PlayerPrefs.GetFloat("HorizontalSensivity", 20);
        verticalSens = PlayerPrefs.GetFloat("VerticalSensivity", 20);
    }

    private void SetMaterialsAlpha(float alpha)
    {
        SetGameobjectAlpha(characterConfig.GetHead(), alpha);
        SetChildGameobjectAlpha(l.weapon.gameObject, alpha);
        SetChildGameobjectAlpha(l.weapon.GetHidden(), alpha);
        SetGameobjectAlpha(characterConfig.GetPants(), alpha);
        SetGameobjectAlpha(characterConfig.GetTop(), alpha);
        SetGameobjectAlpha(characterConfig.GetShoes(), alpha);
        SetGameobjectAlpha(characterConfig.GetGloves(), alpha);
        SetGameobjectAlpha(characterConfig.GetHat(), alpha);
        SetGameobjectAlpha(characterConfig.GetHair(), alpha);
        SetGameobjectAlpha(characterConfig.GetMask(), alpha);
    }

    private void SetGameobjectAlpha(GameObject obj, float alpha)
    {
        if (!obj)
            return;

        foreach (var material in obj.GetComponent<SkinnedMeshRenderer>().materials)
        {
            Color currentColor = material.GetColor("_Color");
            currentColor.a = alpha;
            material.SetColor("_Color", currentColor);
        }
    }

    private void SetChildGameobjectAlpha(GameObject obj, float alpha)
    {
        if (!obj)
            return;

        foreach (var renderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            SetGameobjectAlpha(renderer.gameObject, alpha);
    }
}

public enum CameraMode
{
    Player,
    Car,
    CopyHeadRotation,
}
