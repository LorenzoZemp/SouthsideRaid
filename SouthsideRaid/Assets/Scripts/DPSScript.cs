﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DPSScript : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    void Awake()
    {
        textMesh = gameObject.GetComponent<TextMeshProUGUI>();
    }
    
    public void DPS(int _damage)
    {
        textMesh.SetText(_damage.ToString());
    }
}
