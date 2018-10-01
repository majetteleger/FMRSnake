using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    [TextArea] public string English;
    [TextArea] public string French;
    
    private Text _text;
    
    public void Apply()
    {
        if (_text == null)
        {
            _text = GetComponentInChildren<Text>(true);
        }

        _text.text = MainManager.Instance.Language == MainManager.UserLanguage.English ? English : French;
    }
}
