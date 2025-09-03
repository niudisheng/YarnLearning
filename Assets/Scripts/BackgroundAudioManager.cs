using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// ADV 背景和音效管理器
/// </summary>
public class BackgroundAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundData
    {
        public string backgroundName;
        public Sprite backgroundSprite;
        public AudioClip backgroundMusic;
    }
    
    [System.Serializable]
    public class SoundEffectData
    {
        public string soundName;
        public AudioClip audioClip;
        [Range(0f, 1f)] public float volume = 1f;
    }
    
    [Header("背景设置")]
    public Image backgroundImage;
    public Image overlayImage;  // 用于淡入淡出效果
    public BackgroundData[] backgrounds;
    
    [Header("音效设置")]
    public AudioSource bgmSource;  // 背景音乐
    public AudioSource sfxSource;  // 音效
    public SoundEffectData[] soundEffects;
    
    [Header("过渡效果设置")]
    public float defaultFadeTime = 1f;
    public Ease fadeEase = Ease.InOutQuad;
    
    // 私有变量
    private Dictionary<string, BackgroundData> backgroundDatabase = new Dictionary<string, BackgroundData>();
    private Dictionary<string, SoundEffectData> soundDatabase = new Dictionary<string, SoundEffectData>();
    private Coroutine currentBGMFade;
    
    void Start()
    {
        InitializeDatabases();
        InitializeOverlay();
    }
    
    void InitializeDatabases()
    {
        // 初始化背景数据库
        foreach (var bg in backgrounds)
        {
            backgroundDatabase[bg.backgroundName] = bg;
        }
        
        // 初始化音效数据库
        foreach (var sfx in soundEffects)
        {
            soundDatabase[sfx.soundName] = sfx;
        }
    }
    
    void InitializeOverlay()
    {
        if (overlayImage != null)
        {
            overlayImage.color = new Color(0f, 0f, 0f, 0f);
        }
    }
    
    #region 背景管理
    
    [YarnCommand("change_background")]
    public static void ChangeBackground(string backgroundName, float fadeTime = 1f)
    {
        
        FindAnyObjectByType<BackgroundAudioManager>()?.ChangeBackgroundInternal(backgroundName, fadeTime);
    }
    
    public void ChangeBackgroundInternal(string backgroundName, float fadeTime)
    {
        StartCoroutine(ChangeBackgroundCoroutine(backgroundName, fadeTime));
    }
    
    IEnumerator ChangeBackgroundCoroutine(string backgroundName, float fadeTime)
    {
        if (!backgroundDatabase.TryGetValue(backgroundName, out BackgroundData bgData))
        {
            Debug.LogError($"找不到背景: {backgroundName}");
            yield break;
        }
        
        // 淡出当前背景
        if (overlayImage != null)
        {
            yield return overlayImage.DOFade(1f, fadeTime / 2f).SetEase(fadeEase).WaitForCompletion();
        }
        
        // 切换背景
        if (backgroundImage != null)
        {
            backgroundImage.sprite = bgData.backgroundSprite;
        }
        
        // 切换背景音乐
        if (bgData.backgroundMusic != null)
        {
            yield return StartCoroutine(ChangeBGMCoroutine(bgData.backgroundMusic, fadeTime));
        }
        
        // 淡入新背景
        if (overlayImage != null)
        {
            yield return overlayImage.DOFade(0f, fadeTime / 2f).SetEase(fadeEase).WaitForCompletion();
        }
    }
    
    [YarnCommand("fade_to_black")]
    public static void FadeToBlack(float fadeTime = 1f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.FadeToBlackInternal(fadeTime);
    }
    
    public void FadeToBlackInternal(float fadeTime)
    {
        if (overlayImage != null)
        {
            overlayImage.DOFade(1f, fadeTime).SetEase(fadeEase);
        }
    }
    
    [YarnCommand("fade_from_black")]
    public static void FadeFromBlack(float fadeTime = 1f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.FadeFromBlackInternal(fadeTime);
    }
    
    public void FadeFromBlackInternal(float fadeTime)
    {
        if (overlayImage != null)
        {
            overlayImage.DOFade(0f, fadeTime).SetEase(fadeEase);
        }
    }
    
    #endregion
    
    #region 音效管理
    
    [YarnCommand("play_sound")]
    public static void PlaySound(string soundName, float volume = 1f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.PlaySoundInternal(soundName, volume);
    }
    
    public void PlaySoundInternal(string soundName, float volume)
    {
        if (!soundDatabase.TryGetValue(soundName, out SoundEffectData soundData))
        {
            Debug.LogWarning($"找不到音效: {soundName}");
            return;
        }
        
        if (sfxSource != null && soundData.audioClip != null)
        {
            sfxSource.PlayOneShot(soundData.audioClip, soundData.volume * volume);
        }
    }
    
    [YarnCommand("play_bgm")]
    public static void PlayBGM(string musicName, float fadeTime = 2f)
    {
        var manager = FindAnyObjectByType<BackgroundAudioManager>();
        if (manager != null)
        {
            // 尝试从背景数据中找到音乐
            if (manager.backgroundDatabase.TryGetValue(musicName, out BackgroundData bgData) && 
                bgData.backgroundMusic != null)
            {
                manager.StartCoroutine(manager.ChangeBGMCoroutine(bgData.backgroundMusic, fadeTime));
            }
            else
            {
                Debug.LogWarning($"找不到背景音乐: {musicName}");
            }
        }
    }
    
    [YarnCommand("stop_bgm")]
    public static void StopBGM(float fadeTime = 2f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.StopBGMInternal(fadeTime);
    }
    
    public void StopBGMInternal(float fadeTime)
    {
        StartCoroutine(StopBGMCoroutine(fadeTime));
    }
    
    IEnumerator StopBGMCoroutine(float fadeTime)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            yield return bgmSource.DOFade(0f, fadeTime).WaitForCompletion();
            bgmSource.Stop();
            bgmSource.volume = 1f; // 重置音量
        }
    }
    
    IEnumerator ChangeBGMCoroutine(AudioClip newClip, float fadeTime)
    {
        if (currentBGMFade != null)
        {
            StopCoroutine(currentBGMFade);
        }
        
        if (bgmSource.isPlaying)
        {
            // 淡出当前音乐
            yield return bgmSource.DOFade(0f, fadeTime / 2f).WaitForCompletion();
        }
        
        // 切换音乐
        bgmSource.clip = newClip;
        bgmSource.Play();
        
        // 淡入新音乐
        yield return bgmSource.DOFade(1f, fadeTime / 2f).WaitForCompletion();
    }
    
    #endregion
    
    #region 屏幕效果
    
    [YarnCommand("screen_flash")]
    public static void ScreenFlash(string colorHex = "FFFFFF", float flashTime = 0.1f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.ScreenFlashInternal(colorHex, flashTime);
    }
    
    public void ScreenFlashInternal(string colorHex, float flashTime)
    {
        StartCoroutine(ScreenFlashCoroutine(colorHex, flashTime));
    }
    
    IEnumerator ScreenFlashCoroutine(string colorHex, float flashTime)
    {
        if (overlayImage == null) yield break;
        
        // 解析颜色
        Color flashColor = Color.white;
        if (ColorUtility.TryParseHtmlString("#" + colorHex, out Color parsedColor))
        {
            flashColor = parsedColor;
        }
        
        // 设置颜色并闪烁
        overlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        
        var sequence = DOTween.Sequence();
        sequence.Append(overlayImage.DOFade(1f, flashTime / 2f));
        sequence.Append(overlayImage.DOFade(0f, flashTime / 2f));
        
        yield return sequence.WaitForCompletion();
        
        // 重置为黑色
        overlayImage.color = new Color(0f, 0f, 0f, 0f);
    }
    
    [YarnCommand("screen_shake")]
    public static void ScreenShake(float intensity = 10f, float duration = 0.5f)
    {
        FindAnyObjectByType<BackgroundAudioManager>()?.ScreenShakeInternal(intensity, duration);
    }
    
    public void ScreenShakeInternal(float intensity, float duration)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity, duration));
    }
    
    IEnumerator ScreenShakeCoroutine(float intensity, float duration)
    {
        if (backgroundImage != null)
        {
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            yield return bgRect.DOShakeAnchorPos(duration, intensity, 10, 90f).WaitForCompletion();
        }
    }
    
    #endregion
}