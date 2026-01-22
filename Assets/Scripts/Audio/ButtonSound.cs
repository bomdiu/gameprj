using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc để dùng tính năng phát hiện chuột

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Cài đặt nguồn phát")]
    public AudioSource sfxSource; // Kéo Audio Source SFX vào đây

    [Header("File âm thanh")]
    public AudioClip hoverSound; // Kéo file âm thanh Hover vào đây
    public AudioClip clickSound; // Kéo file âm thanh Click vào đây

    // Hàm này tự động chạy khi chuột đi vào nút (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && sfxSource != null)
        {
            // PlayOneShot cho phép âm thanh chồng lên nhau mà không bị ngắt quãng
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    // Hàm này tự động chạy khi bấm chuột (Click) - Tiện thể làm luôn
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}