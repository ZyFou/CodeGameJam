using UnityEngine;

public class CashierDepositButton : MonoBehaviour, IClickable
{
    public RunManager run;

    public void Click(ClickContext ctx)
    {
        run?.DepositToDebt();
    }
}
