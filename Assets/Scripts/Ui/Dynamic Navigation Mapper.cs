using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicNavigationMapper : MonoBehaviour
{
    [SerializeField]
    private Selectable selectableOnUp,
        selectableOnDown;

    void Start()
    {
        UpdateNavigation();
    }

    public void UpdateNavigation()
    {
        Selectable[] selectables = GetComponentsInChildren<Selectable>(true)
            .Where(s => s.gameObject != gameObject)
            .ToArray();

        if (selectables.Length == 0)
        {
            LinkExternalDirectly();
            return;
        }

        for (int i = 0; i < selectables.Length; i++)
        {
            Navigation nav = new() { mode = Navigation.Mode.Explicit };

            if (i > 0)
            {
                nav.selectOnUp = selectables[i - 1];
            }
            else if (selectableOnUp != null)
            {
                nav.selectOnUp = selectableOnUp;
                Navigation upNav = selectableOnUp.navigation;
                upNav.selectOnDown = selectables[i];
                selectableOnUp.navigation = upNav;
            }

            if (i < selectables.Length - 1)
            {
                nav.selectOnDown = selectables[i + 1];
            }
            else if (selectableOnDown != null)
            {
                nav.selectOnDown = selectableOnDown;
                Navigation downNav = selectableOnDown.navigation;
                downNav.selectOnUp = selectables[i];
                selectableOnDown.navigation = downNav;
            }

            selectables[i].navigation = nav;
        }
    }

    public void SelectFirst()
    {
        GetComponentInChildren<Selectable>(true).Select();
    }

    private void LinkExternalDirectly()
    {
        if (selectableOnUp == null || selectableOnDown == null)
            return;

        Navigation upNav = selectableOnUp.navigation;
        upNav.selectOnDown = selectableOnDown;
        selectableOnUp.navigation = upNav;

        Navigation downNav = selectableOnDown.navigation;
        downNav.selectOnUp = selectableOnUp;
        selectableOnDown.navigation = downNav;
    }
}
