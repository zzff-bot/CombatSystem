using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{

    [SerializeField] Transform followTarget;    // Ŀ�����
    [SerializeField] float rotationSpeed = 2f;  // ��ת�ٶ�

    [SerializeField] float distance = 5;    // ������ľ���

    // ��ֱ�����С�Ƕ�
    [SerializeField] float minVerticalAngle = -10; 
    [SerializeField] float maxVerticalAngle = 45;

    [SerializeField] Vector2 framingOffset; //  ���ƫ����

    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float rotationX;
    float rotationY;

    float invertXVal;
    float invertYVal;

    private void Start()
    {
        //���ع��,�������
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;

        //��ȡ���Y�����ת
        rotationX += Input.GetAxis("Camera Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //��ȡ����X����ת
        rotationY += Input.GetAxis("Camera X") * invertXVal * rotationSpeed;

        //����ͷ��ת
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //���ｹ��λ��,
        var focusPostion = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        //����ͷ�����������ָ������
        transform.position = focusPostion - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;

    }

    //ֻ��ȡY�����תֵ
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

}
