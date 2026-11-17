using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionsAdvance : MonoBehaviour
{
    [Header("Cena seguinte")]
    public string proximaCena = "MenuAndIntro";

    [Header("Feedback")]
    public float vibracaoDuracao = 0.3f;
    public float vibracaoIntensidade = 0.5f;

    private bool jaAvancou = false;

    void Update()
    {
        if (jaAvancou) return;
        // So permite avancar depois do idioma escolhido + instrucoes mostradas
        if (!InstructionsLanguage.IdiomaEscolhido) return;

        bool gripEsq = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.7f;
        bool gripDir = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.7f;

        if (gripEsq || gripDir)
        {
            jaAvancou = true;

            OVRInput.Controller controller = gripEsq
                ? OVRInput.Controller.LTouch
                : OVRInput.Controller.RTouch;
            StartCoroutine(AvancarComVibrar(controller));
        }
    }

    System.Collections.IEnumerator AvancarComVibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);

        SceneManager.LoadScene(proximaCena);
    }
}
