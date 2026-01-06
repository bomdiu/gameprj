using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCardUI : MonoBehaviour
{
    // ... (Các khai báo UI cũ giữ nguyên) ...
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button cardButton;

    private SkillData _data;

    public void Setup(SkillData data)
    {
        _data = data;
        
        // Gán hình ảnh, text như cũ
        titleText.text = data.skillName;
        descText.text = data.description;

        // Xóa sự kiện cũ và thêm sự kiện click mới
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardSelected);
    }

    private void OnCardSelected()
    {
        if (_data == null) return;

        // 1. Gửi lệnh nâng cấp sang nhân vật
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ApplyUpgrade(_data.upgradeType, _data.valueAmount);
        }
        else
        {
            Debug.LogError("Ko tìm thấy PlayerStats! gắn script vào nhân vật ddi");
        }

        // 2. Đóng bảng nâng cấp và tiếp tục game
        UpgradeManager.Instance.CloseUpgradePanel();
    }
}