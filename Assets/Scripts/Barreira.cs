using UnityEngine;

public class Barreira : MonoBehaviour
{
    public AudioClip mensagemVoz;
    private AudioSource audioSource;
    private Transform cabeca;
    private bool dentroBarreira = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        cabeca = GameObject.Find("CenterEyeAnchor").transform;
    }

    void Update()
    {
        if (cabeca == null) return;

        Collider col = GetComponent<Collider>();
        bool estaATocar = col.bounds.Contains(cabeca.position);

        if (estaATocar && !dentroBarreira)
        {
            dentroBarreira = true;

            // Áudio só toca uma vez ao entrar
            if (mensagemVoz != null && !audioSource.isPlaying)
                audioSource.PlayOneShot(mensagemVoz);
        }
        else if (!estaATocar && dentroBarreira)
        {
            dentroBarreira = false;

            // Para a vibração ao sair
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }

        // Vibração contínua enquanto está a tocar na barreira
        if (dentroBarreira)
        {
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        }
    }
}