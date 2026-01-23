using UnityEngine;

public class TablePayButton : MonoBehaviour, IClickable
{
    public RunManager run;

    public void Click(ClickContext ctx)
    {
        run?.TryPayEntryAndStartRound();
    }
}
