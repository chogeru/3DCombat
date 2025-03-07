using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 moveInput;

    /// <summary>
    /// ����������
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// ���͂Ɖ�]�̍X�V
    /// </summary>
    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        moveInput = (v * camForward + h * camRight).normalized;

        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// �����ړ��̍X�V
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 newVelocity = moveInput * moveSpeed;
        newVelocity.y = rb.velocity.y;
        rb.velocity = newVelocity;
    }
}