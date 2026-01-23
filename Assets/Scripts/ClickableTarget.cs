using UnityEngine;

public class ClickableTarget : MonoBehaviour
{
    public bool isClicked;

    public void Click()
    {
        isClicked = true;
        Debug.Log($"{name} clicked => isClicked = true");
    }

    // (optionnel) pour reset à la demande
    public void ResetClicked()
    {
        isClicked = false;
    }
}
