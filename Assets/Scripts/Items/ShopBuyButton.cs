using UnityEngine;

public class ShopBuyButton : MonoBehaviour, IClickable
{
    public ShopManager shop;
    public int index;

    public void Click(ClickContext ctx)
    {
        shop?.Buy(index);
    }
}
