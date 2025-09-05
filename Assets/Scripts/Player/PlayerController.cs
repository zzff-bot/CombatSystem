using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float moveSpeed = 5f;  // �ƶ��ٶ�
    [SerializeField] float rotationSpeed = 500f;  // ��ת�ٶ�

    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGround;

    float ySpeed;
    Quaternion targetRotation;

    CameraController cameraController;

    CharacterController characterController;

    Animator animator;
   
    MeeleFighter meeleFighter;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
    }

    void Update()
    {
        if (meeleFighter.InAction)
        {
            //��Ϊ��Ȼû���ƶ����Ƕ���moveAountֵ����Ϊ0
            animator.SetFloat("fowardSpeed", 0);
            return;
        }

        //��ȡ��ɫˮƽ�ʹ�ֱλ��
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //ͨ��Mathf.Clamp01()��ֵ������0-1֮�� 
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //��ɫ���ƶ�����
        var moveInput = (new Vector3(h , 0, v)).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;

        GroundCheck();
        //Debug.Log("isGround = " + isGround);
        if (isGround)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        //��ɫλ��(ͨ����ɫ������)
        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            //��¼��ɫ��תλ��
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        //ʵ�ֽ�ɫԭ��ƽ����ת
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //���������ʱ��
        animator.SetFloat("fowardSpeed", moveAmount, 0.2f, Time.deltaTime);

    }

    //�����⣬�������꣬�ڽ�ɫ���£����뾶�����ͼ��
    void GroundCheck()
    {
        isGround = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    //�ű������ص����屻ѡ��ʱ�Ż�ִ�У�ʵ�ֿ��ӻ�
    private void OnDrawGizmosSelected()
    {
        //���û���ͼ�ε���ɫ
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        //����һ�����Σ�λ�úͰ뾶�����С��ͬ
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

}
