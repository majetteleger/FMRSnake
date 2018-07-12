using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNamePanel : MonoBehaviour
{
    private const string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public Text[] CharacterSlots;

    private int[] _characterSlotContentIndices;
    private int _currentlySelectedSlotIndex;
    private bool _shown;

    private void Update()
    {
        if(!_shown)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _characterSlotContentIndices[_currentlySelectedSlotIndex]--;

            if (_characterSlotContentIndices[_currentlySelectedSlotIndex] < 0)
            {
                _characterSlotContentIndices[_currentlySelectedSlotIndex] = _alphabet.Length - 1;
            }

            UpdateCharacterSlot();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _currentlySelectedSlotIndex++;

            if (_currentlySelectedSlotIndex >= CharacterSlots.Length)
            {
                _currentlySelectedSlotIndex = 0;
            }

            UpdateSelection();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _characterSlotContentIndices[_currentlySelectedSlotIndex]++;

            if (_characterSlotContentIndices[_currentlySelectedSlotIndex] >= _alphabet.Length)
            {
                _characterSlotContentIndices[_currentlySelectedSlotIndex] = 0;
            }

            UpdateCharacterSlot();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _currentlySelectedSlotIndex--;

            if(_currentlySelectedSlotIndex < 0)
            {
                _currentlySelectedSlotIndex = CharacterSlots.Length - 1;
            }

            UpdateSelection();
        }
    }

    public void Show()
    {
        _shown = true;
    }

    public void Hide()
    {
        _shown = false;
    }

    private void UpdateCharacterSlot()
    {
        CharacterSlots[_currentlySelectedSlotIndex].text = _alphabet[_characterSlotContentIndices[_currentlySelectedSlotIndex]].ToString();
    }

    private void UpdateSelection()
    {

    }


}
