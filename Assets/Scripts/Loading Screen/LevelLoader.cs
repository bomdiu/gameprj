using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    // Singleton: Giúp gọi script này từ bất cứ đâu (nút Start, cổng kết thúc màn...)
    public static LevelLoader instance;

    [Header("Cài đặt")]
    public GameObject loadingScreenPrefab; // Kéo cái Prefab LoadingCanvas vào đây

    private void Awake()
    {
        // Đảm bảo LevelLoader luôn tồn tại xuyên suốt các màn chơi
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm này sẽ được gọi từ nút Start
    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        // 1. Sinh ra màn hình Loading
        GameObject loadingScreen = Instantiate(loadingScreenPrefab);
        DontDestroyOnLoad(loadingScreen);

        // Lấy các thành phần UI
        Slider slider = loadingScreen.GetComponentInChildren<Slider>();
        
        // 2. Bắt đầu tải Scene ngầm
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // QUAN TRỌNG: Ngăn không cho Unity chuyển cảnh ngay khi load xong
        // Nó sẽ dừng lại ở mức 90% (0.9) và chờ lệnh
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        // 3. Vòng lặp xử lý (Chạy cho đến khi tải xong VÀ chạy hết thời gian giả lập)
        // Ở đây mình ép nó chạy ít nhất 3 giây (fakeProgress < 1) thì mới cho qua
        while (!operation.isDone)
        {
            // --- PHẦN LÀM GIẢ TIẾN TRÌNH (FAKE LOADING) ---
            // Tăng fakeProgress từ từ theo thời gian thực (3 giây để đầy cây)
            fakeProgress += Time.deltaTime * 0.33f; // 0.33 nghĩa là mất khoảng 3s để lên được 1

            // Lấy giá trị thực của việc load scene (max là 0.9)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Giá trị hiển thị trên Slider sẽ là số NHỎ HƠN giữa Fake và Real
            // Điều này đảm bảo thanh slider chạy mượt từ 0->1 chứ không nhảy cóc
            float displayProgress = Mathf.Min(fakeProgress, realProgress);

            if (slider != null)
            {
                slider.value = displayProgress;
            }

            // --- ĐIỀU KIỆN KẾT THÚC ---
            // Nếu tải thật đã xong (progress >= 0.9) VÀ thanh slider giả đã chạy đầy
            if (operation.progress >= 0.9f && fakeProgress >= 1f)
            {
                // Cho phép chuyển cảnh
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 4. Dọn dẹp
        Destroy(loadingScreen);
    }
}