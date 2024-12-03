using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution (1536, 864, FullScreenMode.Windowed);
    }
}
