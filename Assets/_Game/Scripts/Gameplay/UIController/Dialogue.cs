using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _instruction;
    [SerializeField] private GameObject _demoTiles, _okButton;
    [SerializeField] private TypewriterByCharacter _typewriter;

    private int _index;

    private string[] _lines = {
        "Welcome to your first game.\nI'll get you through the basics.",
        "Click and drag a line of letters to form a word.\nClick the tick button to confirm.",
        "You will get points equal to the combined score of each letter times the total letters.",
        "You can choose up to 3 powerups to help you during the game.",
        "You can also use 2 powerups at the bottom left anytime you want.",
        "There will be a timer.\nUse your time wisely.",
        "Pop the following tiles to start the game."
    };

    private void OnEnable()
    {
        _instruction.text = string.Empty;
        Notifier.Instance.StopCountdown();
        StartDialogue();

        _okButton.SetActive(true);
        _demoTiles.SetActive(false);
    }

    private void OnDisable()
    {
        Notifier.Instance.BeginCountdown();
    }

    private void StartDialogue()
    {
        _index = 0;
        _typewriter.ShowText(_lines[_index]);
    }

    public void NextLine()
    {
        if (!_typewriter.isShowingText)
        {
            if (_index < _lines.Length - 1)
            {
                _index++;
                _typewriter.ShowText(_lines[_index]);
            }
            else
            {
                _okButton.SetActive(false);
                _demoTiles.SetActive(true);
                _demoTiles.transform.localScale = Vector3.zero;
                _demoTiles.transform.DOScale(Vector3.one, 0.5f).OnComplete(() => DemoTiles.Instance.CursorAnimation());
            }
        }
        else
        {
            _typewriter.SkipTypewriter();
        }
    }
}
