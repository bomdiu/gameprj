using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;
    
    [Header("Audio Settings")] // MỚI: Thêm âm thanh cho UI
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    private Vector3 originalScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    private SkillData thisSkillData;

    private void Awake()
    {
        // Tự động tìm AudioSource nếu chưa gán
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // Đảm bảo card bắt đầu từ scale hiện tại
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    public void SetData(SkillData data)
    {
        thisSkillData = data;
    }

    private void Update()
    {
        // Sử dụng unscaledDeltaTime vì menu nâng cấp thường làm game tạm dừng
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;

        // MỚI: Phát tiếng "pặc" khi hover
        if (audioSource != null && hoverSFX != null)
        {
            audioSource.PlayOneShot(hoverSFX);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // MỚI: Phát tiếng khi chọn thẻ
        if (audioSource != null && clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX);
        }

        if (thisSkillData != null && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.SelectUpgrade(thisSkillData, this.transform);
        }
    }
}