using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum DoorState
{
    Stop,
    Open,
    Opened,
    Close,
    Closed
}

public class OpenDoor : MonoBehaviour
{
    public GameObject Point;

    [Range(0, 360)]
    public float MinAngle = 0;
    [Range(0, 360)]
    public float MaxAngle = 90;
    
    
    [SerializeField]
    private DoorState state = DoorState.Stop;

    [Range(0, 360)]
    public float Speed = 60;

    private float angle = 0;

    private float rotationY = 0;

    private void Awake()
    {
        rotationY = gameObject.transform.localRotation.y;
    }

    void Start()
    {
    }

    // 切换开关状态
    public void Switch()
    {
        switch (state)
        {
            case DoorState.Stop:
                state = DoorState.Open;
                break;
            case DoorState.Open:
                state = DoorState.Close;
                break;
            case DoorState.Close:
                state = DoorState.Open;
                break;
            case DoorState.Closed:
                state = DoorState.Open;
                break;
            case DoorState.Opened:
                state = DoorState.Close;
                break;
        }
    }

    private void OnValidate()
    {
        MinAngle = Math.Min(MinAngle, MaxAngle);
        MaxAngle = Math.Max(MinAngle, MaxAngle);
        Speed = Math.Clamp(Speed, 0, 360);
    }


    // Update is called once per frame
    void Update()
    {
        if (!Point)
        {
            return;
        }

        float thisFrameAngle = Speed * Time.deltaTime;
        switch (state)
        {
            case DoorState.Stop:
            case DoorState.Opened:
            case DoorState.Closed:
                return;
            case DoorState.Open:
                if (angle + thisFrameAngle > MaxAngle)
                {
                    thisFrameAngle = MaxAngle - angle;
                    state = DoorState.Opened;
                }

                break;
            case DoorState.Close:
                thisFrameAngle *= -1;
                if (angle + thisFrameAngle < MinAngle)
                {
                    thisFrameAngle = -(angle - MinAngle);
                    state = DoorState.Closed;
                }

                break;
        }
        

        angle += thisFrameAngle;

        gameObject.transform.RotateAround(Point.transform.position, Vector3.up, thisFrameAngle);
    }
}