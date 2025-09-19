using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Yarn.Unity;

public class CustomDialoguePresenter : DialoguePresenterBase
{
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;

    public override async YarnTask OnDialogueStartedAsync() {
        dialoguePanel.SetActive(true);
        await YarnTask.CompletedTask;
    }

    public override async YarnTask OnDialogueCompleteAsync() {
        dialoguePanel.SetActive(false);
        await YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token) {
        dialogueText.text = line.TextWithoutCharacterName.Text;
        await Task.Delay(2000); // 2 秒后自动下一句
    }


    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken) {
        // 最简单：直接返回第一个选项
        Debug.Log("Options: " + dialogueOptions.Length);
        return dialogueOptions.Length > 0 ? dialogueOptions[0] : null;
    }
}