using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{

    [SerializeField] Transform followTarget;    // 目标玩家
    [SerializeField] float rotationSpeed = 2f;  // 旋转速度

    [SerializeField] float distance = 5;    // 与人物的距离

    // 垂直最大最小角度
    [SerializeField] float minVerticalAngle = -10; 
    [SerializeField] float maxVerticalAngle = 45;

    [SerializeField] Vector2 framingOffset; //  框架偏移量

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    private void Start()
    {
        //隐藏光标,锁定光标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        //获取鼠标Y轴的旋转
        rotationX += Input.GetAxis("Camera Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //获取鼠标的X轴旋转
        rotationY += Input.GetAxis("Camera X") * invertXVal * rotationSpeed;

        //摄像头旋转
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //人物焦点位置,
        var focusPostion = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        //摄像头跟随人物，并且指向人物
        transform.position = focusPostion - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;

    }

    //只获取Y轴的旋转值
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

}
