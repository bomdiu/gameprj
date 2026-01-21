using UnityEngine;
using System.Collections;

public class BackgroundController : MonoBehaviour
{
    [Header("Cài đặt")]
    public Animator bgAnimator; // Kéo component Animator của Background vào đây
    public string triggerName = "PlayAnim"; // Tên Trigger đã đặt trong Animator
    
    [Header("Thời gian chờ (Giây)")]
    public float minWaitTime = 3f; // Chờ ít nhất 3 giây
    public float maxWaitTime = 8f; // Chờ tối đa 8 giây

    void Start()
    {
        // Bắt đầu quy trình lặp lại
        StartCoroutine(AnimateRoutine());
    }

    IEnumerator AnimateRoutine()
    {
        while (true) // Lặp vô tận
        {
            // 1. Random thời gian chờ
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            
            // 2. Đợi hết thời gian này (Lúc này Background đang ở state Idle)
            yield return new WaitForSeconds(waitTime);

            // 3. Kích hoạt Animation
            if (bgAnimator != null)
            {
                bgAnimator.SetTrigger(triggerName);
            }

            // 4. Đợi một chút để tránh trigger bị gọi chồng chéo (optional)
            // Lấy độ dài animation để đợi thì càng tốt, nhưng đơn giản thì cứ để vòng lặp tự xử lý
            // vì transition Action -> Idle cần thời gian để chạy xong.
            yield return new WaitForSeconds(1f); 
        }
    }
}