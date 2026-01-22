using UnityEngine;
using UnityEngine.Audio; // Cần thư viện này để dùng AudioMixer
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Cài đặt")]
    public AudioMixer audioMixer; // Kéo Audio Mixer vào đây
    public Slider volumeSlider;   // Kéo thanh Slider vào đây

    void Start()
    {
        // Khi game bắt đầu, đặt giá trị của slider khớp với âm lượng hiện tại
        // (Phần nâng cao: Lấy từ PlayerPrefs nếu có lưu)
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        // Công thức chuyển đổi từ 0-1 sang Decibel (dB)
        // Log10 giúp thanh trượt nghe tự nhiên với tai người hơn
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }
}