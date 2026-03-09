using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dash : MonoBehaviour
{
    public float dashForce = 50;
    public float dashCd = 1.5f;
    private float dashTimePassed = 0;
    private bool onCd;
    private GameObject dashButton;
    private Image dashSprite,
        dashCdFiller;
    private TMP_Text dashCdText;
    public Sprite dashReadySprite,
        dashNotReadySprite;
    private Links l;

    private void Start()
    {
        l = GetComponent<Links>();
    }

    private void Update()
    {
        if (onCd)
        {
            if (dashTimePassed < dashCd)
            {
                // While on cd
                dashSprite.sprite = dashNotReadySprite;
                dashCdText.text = dashTimePassed.ToString("F1");
                dashCdFiller.fillAmount = dashTimePassed / dashCd;
                dashTimePassed += Time.deltaTime;
            }
            else
            {
                // When cd ends
                dashSprite.sprite = dashReadySprite;
                dashCdText.text = "";
                dashCdFiller.fillAmount = 1;
                onCd = false;
            }
        }
    }

    public void SetUi(Transform canvas)
    {
        dashButton = canvas.Find("Character Ui/Run Button").gameObject;
        dashSprite = dashButton.GetComponent<Image>();
        dashSprite.sprite = dashReadySprite;
        dashCdText = dashButton.transform.Find("Timer").GetComponent<TMP_Text>();
        dashCdFiller = dashButton.transform.Find("Filler").GetComponent<Image>();
        dashCdFiller.fillAmount = 1;
    }

    private void OnEnable()
    {
        if (dashButton)
            dashButton.SetActive(true);
    }

    private void OnDisable()
    {
        if (dashButton)
            dashButton.SetActive(false);
    }

    public void Execute()
    {
        if (onCd)
            return;

        Vector3 joystickWorldMovement;

        if (InputManager.Instance.isMoving)
        {
            Vector3 dashDirection = new Vector3(
                InputManager.Instance.Horizontal,
                0f,
                InputManager.Instance.Vertical
            );
            joystickWorldMovement = l.playerCamera.TransformDirection(dashDirection);
        }
        else
        {
            joystickWorldMovement =
                Quaternion.Euler(0, l.playerCamera.eulerAngles.y, 0) * Vector3.forward;
        }

        joystickWorldMovement.y = 0;
        joystickWorldMovement.Normalize();

        if (l.movement.isGrounded)
            l.rb.linearVelocity += joystickWorldMovement * dashForce;
        else
            l.rb.linearVelocity += joystickWorldMovement * dashForce / 10;

        // Rotate character towards direction
        Quaternion targetRotation = Quaternion.LookRotation(joystickWorldMovement);
        transform.rotation = targetRotation;

        dashTimePassed = 0;

        onCd = true;
    }
}
