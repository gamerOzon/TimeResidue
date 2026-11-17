using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalEscolha : MonoBehaviour
{
    public string cenaAlternativa = "Alternativa";

    private Transform cabeca;
    private bool jaAtivou = false;
    private OVRGrabber grabberEsquerda;
    private OVRGrabber grabberDireita;

    void Start()
    {
        GameObject cab = GameObject.Find("CenterEyeAnchor");
        if (cab != null) cabeca = cab.transform;

        GameObject le = GameObject.Find("LeftHandOnControllerAnchor");
        GameObject re = GameObject.Find("RightHandOnControllerAnchor");
        if (le != null) grabberEsquerda = le.GetComponent<OVRGrabber>();
        if (re != null) grabberDireita = re.GetComponent<OVRGrabber>();
    }

    void Update()
    {
        if (jaAtivou || cabeca == null) return;

        Collider col = GetComponent<Collider>();
        if (col != null && col.bounds.Contains(cabeca.position))
        {
            OVRGrabbable objetoAgarrado = null;

            if (grabberEsquerda != null && grabberEsquerda.grabbedObject != null)
                objetoAgarrado = grabberEsquerda.grabbedObject;
            else if (grabberDireita != null && grabberDireita.grabbedObject != null)
                objetoAgarrado = grabberDireita.grabbedObject;

            if (objetoAgarrado != null && objetoAgarrado.gameObject.name.Contains("bolademadeira"))
            {
                jaAtivou = true;
                SceneManager.LoadScene(cenaAlternativa);
            }
        }
    }
}