using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    protected Rigidbody _rb;
    public float speed;
    protected float _time;
    protected float angleX;
    protected float angleY;
    protected int mSetActive;
    protected TouchField mTouchField;
    protected Transform mCameraHolder;
    protected Vector3 positionOffset;
#if UNITY_ANDROID
    protected FixedJoystick mJoystick;
#endif    
    protected Trigger _back;
    protected Transform mRifle;
    protected Transform mCamera;
    public GameObject characterPref;

    void Start()
    {
        _RagdollStart();
    }

    void Update()
    {
        _RagdollUpdate();
    }

    protected void _RagdollStart()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _back = GetComponentInChildren<Trigger>();
    }

    protected void _RagdollUpdate()
    {
        speed = _rb.linearVelocity.magnitude;

        if (speed <= 1)
        {
            _time += Time.deltaTime;

            if (mCameraHolder == null || (
#if UNITY_ANDROID
               mJoystick.Horizontal != 0 || mJoystick.Vertical != 0 ||
#endif
               //#if UNITY_STANDALONE
               Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0
               //#endif
               ) && _time > 1) //stand up
                TransitionToAnim();
        }
        else
            _time = 0;
    }

    public void TransitionToAnim()
    {
        GameObject ch = Instantiate(characterPref, transform.position, transform.rotation);
        ch.transform.parent = transform.parent;
        ch.GetComponentInChildren<Animator>().enabled = false;

        Transform[] allTransforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name == "WeaponHolder")
            { mRifle = t; break; }
        }

        List<Transform> boneTransforms = new List<Transform>();
        foreach (Transform objTransform in allTransforms)
        {
            if (objTransform.gameObject.name.Contains("mixamorig"))
            {
                boneTransforms.Add(objTransform);

                if (objTransform.name.Contains("Hips"))
                    ch.GetComponent<Rigidbody>().linearVelocity = objTransform.GetComponent<Rigidbody>().linearVelocity;
            }
        }

        Transform chHand = null;
        Transform chRifle = null;
        Transform chHead = null;

        allTransforms = ch.GetComponentsInChildren<Transform>();
        List<Transform> boneCHTransforms = new List<Transform>();
        foreach (Transform objTransform in allTransforms)
        {
            if (objTransform.name.Contains("mixamorig"))
            {
                boneCHTransforms.Add(objTransform);

                if (objTransform.name.Equals("mixamorig:RightHand"))
                    chHand = objTransform;

                if (objTransform.name.Equals("mixamorig:Head"))
                    chHead = objTransform;
            }

            if (objTransform.name.Equals("WeaponHolder"))
                chRifle = objTransform;

            if (objTransform.name.Equals("CamHolder"))
                objTransform.localPosition = new Vector3(0, 0.38f, 0.764f);
        }

        for (int i = 0; i < boneTransforms.Count; i++)
        {
            string nm = boneCHTransforms[i].name;
            if (!(nm.Contains("Hand") || nm.Contains("Foot") || nm.Contains("Toe")))
                boneCHTransforms[i].position = boneTransforms[i].position;
            boneCHTransforms[i].rotation = boneTransforms[i].rotation;
        }

        if (mRifle.parent.name.Equals("mixamorig:RightHand"))
        {
            chRifle.parent = chHand;
            chRifle.position = mRifle.position;
            chRifle.rotation = mRifle.rotation;
        }

        if (mCameraHolder != null)
        {
            // ch.GetComponent<Humanoid>().Setter(positionOffset, mSetActive);

            Transform chCamHolder = ch.transform.Find("CamHolder");
            chCamHolder.position = chHead.position;
            chCamHolder.localPosition = new Vector3(0, chCamHolder.localPosition.y, 0);
            chCamHolder.rotation = mCamera.rotation;
            chCamHolder.Find("Camera").position = mCamera.transform.position;
            Destroy(mCamera.gameObject);
        }
        // else
            // ch.GetComponent<Humanoid>().StartBonesRotationFront();

        Destroy(gameObject);
    }
}
