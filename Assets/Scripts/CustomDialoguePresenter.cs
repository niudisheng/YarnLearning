using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class DialoguePresenter : DialoguePresenterBase
{
    public Text dialogueText;

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        // 用 YarnTask 封装一个协程
        // 创建 YarnTaskCompletionSource 实例
        var completionSource = new YarnTaskCompletionSource();

        // 启动您的异步任务（例如协程、异步方法等）

        StartCoroutine(ShowLine(line, token));
        return completionSource.Task;
    }

    private IEnumerator ShowLine(LocalizedLine line, LineCancellationToken token)
    {
        dialogueText.text = "";

        string text = line.TextWithoutCharacterName.Text;

        foreach (char c in text)
        {
            if (token.IsHurryUpRequested)
            {
                dialogueText.text = text; // 跳过动画，直接显示
                yield break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f); // 打字机延迟
        }

        // 等待玩家点击继续
        bool clicked = false;
        while (!clicked && !token.IsHurryUpRequested)
        {
            if (Input.GetMouseButtonDown(0))
                clicked = true;

            yield return null;
        }
    }


    public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        var completionSource = new YarnTaskCompletionSource();
        
        Debug.Log("Dialogue started");
        return completionSource.Task;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        throw new NotImplementedException();
    }
}