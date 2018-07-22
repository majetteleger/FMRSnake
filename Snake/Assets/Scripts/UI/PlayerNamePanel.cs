using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNamePanel : MonoBehaviour
{
    public static PlayerNamePanel Instance;

    private const string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public CharacterSlot[] CharacterSlots;

    private int[] _characterSlotContentIndices;
    private int _currentlySelectedSlotIndex;

    private void Awake()
    {
        Instance = this;

        ToggleConfirm(false);
    }
    
    private void OnEnable()
    {
        _characterSlotContentIndices = new int[CharacterSlots.Length];
        _currentlySelectedSlotIndex = 0;
        UpdateSelection();

        foreach (var characterSlot in CharacterSlots)
        {
            if (characterSlot.IsConfirm)
            {
                continue;
            }

            characterSlot.Character.text = "A";
        }
    }
    
    public void Input(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.UpArrow:

                if (_currentlySelectedSlotIndex == CharacterSlots.Length - 1)
                {
                    break;
                }

                _characterSlotContentIndices[_currentlySelectedSlotIndex]--;

                if (_characterSlotContentIndices[_currentlySelectedSlotIndex] < 0)
                {
                    _characterSlotContentIndices[_currentlySelectedSlotIndex] = _alphabet.Length - 1;
                }

                UpdateCharacterSlot();

                break;

            case KeyCode.RightArrow:
                
                if (_currentlySelectedSlotIndex == CharacterSlots.Length - 1)
                {
                    ConfirmName();
                    break;
                }

                if (!CharacterSlots[_currentlySelectedSlotIndex + 1].IsOn)
                {
                    break;
                }

                _currentlySelectedSlotIndex++;
                
                UpdateSelection();

                break;

            case KeyCode.DownArrow:

                if (_currentlySelectedSlotIndex == CharacterSlots.Length - 1)
                {
                    break;
                }

                _characterSlotContentIndices[_currentlySelectedSlotIndex]++;

                if (_characterSlotContentIndices[_currentlySelectedSlotIndex] >= _alphabet.Length)
                {
                    _characterSlotContentIndices[_currentlySelectedSlotIndex] = 0;
                }

                UpdateCharacterSlot();

                break;

            case KeyCode.LeftArrow:

                if (_currentlySelectedSlotIndex == 0 || !CharacterSlots[_currentlySelectedSlotIndex - 1].IsOn)
                {
                    break;
                }

                _currentlySelectedSlotIndex--;
                
                UpdateSelection();

                break;
        }
    }

    public void ToggleConfirm(bool toggle)
    {
        CharacterSlots[CharacterSlots.Length - 1].IsOn = toggle;

        var color = CharacterSlots[CharacterSlots.Length - 1].GetComponent<Image>().color;
        color.a = toggle ? 1f : 0.5f;

        CharacterSlots[CharacterSlots.Length - 1].GetComponent<Image>().color = color;
    }

    private void UpdateCharacterSlot()
    {
        CharacterSlots[_currentlySelectedSlotIndex].Character.text = _alphabet[_characterSlotContentIndices[_currentlySelectedSlotIndex]].ToString();
    }

    private void UpdateSelection()
    {
        if (_currentlySelectedSlotIndex == CharacterSlots.Length - 1)
        {
            MainPanel.Instance.PlayerNameEnterConfirmControls.ApplyControls();
        }
        else if(_currentlySelectedSlotIndex == 0)
        {
            MainPanel.Instance.PlayerNameEnterLeftControls.ApplyControls();
        }
        else
        {
            MainPanel.Instance.PlayerNameEnterControls.ApplyControls();
        }

        for (var i = 0; i < CharacterSlots.Length; i++)
        {
            CharacterSlots[i].Toggle(i == _currentlySelectedSlotIndex);
        }
    }

    private void ConfirmName()
    {
        var playerName = string.Empty;

        foreach (var characterSlot in CharacterSlots)
        {
            if (characterSlot.IsConfirm)
            {
                continue;
            }

            playerName += characterSlot.Character.text;
        }

        MainManager.Instance.CurrentPlayerName = playerName;
        MainManager.Instance.TransitionToPlay();
    }
}
