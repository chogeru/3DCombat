using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 0.05f;

    void Update()
    {
        float rotation = Time.time * rotationSpeed;

        RenderSettings.skybox.SetFloat("_Rotation", rotation);
    }
}
