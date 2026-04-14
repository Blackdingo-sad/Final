using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ShopController.Instance == null) return;

        ShopSlot shopSlot = GetComponentInParent<ShopSlot>();
        if (shopSlot == null) return;

        if (shopSlot.isShopSlot)
        {
            //ShopController.Instance.BuyItem(shopSlot);
        }
        else
        {
            //ShopController.Instance.SellItem(shopSlot);
        }
    }
}
