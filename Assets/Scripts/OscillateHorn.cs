using UnityEngine;

public class OscillateHorn : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float speed = 1f;

    private Vector3 startRotation;

    void Start()
    {
        startRotation = transform.localEulerAngles;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localEulerAngles = new Vector3(startRotation.x + angle, startRotation.y, startRotation.z);
    }
}