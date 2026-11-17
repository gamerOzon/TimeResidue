using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPress : MonoBehaviour
{
    [Header("Botão")]
    public Transform buttonPart;
    public float pressedY = -0.0096f;
    public float distanciaAtivacao = 0.3f;

    [Header("Som")]
    public AudioClip clickSound;

    [Header("Cena")]
    public string sceneName = "Presente";

    [Header("Feedback")]
    public float vibracaoDuracao = 0.3f;
    public float vibracaoIntensidade = 0.5f;

    private float originalY;
    private bool pressed = false;
    private AudioSource audioSource;
    private Transform maoEsquerda;
    private Transform maoDireita;

    void Start()
    {
        originalY = buttonPart.localPosition.y;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;
    }

    void Update()
    {
        if (pressed) return;

        bool maoPertoEsq = maoEsquerda != null &&
            Vector3.Distance(maoEsquerda.position, transform.position) < distanciaAtivacao;
        bool maoPertoDir = maoDireita != null &&
            Vector3.Distance(maoDireita.position, transform.position) < distanciaAtivacao;

        if (maoPertoEsq || maoPertoDir)
        {
            pressed = true;

            OVRInput.Controller controller = maoPertoEsq
                ? OVRInput.Controller.LTouch
                : OVRInput.Controller.RTouch;
            StartCoroutine(Vibrar(controller));

            StartCoroutine(PrimirBotao());
        }
    }

    System.Collections.IEnumerator PrimirBotao()
    {
        // Prime o botão
        Vector3 pos = buttonPart.localPosition;
        pos.y = pressedY;
        buttonPart.localPosition = pos;

        // Toca o som
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // Espera pelo som
        float delay = clickSound != null ? clickSound.length : 0.5f;
        yield return new WaitForSeconds(delay);

        // Carrega a cena
        SceneManager.LoadScene(sceneName);
    }

    System.Collections.IEnumerator Vibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }
}