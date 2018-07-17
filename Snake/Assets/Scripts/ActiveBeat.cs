using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBeat : UIBeat {

    public Image Light;

    public void ResetColor()
    {
        Light.color = Color.black;
    }
}