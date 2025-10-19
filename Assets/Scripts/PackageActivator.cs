using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemPosition
{
    public Transform item;
    public Vector3 initialPalletPosition;
    public Quaternion initialPalletRotation;
    public Vector3 initialCardboardPosition;
    public Quaternion initialCardboardRotation;
}

public class PackageActivator : MonoBehaviour
{
    public GameObject itemsContainer;
    [SerializeField] public List<ItemPosition> itemPositions = new List<ItemPosition>();

    void Awake()
    {
        if (itemsContainer != null)
        {
            foreach (Transform item in itemsContainer.transform)
            {
                Transform pallet = item.Find("Pallet");
                Transform cardboard = item.Find("Cardboard_A");

                if (pallet != null && cardboard != null)
                {
                    pallet.gameObject.SetActive(false);
                    cardboard.gameObject.SetActive(false);

                    itemPositions.Add(new ItemPosition
                    {
                        item = item,
                        initialPalletPosition = pallet.localPosition,
                        initialPalletRotation = pallet.localRotation,
                        initialCardboardPosition = cardboard.localPosition,
                        initialCardboardRotation = cardboard.localRotation
                    });
                }
            }
        }
    }

    public void ActivatePackage(int index)
    {
        if (itemsContainer != null && index >= 0 && index < itemsContainer.transform.childCount)
        {
            foreach (Transform item in itemsContainer.transform)
            {
                item.gameObject.SetActive(false);
                foreach (Transform child in item)
                {
                    child.gameObject.SetActive(false);
                }
            }

            Transform selectedItem = itemsContainer.transform.GetChild(index);
            selectedItem.gameObject.SetActive(true);

            foreach (Transform child in selectedItem)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void ResetPalletPositions()
    {
        foreach (var itemPosition in itemPositions)
        {
            if (itemPosition.item != null)
            {
                itemPosition.item.gameObject.SetActive(false);

                Transform pallet = itemPosition.item.Find("Pallet");
                if (pallet != null)
                {
                    pallet.localPosition = itemPosition.initialPalletPosition;
                    pallet.localRotation = itemPosition.initialPalletRotation;
                    pallet.gameObject.SetActive(false);
                }

                Transform cardboard = itemPosition.item.Find("Cardboard_A");
                if (cardboard != null)
                {
                    cardboard.localPosition = itemPosition.initialCardboardPosition;
                    cardboard.localRotation = itemPosition.initialCardboardRotation;
                    cardboard.gameObject.SetActive(false);
                }
            }
        }
    }
}
