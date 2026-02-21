using Mirror;
using UnityEngine;

public class Humanoid : NetworkBehaviour
{
    [Header("Main")]
    public State state { get; private set; }

    [Header("Movement")]
    public bool canMove = true;
    public float defaultSpeed = 3;

    [Header("Ground ray")]
    public bool isGrounded;
    public Vector3 groundRayOffset = new(0, 0.25f, 0);
    public float groundRayLength = 0.25f;
    public LayerMask groundTriggerLayers;

    [Header("Item Grabber")]
    public float itemGrabberLength = 3;
    public float itemThrowingForce = 20;
    public LayerMask itemGrabberLayers;

    [Header("Combat")]
    public LayerMask combatLayers;

    [Header("Car Trigger")]
    public float carTriggerRadius = 1;
    public Vector3 carTriggerOffset = new(0, 1, 0);
    public LayerMask carTriggerLayers;

    [Header("Ragdoll")]
    //public GameObject ragdollPrefab;
    //private float rotateTo;
    private float boneRotTime = 1;
    private bool bonesRotating;
    private Transform[] bones;
    private Links l;

    private void Start()
    {
        l = GetComponent<Links>();

        state = new Default(l);

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.gameObject.layer = ignoreRaycastLayer;
        }

        InputManager.Instance.GetAction(KeyCode.Mouse0).onDown.AddListener(SwitchToCombat);
        InputManager.Instance.GetAction(KeyCode.Mouse1).onDown.AddListener(SwitchToCombat);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.LeftControl))
        //    OnCrouchButtonDown();

        state.UpdateState();
    }

    private void LateUpdate()
    {
        if (!canMove && !bonesRotating && boneRotTime < 1)
            boneRotTime += Time.deltaTime / 2;
    }

    private void FixedUpdate()
    {
        state.FixedUpdateState();
    }

    public void SetState(State state)
    {
        if (this.state.GetType() == state.GetType())
            return;

        this.state.ExitState();
        this.state = state;
    }

    public void SwitchToCombat()
    {
        if (state is not Combat)
            SetState(new Combat(l));
    }

    public void TransitionToRagdoll()
    {
        /*//#if UNITY_ANDROID
        if (isAiming)
        {
            StopAim();
            aimFiller.SetActive(false);
            aimFiller2.SetActive(false);
        }

        foreach (Button but in statesButtons.GetComponentsInChildren<Button>())
            but.onClick.RemoveAllListeners();

        foreach (EventTrigger eventTrigger in eventTriggers)
        {
            eventTrigger.triggers.Clear();
        }

        mainCamera.parent = transform.parent;

        GameObject rd = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        rd.transform.parent = transform.parent;

        Transform[] allTransforms = GetComponentsInChildren<Transform>();
        List<Transform> boneTransforms = new List<Transform>();
        foreach (Transform objTransform in allTransforms)
        {
            if (objTransform.gameObject.name.Contains("mixamorig"))
                boneTransforms.Add(objTransform);
        }

        Transform rdHand = null;
        Transform rdRifle = null;

        allTransforms = rd.GetComponentsInChildren<Transform>();
        List<Transform> boneRDTransforms = new List<Transform>();
        foreach (Transform objTransform in allTransforms)
        {
            if (objTransform.name.Contains("mixamorig"))
            {
                boneRDTransforms.Add(objTransform);

                if (objTransform.name.Equals("mixamorig:RightHand"))
                    rdHand = objTransform;
            }

            if (objTransform.name.Equals("WeaponHolder"))
                rdRifle = objTransform;
        }

        for (int i = 0; i < boneTransforms.Count; i++)
        {
            boneRDTransforms[i].rotation = boneTransforms[i].rotation;
            boneRDTransforms[i].position = boneTransforms[i].position;
        }

        if (rifleTransform.parent.name.Equals("mixamorig:RightHand"))
        {
            rdRifle.parent = rdHand;
            rdRifle.position = rifleTransform.position;
            rdRifle.rotation = rifleTransform.rotation;
        }

        Rigidbody[] ragdolRigidbodies = rd.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody ragdolRigidbody in ragdolRigidbodies)
        {
            ragdolRigidbody.linearVelocity = rb.linearVelocity;
        }

        rd.GetComponent<PlayerRagdollControl>().Setter(canvas.Find("Touch Field").GetComponent<TouchField>(), cameraController.positionOffset, 0, movement.joystick, minimap);

        Destroy(gameObject);*/
    }

    public void Setter(Vector3 pos, int set)
    {
        l.cameraController.positionOffset = pos;

        //set
        //currentSet = set;

        StartBonesRotationFront();
    }

    public void StartBonesRotationFront()
    {
        Transform armature = transform.Find("Armature");
        Transform hip = armature.Find("mixamorig:Hips");

        //rotate hips forward
        //rotateTo = Vector3.Angle(hip.up, Vector3.forward) * (Vector3.Angle(hip.up, Vector3.right) > 90 ? -1 : 1);

        //move player to the hips
        Vector3 shiftPos = hip.position - armature.position;
        hip.position -= shiftPos;
        transform.position += shiftPos - Vector3.up * 0.3f;
        armature.localPosition = Vector3.zero;
        hip.localPosition = new Vector3(0, 0.3f, 0);

        bones = armature.GetComponentsInChildren<Transform>();
        boneRotTime = 0;
        bonesRotating = true;

        canMove = false;

        //if (cameraController.positionOffset.z == 0)
        //    cameraType = CameraType.CopyHeadRotation;
    }

    private void StandUpForward()
    {
        /*boneRotTime += Time.deltaTime / 5;

        foreach (Transform bone in bones)
        {
            if (bone.TryGetComponent<Rigidbody>(out Rigidbody rig))
                rig.freezeRotation = true;

            if (bone.name.Equals("mixamorig:Hips"))
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, rotateTo, 0), boneRotTime);
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.2555f, 0.098863f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(87.26f, 0, 0), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftUpLeg"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(-0.08651f, -0.05492f, 0.0068763f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(359.35f, 358.59f, 169.93f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:RightUpLeg"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0.0865083f, -0.05492f, 0.0058773f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(352.98f, 358.838f, 195.72f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftLeg"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.4401798f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(343.444f, 9.2f, 2.9f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:RightLeg"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.440066f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(332.688f, 356.3f, 357.5f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftFoot"))
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(38.87f, 359.3f, 359), boneRotTime);
            if (bone.name.Equals("mixamorig:RightFoot"))
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(30.695f, 347.5f, 348.1f), boneRotTime);
            if (bone.name.Equals("mixamorig:Spine"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.11683f, -0.00835f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(350.5f, 1, 1), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:Spine1"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.1154f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(0.2f, 0.5f, 0.338f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:Spine2"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.1319f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(1.67f, 0.511f, 0.34f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:Neck"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.14839f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(2.25f, 7, 8.4f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:Head"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.089242f, 0.015634f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(352, 51.78f, 356.55f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftShoulder"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(-0.05963f, 0.124944f, -0.00194f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(79.535f, 229.9f, 335.3f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:RightShoulder"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0.05963f, 0.1249f, -0.0014f), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(72.77f, 113, 3.33f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftArm"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.12816f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(17.93f, 49.8f, 25.854f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:RightArm"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.15173f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(52.258f, 245.775f, 303.113f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftForeArm"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.2369f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(1.465f, 1.188f, 232.3f), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:RightForeArm"))
            {
                bone.localPosition = Vector3.Lerp(bone.localPosition, new Vector3(0, 0.28037f, 0), boneRotTime);
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(8.8f, -2.6f, 113), boneRotTime);
            }
            if (bone.name.Equals("mixamorig:LeftHand"))
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(342.57f, 345.654f, 25.6f), boneRotTime);
            if (bone.name.Equals("mixamorig:RightHand"))
                bone.localRotation = Quaternion.Slerp(bone.localRotation, Quaternion.Euler(10.98f, 29.25f, 1.11f), boneRotTime);
        }

        if (boneRotTime > 0.1)
        {
            animator.enabled = true;
            animator.Play("Standing Front");

            if (currentSet != 0)
            {
                inCombat = true;
                animator.SetBool("inCombat", true);
            }

            bonesRotating = false;
        }*/
    }

    private void StandUpBack()
    {
        boneRotTime += Time.deltaTime;

        foreach (Transform bone in bones)
        {
            if (bone.GetComponent<Rigidbody>() != null)
                bone.GetComponent<Rigidbody>().freezeRotation = true;

            if (bone.name.Equals("mixamorig:Hips"))
                bone.localEulerAngles = Vector3.Lerp(
                    bone.localEulerAngles,
                    new Vector3(277.6f, bone.localEulerAngles.y, 92),
                    boneRotTime / 5
                );
        }
    }
}
