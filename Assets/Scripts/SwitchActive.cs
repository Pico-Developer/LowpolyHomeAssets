using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchActive : MonoBehaviour
{
    [SerializeField] private GameObject LightOnNode;
    
    [SerializeField] private GameObject LightOffNode;

    private bool switchFlag = true;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Awake()
    {
        UpdateSwitch();
    }

    public void Switch()
    {
        switchFlag = !switchFlag;
        UpdateSwitch();
    }

    private void UpdateSwitch()
    {
        if (LightOnNode)
        {
            LightOnNode.SetActive(switchFlag);
        }

        if (LightOffNode)
        {
            LightOffNode.SetActive(!switchFlag);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}