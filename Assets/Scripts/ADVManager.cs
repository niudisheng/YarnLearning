using System;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

/// <summary>
/// ADV 系统的核心管理器，处理立绘显示、移动、差分切换等功能
/// </summary>
public class ADVManager : MonoBehaviour
{
    public DialogueRunner dialogueRunner; // 对话系统
    [Header("立绘系统设置")] public Transform characterContainer; // 立绘的父容器
    public Canvas mainCanvas; // 主画布
    public AudioSource typewriterSound; // 打字机声音
    [Header("角色数据")] public CharacterData[] characters;
    public GameObject characterPrefab; // 立绘预制体
    [Header("演出设置")] public float defaultMoveSpeed = 1f;
    public float defaultFadeSpeed = 0.5f;
    public Ease defaultEaseType = Ease.OutQuad;

    [Header("文字显示设置")] public TextMeshProUGUI dialogueText; // 对话文本组件

    // 私有变量
    private Dictionary<string, GameObject> activeCharacters = new Dictionary<string, GameObject>();
    private Dictionary<string, CharacterData> characterDatabase = new Dictionary<string, CharacterData>();
    private Coroutine currentTypewriter;

    private void Awake()
    {
        RegisterYarnCommands();
    }

    void Start()
    {
        InitializeCharacterDatabase();
    }

    void InitializeCharacterDatabase()
    {
        foreach (var character in characters)
        {
            characterDatabase[character.characterName] = character;
        }
    }

    void RegisterYarnCommands()
    {
        // 注册 Yarn 命令
        // 注意：这些方法需要是静态的，或者使用手动注册方式
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();

        // 监听Yarn的文本显示事件
        if (dialogueRunner != null)
        {
            // 假设你使用的是Yarn Spinner，监听文本更新事件
            // 这里需要根据你使用的Yarn版本调整事件名称
            if (dialogueRunner.onDialogueStart == null || dialogueRunner.onDialogueComplete == null)
            {
                Debug.LogWarning("请检查Yarn版本，可能需要调整事件名称");
            }
            else
            {
                Debug.Log("已注册Yarn文本显示事件");
                // dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
                // dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            }
        }
    }

    #region 立绘显示和管理

    [YarnCommand("show_character")]
    public static void ShowCharacter(string characterName, string position = "center", string expression = "normal",
        float fadeTime = 0.5f)
    {
        FindAnyObjectByType<ADVManager>()?.ShowCharacterInternal(characterName, position, expression, fadeTime);
    }

    public void ShowCharacterInternal(string characterName, string position, string expression, float fadeTime)
    {
        StartCoroutine(ShowCharacterCoroutine(characterName, position, expression, fadeTime));
    }

    IEnumerator ShowCharacterCoroutine(string characterName, string pos, string expression, float fadeTime)
    {
        // 如果角色已存在，直接切换表情
        if (activeCharacters.ContainsKey(characterName))
        {
            yield return StartCoroutine(ChangeExpressionCoroutine(characterName, expression, fadeTime));
            yield break;
        }

        // 获取角色数据
        if (!characterDatabase.TryGetValue(characterName, out CharacterData characterData))
        {
            Debug.LogError($"找不到角色数据: {characterName}");
            yield break;
        }

        // 创建角色立绘对象
        GameObject characterObj = Instantiate(characterPrefab);
        characterObj.transform.SetParent(characterContainer);

        // 添加 Image 组件
        Image characterImage = characterObj.GetComponent<Image>();
        if (characterImage == null)
        {
            characterImage = characterObj.AddComponent<Image>();
        }

        characterImage.sprite = GetFgSprite(characterData, expression);
        characterImage.color = new Color(1f, 1f, 1f, 0f); // 开始时透明

        // 设置位置和缩放
        RectTransform rectTransform = characterObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = GetPositionFromString(pos);
        rectTransform.localScale = Vector3.one * characterData.defaultScale;

        // 添加到活跃角色列表
        activeCharacters[characterName] = characterObj;

        // 淡入效果
        characterImage.DOFade(1f, fadeTime).SetEase(defaultEaseType);

        yield return new WaitForSeconds(fadeTime);
    }

    [YarnCommand("hide_character")]
    public static void HideCharacter(string characterName, float fadeTime = 0.5f)
    {
        FindAnyObjectByType<ADVManager>()?.HideCharacterInternal(characterName, fadeTime);
    }

    public void HideCharacterInternal(string characterName, float fadeTime)
    {
        StartCoroutine(HideCharacterCoroutine(characterName, fadeTime));
    }

    IEnumerator HideCharacterCoroutine(string characterName, float fadeTime)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        Image characterImage = characterObj.GetComponent<Image>();

        // 淡出效果
        yield return characterImage.DOFade(0f, fadeTime).WaitForCompletion();

        // 移除对象
        activeCharacters.Remove(characterName);
        Destroy(characterObj);
    }

    #endregion

    #region 立绘移动和动画

    [YarnCommand("move_character")]
    public static void MoveCharacter(string characterName, string targetPosition, float moveTime = 1f)
    {
        FindAnyObjectByType<ADVManager>()?.MoveCharacterInternal(characterName, targetPosition, moveTime);
    }

    public void MoveCharacterInternal(string characterName, string targetPosition, float moveTime)
    {
        StartCoroutine(MoveCharacterCoroutine(characterName, targetPosition, moveTime));
    }

    IEnumerator MoveCharacterCoroutine(string characterName, string targetPosition, float moveTime)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        RectTransform rectTransform = characterObj.GetComponent<RectTransform>();
        Vector2 targetPos = GetPositionFromString(targetPosition);

        yield return rectTransform.DOAnchorPos(targetPos, moveTime).SetEase(defaultEaseType).WaitForCompletion();
    }

    [YarnCommand("scale_character")]
    public static void ScaleCharacter(string characterName, float targetScale, float scaleTime = 1f)
    {
        FindAnyObjectByType<ADVManager>()?.ScaleCharacterInternal(characterName, targetScale, scaleTime);
    }

    public void ScaleCharacterInternal(string characterName, float targetScale, float scaleTime)
    {
        StartCoroutine(ScaleCharacterCoroutine(characterName, targetScale, scaleTime));
    }

    IEnumerator ScaleCharacterCoroutine(string characterName, float targetScale, float scaleTime)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        Transform transform = characterObj.transform;
        Vector3 targetScaleVector = Vector3.one * targetScale;

        yield return transform.DOScale(targetScaleVector, scaleTime).SetEase(defaultEaseType).WaitForCompletion();
    }

    #endregion

    #region 表情差分切换

    [YarnCommand("change_expression")]
    public static void ChangeExpression(string characterName, string expression, float fadeTime = 0.3f)
    {
        FindAnyObjectByType<ADVManager>()?.ChangeExpressionInternal(characterName, expression, fadeTime);
    }

    public void ChangeExpressionInternal(string characterName, string expression, float fadeTime)
    {
        StartCoroutine(ChangeExpressionCoroutine(characterName, expression, fadeTime));
    }

    IEnumerator ChangeExpressionCoroutine(string characterName, string expression, float fadeTime)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        if (!characterDatabase.TryGetValue(characterName, out CharacterData characterData))
        {
            Debug.LogError($"找不到角色数据: {characterName}");
            yield break;
        }

        Image characterImage = characterObj.GetComponent<Image>();
        Sprite newSprite = GetFgSprite(characterData, expression);

        if (newSprite == null)
        {
            Debug.LogWarning($"找不到表情: {expression} for {characterName}");
            yield break;
        }

        // 创建淡入淡出效果
        yield return characterImage.DOFade(0f, fadeTime / 2f).WaitForCompletion();
        characterImage.sprite = newSprite;
        yield return characterImage.DOFade(1f, fadeTime / 2f).WaitForCompletion();
    }

    #endregion

    #region 演出效果

    [YarnCommand("shake_character")]
    public static void ShakeCharacter(string characterName, float intensity = 10f, float duration = 0.5f)
    {
        FindAnyObjectByType<ADVManager>()?.ShakeCharacterInternal(characterName, intensity, duration);
    }

    public void ShakeCharacterInternal(string characterName, float intensity, float duration)
    {
        StartCoroutine(ShakeCharacterCoroutine(characterName, intensity, duration));
    }

    IEnumerator ShakeCharacterCoroutine(string characterName, float intensity, float duration)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        RectTransform rectTransform = characterObj.GetComponent<RectTransform>();
        yield return rectTransform.DOShakeAnchorPos(duration, intensity).WaitForCompletion();
    }

    [YarnCommand("bounce_character")]
    public static void BounceCharacter(string characterName, float bounceHeight = 50f, float duration = 0.5f)
    {
        FindAnyObjectByType<ADVManager>()?.BounceCharacterInternal(characterName, bounceHeight, duration);
    }

    public void BounceCharacterInternal(string characterName, float bounceHeight, float duration)
    {
        StartCoroutine(BounceCharacterCoroutine(characterName, bounceHeight, duration));
    }

    IEnumerator BounceCharacterCoroutine(string characterName, float bounceHeight, float duration)
    {
        if (!activeCharacters.TryGetValue(characterName, out GameObject characterObj))
        {
            Debug.LogWarning($"角色 {characterName} 不在场景中");
            yield break;
        }

        RectTransform rectTransform = characterObj.GetComponent<RectTransform>();
        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector2 bouncePos = originalPos + Vector2.up * bounceHeight;

        var sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(bouncePos, duration / 2f).SetEase(Ease.OutQuad));
        sequence.Append(rectTransform.DOAnchorPos(originalPos, duration / 2f).SetEase(Ease.InQuad));

        yield return sequence.WaitForCompletion();
    }

    #endregion


    #region 打字机效果

    [YarnCommand("typewriter")]
    public static void StartTypewriter(string text, float speed = 0.05f)
    {
        FindAnyObjectByType<ADVManager>()?.StartTypewriterInternal(text, speed);
    }

    public void StartTypewriterInternal(string text, float speed)
    {
        if (currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        currentTypewriter = StartCoroutine(TypewriterEffect(text, speed));
    }

    IEnumerator TypewriterEffect(string text, float speed)
    {
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;

            // 播放打字音效
            if (typewriterSound != null && letter != ' ')
            {
                typewriterSound.pitch = Random.Range(0.8f, 1.2f);
                typewriterSound.Play();
            }

            yield return new WaitForSeconds(speed);
        }

        currentTypewriter = null;
    }

    [YarnCommand("pause")]
    public static void Pause(float duration)
    {
        FindAnyObjectByType<ADVManager>()?.StartCoroutine(PauseCoroutine(duration));
    }

    static IEnumerator PauseCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    [YarnCommand("wait_for_click")]
    public static void WaitForClick()
    {
        FindAnyObjectByType<ADVManager>()?.StartCoroutine(WaitForClickCoroutine());
    }

    static IEnumerator WaitForClickCoroutine()
    {
        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
    }


    [YarnCommand("pause_dialogue")]
    public static void PauseDialogue(float duration)
    {
        ADVManager advManager = FindAnyObjectByType<ADVManager>();
        if (advManager != null)
        {
            advManager.StartCoroutine(advManager.PauseDialogueCoroutine(duration));
        }
    }

    public IEnumerator PauseDialogueCoroutine(float duration)
    {
        // 暂停Yarn对话系统
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.Stop();

            yield return new WaitForSeconds(duration);

            // 可以在这里添加恢复对话的逻辑
            // 这取决于你的具体需求和Yarn版本
        }
    }

    [YarnCommand("wait_for_input")]
    public static void WaitForInput()
    {
        ADVManager advManager = FindAnyObjectByType<ADVManager>();
        if (advManager != null)
        {
            advManager.StartCoroutine(advManager.WaitForInputCoroutine());
        }
    }

    public IEnumerator WaitForInputCoroutine()
    {
        // 暂停Yarn对话
        if (dialogueRunner != null)
        {
            // 设置等待输入状态
            bool waitingForInput = true;

            while (waitingForInput)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    waitingForInput = false;
                }

                yield return null;
            }
        }
    }

    #endregion

    #region 辅助方法

    Vector2 GetPositionFromString(string position)
    {
        var positions = characterContainer.GetComponent<CharacterContainer>().positions;
        switch (position.ToLower())
        {
            case "left":
                return new Vector2(-300f, 0f);
            case "center":
                return new Vector2(0f, 0f);
            case "right":
                return new Vector2(300f, 0f);
            default:
                if (int.TryParse(position, out int index) &&
                    index >= 0 && index < positions.Count)
                {
                    return positions[index].anchoredPosition;
                }

                Debug.LogWarning($"找不到位置: {position}");
                return Vector2.zero;
        }
    }

    Sprite GetFgSprite(CharacterData characterData, string expression)
    {
        switch (expression.ToLower())
        {
            case "normal":
            case "default":
                return characterData.expressions.Length > 0 ? characterData.expressions[0] : null;
            case "happy":
                return characterData.expressions.Length > 1 ? characterData.expressions[1] : null;
            case "sad":
                return characterData.expressions.Length > 2 ? characterData.expressions[2] : null;
            case "angry":
                return characterData.expressions.Length > 3 ? characterData.expressions[3] : null;
            case "surprised":
                return characterData.expressions.Length > 4 ? characterData.expressions[4] : null;
            default:
                Debug.Log($"尝试通过索引获取表情: {expression}");
                // 尝试通过索引获取
                if (int.TryParse(expression, out int index) &&
                    index >= 0 && index < characterData.fgs.Length)
                {
                    return characterData.fgs[index];
                }

                return characterData.fgs.Length > 0 ? characterData.fgs[0] : null;
        }
    }

    #endregion
}