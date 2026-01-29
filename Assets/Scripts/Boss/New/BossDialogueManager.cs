using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BossDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI bodyText;
    
    // THÊM: Tham chiếu đến script rung lắc vừa tạo
    public TextShaker textShakerScript; 

    [Header("Cấu hình Màu Sắc")]
    public Color playerColor = Color.cyan;
    public Color bossColor = Color.red;

    [Header("Audio (SFX)")]
    public AudioSource audioSource;
    public AudioClip typingSFX;      // Kéo file âm thanh gõ chữ vào đây
    [Range(0.1f, 2f)] public float pitchVariation = 0.1f; // Độ biến thiên cao độ cho tự nhiên

    [Header("Cài đặt")]
    public float defaultTypingSpeed = 0.04f;

    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(3, 5)] public string content;
        public float speedMultiplier;
    }

    [Header("Nội dung Hội Thoại")]
    public List<DialogueLine> conversation;

    [Header("Sự kiện sau khi nói xong")]
    public UnityEvent OnDialogueFinished;

    private int index = 0;
    private bool isTyping = false;

    void Start()
    {
        // Tự động tìm AudioSource nếu quên kéo
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue()
    {
        dialogueBox.SetActive(true);
        index = 0;
        StartCoroutine(TypeLine(conversation[index]));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        nameText.text = line.speakerName;

        // --- LOGIC MỚI: KIỂM TRA PHẢI BOSS KHÔNG? ---
        bool isBoss = line.speakerName.Contains("?") || line.speakerName.Contains("Despair");

        if (isBoss)
        {
            nameText.color = bossColor;
            // BẬT hiệu ứng rung chữ cho Boss
            if (textShakerScript != null) textShakerScript.enabled = true;
        }
        else
        {
            nameText.color = playerColor;
            // TẮT hiệu ứng rung chữ cho Player
            if (textShakerScript != null) textShakerScript.enabled = false;
        }

        bodyText.text = line.content;
        bodyText.maxVisibleCharacters = 0;

        yield return null;

        int totalChars = bodyText.textInfo.characterCount;
        float waitTime = defaultTypingSpeed / line.speedMultiplier;

        for (int i = 0; i <= totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i;

            // --- LOGIC MỚI: PHÁT ÂM THANH ---
            // Chỉ phát mỗi 2 ký tự 1 lần để đỡ đau đầu (i % 2 == 0)
            // Và chỉ phát khi chưa hiện hết câu
            if (i < totalChars && i % 2 == 0 && typingSFX != null && audioSource != null)
            {
                // Thay đổi pitch ngẫu nhiên 1 chút để nghe giống tiếng gõ tự nhiên hơn
                audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
                audioSource.PlayOneShot(typingSFX);
            }

            yield return new WaitForSeconds(waitTime);
        }

        isTyping = false;
    }

    void NextLine()
    {
        index++;
        if (index < conversation.Count)
        {
            StartCoroutine(TypeLine(conversation[index]));
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false);
        // Tắt rung luôn cho chắc
        if (textShakerScript != null) textShakerScript.enabled = false;
        OnDialogueFinished.Invoke();
    }
}