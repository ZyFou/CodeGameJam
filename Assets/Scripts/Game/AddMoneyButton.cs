using UnityEngine;

public class AddMoneyButton : MonoBehaviour, IClickable
{
    public RunManager run;
    public int wantedAmount = 10; // ex: 5 * tour, ou 10, etc.

    public void Click(ClickContext ctx)
    {
        if (run == null) return;
        run.InsertMoney(wantedAmount);
    }
}
