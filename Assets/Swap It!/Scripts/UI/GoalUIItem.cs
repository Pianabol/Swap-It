using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUIItem : MonoBehaviour
{
    public ItemType GoalType { get; private set; }  
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ItemType type, int initialAmount, Sprite iconSprite)
    {
        GoalType = type;
        iconImage.sprite = iconSprite;
        UpdateAmount(initialAmount);
    }

    public void UpdateAmount(int newAmount)
    {
        amountText.text = newAmount.ToString();
        
        LeanTween.cancel(amountText.gameObject);
        amountText.transform.localScale = Vector3.one;
    
        LeanTween.scale(amountText.gameObject, Vector3.one * 1.5f, 0.3f).setEasePunch();
    }
}