using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;
    
    private Vector3 originalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    private void Update()
    {
        // Use unscaledDeltaTime because game is paused
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Tell the manager this card was selected
        UpgradeManager.Instance.SelectUpgrade(this.transform);
    }
}