#nullable enable
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Yarn.Unity;

public class CustomDialoguePresenter : DialoguePresenterBase
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private CanvasGroup canvasGroup;


    [Header("Effects Settings")]
    [SerializeField] private bool enableFadeEffects = true;   // 是否启用渐入渐出
    [SerializeField] private float fadeInDuration = 0.5f;     // 渐入时间
    [SerializeField] private float fadeOutDuration = 0.5f;    // 渐出时间
    [Header("Typewriter Effect Settings")]
    [SerializeField] private bool enableTypewriter = true;    // 是否启用打字机效果
    [SerializeField] private int lettersPerSecond = 60;       // 每秒多少字
    [SerializeField] private AudioSource typingAudioSource; 
    [SerializeField] private AudioClip typingClip; 
    [SerializeField] [Range(0f, 1f)] private float typingVolume = 0.5f;
    [SerializeField] private float typingSoundInterval = 0.05f; // 每隔多少秒播放一次音效
    

    private float typewriterSpeed {
        get { return 1.0f / lettersPerSecond; }
    }

    [Header("Auto Play Settings")]
    [SerializeField] private bool autoPlay = false;           // 是否自动播放
    [SerializeField] private float autoPlayDelay = 2.0f;      // 自动下一句延迟
    
    [Header("Typing Sound Settings")]

    private float typingSoundTimer = 0f;

    private void PlayTypingSound() {
        if (typingClip == null || typingAudioSource == null) return;

        typingSoundTimer -= Time.deltaTime;
        if (typingSoundTimer <= 0f) {
            typingAudioSource.PlayOneShot(typingClip, typingVolume);
            typingSoundTimer = typingSoundInterval;
        }
    }
    
    
    
    public override async YarnTask OnDialogueStartedAsync() {
        dialoguePanel.SetActive(true);
        // 不等待渐变完成，而是并行执行
        _ = FadeCanvasGroup(0, 1, fadeInDuration);
        await YarnTask.CompletedTask;
    }

    public override async YarnTask OnDialogueCompleteAsync() {
        await FadeCanvasGroup(1, 0, fadeOutDuration);
        dialoguePanel.SetActive(false);
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token) {
        characterNameText.text = line.CharacterName;
        dialogueText.text = "";

        string fullText = line.TextWithoutCharacterName.Text;

        if (enableTypewriter) {
            // 打字机效果
            for (int i = 0; i < fullText.Length; i++) {
                if (token.IsHurryUpRequested) {
                    dialogueText.text = fullText;
                    break;
                }

                dialogueText.text = fullText.Substring(0, i + 1);
                PlayTypingSound();
                await Task.Delay((int)(typewriterSpeed * 1000));
            }
        } else {
            // 直接显示完整文本
            dialogueText.text = fullText;
        }

        // 自动播放 or 等待玩家
        float timer = 0f;
        while (!token.IsNextLineRequested && !token.IsHurryUpRequested) {
            await Task.Yield();

            timer += Time.deltaTime;
            if (autoPlay && timer >= autoPlayDelay) {
                break; // 超时自动进入下一句
            }
        }
    }

    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken) {
        // 简单处理：自动选择第一个选项
        Debug.Log("Options: " + dialogueOptions.Length);
        await Task.Delay(500); // 模拟一点延迟
        return dialogueOptions.Length > 0 ? dialogueOptions[0] : null;
    }

    // 辅助方法：CanvasGroup 渐变
    private async Task FadeCanvasGroup(float from, float to, float duration) {
        float time = 0f;
        while (enableFadeEffects && time < duration) {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            await Task.Yield();
        }
        canvasGroup.alpha = to;
    }
}
