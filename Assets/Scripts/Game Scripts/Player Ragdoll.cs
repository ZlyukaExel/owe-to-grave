using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerRagdollControl : Ragdoll
{
    private float mHorizontalSens;
    private float mVerticalSens;
    private float cameraSpeed = 10;
    private float mMaxPitch = 80;
    private float mMinPitch = 5;
    private float cameraMovingTime = 0;
    private int defaultLayerId;
    private int itemsLayerId;
    private bool mFirstPerson;
    private Vector3 mAngleOffset = Vector3.zero;
    private Minimap minimap;


    void Start()
    {
        _RagdollStart();

        defaultLayerId = 1 << LayerMask.NameToLayer("Default");
        itemsLayerId = 1 << LayerMask.NameToLayer("Items");

        mHorizontalSens = PlayerPrefs.GetFloat("HorizontalSensivity");
        mVerticalSens = PlayerPrefs.GetFloat("VerticalSensivity");
    }

    void Update()
    {
        _RagdollUpdate();
        minimap.angleY = angleY;
    }

    private void LateUpdate()
    {
        FollowFreeRotation();
    }

    void FollowFreeRotation()
    {
        float mx, my;

        if (mFirstPerson)
        {
            mCamera.rotation = Quaternion.Slerp(mCamera.rotation, mCameraHolder.rotation, Time.deltaTime * 2);
            cameraMovingTime += Time.deltaTime;
            mCamera.position = Vector3.Slerp(mCamera.position, mCameraHolder.position, cameraMovingTime);
        }
        else
        {
            {
#if UNITY_STANDALONE
                mx = Input.GetAxis("Mouse X");
                my = Input.GetAxis("Mouse Y");
#endif
#if UNITY_ANDROID
                mx = mTouchField.TouchDist.x * Time.deltaTime;
                my = mTouchField.TouchDist.y * Time.deltaTime;
#endif
                // Rotate camera by mouse/touchpad
                Quaternion initialRotation = Quaternion.Euler(mAngleOffset);
                Vector3 eu = mCamera.rotation.eulerAngles;
                angleX -= my * mVerticalSens;
                angleY += mx * mHorizontalSens;

                // Camera angle borders
                angleX = Mathf.Clamp(angleX, mMinPitch, mMaxPitch);

                //Rotate camera
                mCamera.rotation = Quaternion.Euler(angleX, angleY, 0) * initialRotation;
            }

            //Lock camera behind the player
            {
                Vector3 forward = mCamera.rotation * Vector3.forward;
                Vector3 right = mCamera.rotation * Vector3.right;
                Vector3 up = mCamera.rotation * Vector3.up;

                Vector3 targetPos = mCameraHolder.position;
                Vector3 desiredPosition = targetPos
                    + forward * positionOffset.z
                    + right * positionOffset.x
                    + up * positionOffset.y;

                Vector3 start = targetPos + up;
                Vector3 end = desiredPosition + up;

                //Prevent collision enter
                RaycastHit hit;
                Vector3 direction = desiredPosition - targetPos;
                Debug.DrawRay(targetPos, direction, Color.yellow);
                float mCollisionRadius = 0.4f;
                if (Physics.SphereCast(mCameraHolder.position, mCollisionRadius, direction.normalized, out hit, direction.magnitude, defaultLayerId | itemsLayerId))
                {
                    if (!hit.collider.CompareTag("Interactable"))
                        desiredPosition = hit.point + hit.normal * mCollisionRadius;
                }

                mCamera.position = Vector3.Slerp(mCamera.position, desiredPosition, cameraSpeed * Time.deltaTime);
            }
        }
    }

    public void Setter(TouchField touchField, Vector3 pos, int set, FixedJoystick joystick, Minimap minimap)
    {
        mCameraHolder = transform.Find("Armature/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head");
        mTouchField = touchField;
        positionOffset = pos;
        if (positionOffset.z == 0)
        {
            mFirstPerson = true;
            /*foreach (SkinnedMeshRenderer child in transform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (child.name.Equals("Head") || child.name.Equals("Hair") || child.name.Equals("Neck"))
                    child.enabled = false;
            }*/
        }
        mSetActive = set;
#if UNITY_ANDROID
        mJoystick = joystick;
#endif
        mCamera = transform.parent.Find("Camera");
        this.minimap = minimap;
        minimap.SetTarget(transform.Find("Armature/mixamorig:Hips"));
    }
}
