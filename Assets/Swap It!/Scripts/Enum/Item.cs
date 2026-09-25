using UnityEngine;

public enum ItemType
{
    Red,
    Green,
    Blue,
    Yellow
}

public class Item : MonoBehaviour
{
    [SerializeField] private ItemType itemType;

    public ItemType ItemType => itemType;
}