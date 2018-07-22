using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetroLine : MonoBehaviour
{
    public GameObject Outline;
    public Color Color;

    public void Emphasize()
    {
        Outline.SetActive(true);
    }

    public void DeEmphasize()
    {
        Outline.SetActive(false);
    }
}
