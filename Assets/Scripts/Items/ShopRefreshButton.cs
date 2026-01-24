using UnityEngine;

public class ShopRefreshButton : MonoBehaviour, IClickable
{
    public ShopManager shop;

    public void Click(ClickContext ctx)
    {
        shop?.RefreshShop();
    }
}
