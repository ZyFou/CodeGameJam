using UnityEngine;

public class StandButton : MonoBehaviour, IClickable
{
    public RunManager run;

    public void Click(ClickContext ctx)
    {
        run?.PressStandButton();
    }
}
