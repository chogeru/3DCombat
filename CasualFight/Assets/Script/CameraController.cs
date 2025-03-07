using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("プレイヤーとのオフセット設定")]
    [SerializeField] private Transform player;
    [SerializeField] private float distance = 3f;
    [SerializeField] private float smoothSpeed = 0.125f;

    [Header("マウス感度・角度制限")]
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    

    private float horizontalAngle = 0f;
    private float verticalAngle = 45f;

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalAngle += mouseX;
        verticalAngle -= mouseY;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);

        float radVert = verticalAngle * Mathf.Deg2Rad;
        float radHoriz = horizontalAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            distance * Mathf.Cos(radVert) * Mathf.Sin(radHoriz),
            distance * Mathf.Sin(radVert),
            distance * Mathf.Cos(radVert) * Mathf.Cos(radHoriz)
        );
        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(player.position);
    }
}