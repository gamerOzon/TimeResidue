using UnityEngine;
using System.Collections;

public class NarratorManager : MonoBehaviour
{
    [Header("Audio Clips — em ordem")]
    public AudioClip introSupermercado;
    public AudioClip instrucaoPeixe;
    public AudioClip infoPeixe;
    public AudioClip instrucaoCaixote;
    public AudioClip infoCaixote;
    public AudioClip instrucaoLeite;
    public AudioClip infoLeite;
    public AudioClip reflectionMoment;
    public AudioClip portalCrossing;

    [Header("Audio Clips PT — mesma ordem")]
    public AudioClip introSupermercadoPt;
    public AudioClip instrucaoPeixePt;
    public AudioClip infoPeixePt;
    public AudioClip instrucaoCaixotePt;
    public AudioClip infoCaixotePt;
    public AudioClip instrucaoLeitePt;
    public AudioClip infoLeitePt;
    public AudioClip reflectionMomentPt;
    public AudioClip portalCrossingPt;

    [Header("Scripts de Interação")]
    public PeixeInteracao peixeInteracao;
    public CaixoteInteracao caixoteInteracao;
    public LeiteInteracao leiteInteracao;

    [Header("Textos de Info")]
    public GameObject infoFish;
    public GameObject infoTrash;
    public GameObject infoMilk;

    [Header("Textos de Info PT")]
    public GameObject infoFishPt;
    public GameObject infoTrashPt;
    public GameObject infoMilkPt;

    [Header("Setas de Direção")]
    public GameObject arrowFish;
    public GameObject arrowTrash;
    public GameObject arrowMilk;

    [Header("Portal")]
    public GameObject portal;
    public AudioClip somPortal;

    [Header("Pausas entre áudios (segundos)")]
    public float pausaAposIntro = 5f;
    public float pausaAposInstrucaoPeixe = 2f;
    public float pausaAposInfoPeixe = 2f;
    public float pausaAposInstrucaoCaixote = 2f;
    public float pausaAposInfoCaixote = 2f;
    public float pausaAposInstrucaoLeite = 2f;
    public float pausaAposInfoLeite = 2f;
    public float pausaAposReflection = 2f;

    [Header("Feedback")]
    public float vibracaoDuracao = 0.3f;
    public float vibracaoIntensidade = 0.5f;

    [Header("Sons UI")]
    public AudioClip somArrows;
    [Tooltip("Volume do som da seta. >1 amplifica.")]
    public float volumeArrows = 2f;
    [Tooltip("Segundos ate repetir o som da seta se a pessoa nao ativar.")]
    public float intervaloLembrete = 5f;

    private AudioSource audioSource;
    private int estado = 0;
    private Transform maoEsquerda;
    private Transform maoDireita;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (peixeInteracao != null) peixeInteracao.enabled = false;
        if (caixoteInteracao != null) caixoteInteracao.enabled = false;
        if (leiteInteracao != null) leiteInteracao.enabled = false;
        if (portal != null) portal.SetActive(false);

        if (infoFish != null) infoFish.SetActive(false);
        if (infoTrash != null) infoTrash.SetActive(false);
        if (infoMilk != null) infoMilk.SetActive(false);
        if (infoFishPt != null) infoFishPt.SetActive(false);
        if (infoTrashPt != null) infoTrashPt.SetActive(false);
        if (infoMilkPt != null) infoMilkPt.SetActive(false);

        if (arrowFish != null) arrowFish.SetActive(false);
        if (arrowTrash != null) arrowTrash.SetActive(false);
        if (arrowMilk != null) arrowMilk.SetActive(false);

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;

        if (peixeInteracao != null)
            peixeInteracao.onAnimacaoIniciou = OnPeixeIniciou;
        if (caixoteInteracao != null)
            caixoteInteracao.onAnimacaoIniciou = OnCaixoteIniciou;
        if (leiteInteracao != null)
            leiteInteracao.onAnimacaoIniciou = OnLeiteIniciou;

        if (peixeInteracao != null)
            peixeInteracao.onInteracaoCompleta = OnPeixeCompleto;
        if (caixoteInteracao != null)
            caixoteInteracao.onInteracaoCompleta = OnCaixoteCompleto;
        if (leiteInteracao != null)
            leiteInteracao.onInteracaoCompleta = OnLeiteCompleto;

        StartCoroutine(Sequencia());
    }

    IEnumerator Sequencia()
    {
        yield return StartCoroutine(TocarEsperar(L(introSupermercado, introSupermercadoPt)));
        yield return new WaitForSeconds(pausaAposIntro);

        if (arrowFish != null) arrowFish.SetActive(true);
        TocarSomArrows();
        yield return StartCoroutine(TocarEsperar(L(instrucaoPeixe, instrucaoPeixePt)));
        yield return new WaitForSeconds(pausaAposInstrucaoPeixe);
        if (peixeInteracao != null) peixeInteracao.enabled = true;
        Coroutine lembreteFish = StartCoroutine(LembreteSom(arrowFish));
        yield return new WaitUntil(() => estado >= 1);
        StopCoroutine(lembreteFish);
        Debug.Log("[NAR] estado>=1 (peixe feito) t=" + Time.time.ToString("0.0") + " isPlaying=" + audioSource.isPlaying);

        // espera o audio de info do peixe terminar antes de avancar
        yield return new WaitWhile(() => audioSource.isPlaying);
        Debug.Log("[NAR] info-audio acabou t=" + Time.time.ToString("0.0"));
        yield return new WaitForSeconds(pausaAposInfoPeixe);
        Debug.Log("[NAR] pausa acabou -> vai tocar instrucaoCaixote t=" + Time.time.ToString("0.0"));

        if (arrowTrash != null) arrowTrash.SetActive(true);
        TocarSomArrows();
        yield return StartCoroutine(TocarEsperar(L(instrucaoCaixote, instrucaoCaixotePt)));
        yield return new WaitForSeconds(pausaAposInstrucaoCaixote);
        if (caixoteInteracao != null) caixoteInteracao.enabled = true;
        Coroutine lembreteTrash = StartCoroutine(LembreteSom(arrowTrash));
        yield return new WaitUntil(() => estado >= 2);
        StopCoroutine(lembreteTrash);

        // espera o audio de info do lixo terminar antes de avancar
        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(pausaAposInfoCaixote);

        if (arrowMilk != null) arrowMilk.SetActive(true);
        TocarSomArrows();
        yield return StartCoroutine(TocarEsperar(L(instrucaoLeite, instrucaoLeitePt)));
        yield return new WaitForSeconds(pausaAposInstrucaoLeite);
        if (leiteInteracao != null) leiteInteracao.enabled = true;
        Coroutine lembreteMilk = StartCoroutine(LembreteSom(arrowMilk));
        yield return new WaitUntil(() => estado >= 3);
        StopCoroutine(lembreteMilk);

        // espera o audio de info do leite terminar antes de avancar
        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(pausaAposInfoLeite);

        yield return StartCoroutine(TocarEsperar(L(reflectionMoment, reflectionMomentPt)));
        yield return new WaitForSeconds(pausaAposReflection);

        if (portal != null)
        {
            portal.SetActive(true);
            if (somPortal != null)
                audioSource.PlayOneShot(somPortal);
        }
        yield return StartCoroutine(TocarEsperar(L(portalCrossing, portalCrossingPt)));

        if (portal != null)
        {
            PortalTeleporte pt = portal.GetComponent<PortalTeleporte>();
            if (pt != null) pt.Armar();
        }
    }

    // Escolhe o clip PT se idioma for portugues e existir; senao o EN.
    AudioClip L(AudioClip en, AudioClip pt)
    {
        return (LanguageManager.Portugues && pt != null) ? pt : en;
    }

    GameObject LG(GameObject en, GameObject pt)
    {
        return (LanguageManager.Portugues && pt != null) ? pt : en;
    }

    IEnumerator TocarEsperar(AudioClip clip)
    {
        if (clip == null) yield break;
        audioSource.PlayOneShot(clip);
        yield return new WaitWhile(() => audioSource.isPlaying);
    }
        
    IEnumerator DesativarAposAudio(GameObject texto, AudioClip clip)
    {
        if (clip != null)
            yield return new WaitForSeconds(clip.length);
        if (texto != null) texto.SetActive(false);
    }

    void OnPeixeIniciou()
    {
        if (arrowFish != null) arrowFish.SetActive(false);
        GameObject fFish = LG(infoFish, infoFishPt);
        if (fFish != null) fFish.SetActive(true);
        AudioClip cPeixe = L(infoPeixe, infoPeixePt);
        audioSource.PlayOneShot(cPeixe);
        Debug.Log("[NAR] OnPeixeIniciou: toca infoPeixe t=" + Time.time.ToString("0.0") + " len=" + (cPeixe != null ? cPeixe.length.ToString("0.0") : "null"));
        StartCoroutine(DesativarAposAudio(fFish, cPeixe));
        if (peixeInteracao != null) VibrarMaoPerto(peixeInteracao.transform);
    }

    void OnCaixoteIniciou()
    {
        if (arrowTrash != null) arrowTrash.SetActive(false);
        GameObject fTrash = LG(infoTrash, infoTrashPt);
        if (fTrash != null) fTrash.SetActive(true);
        AudioClip cCaixote = L(infoCaixote, infoCaixotePt);
        audioSource.PlayOneShot(cCaixote);
        StartCoroutine(DesativarAposAudio(fTrash, cCaixote));
        if (caixoteInteracao != null) VibrarMaoPerto(caixoteInteracao.transform);
    }

    void OnLeiteIniciou()
    {
        if (arrowMilk != null) arrowMilk.SetActive(false);
        GameObject fMilk = LG(infoMilk, infoMilkPt);
        if (fMilk != null) fMilk.SetActive(true);
        AudioClip cLeite = L(infoLeite, infoLeitePt);
        audioSource.PlayOneShot(cLeite);
        StartCoroutine(DesativarAposAudio(fMilk, cLeite));
        if (leiteInteracao != null) VibrarMaoPerto(leiteInteracao.transform);
    }

    void TocarSomArrows()
    {
        if (somArrows != null)
            audioSource.PlayOneShot(somArrows, volumeArrows);
    }

    IEnumerator LembreteSom(GameObject arrow)
    {
        while (arrow != null && arrow.activeSelf)
        {
            yield return new WaitForSeconds(intervaloLembrete);
            if (arrow == null || !arrow.activeSelf) yield break;
            TocarSomArrows();
        }
    }

    void VibrarMaoPerto(Transform objeto)
    {
        float distEsq = maoEsquerda != null ? Vector3.Distance(maoEsquerda.position, objeto.position) : float.MaxValue;
        float distDir = maoDireita != null ? Vector3.Distance(maoDireita.position, objeto.position) : float.MaxValue;

        OVRInput.Controller controller = (distEsq < distDir)
            ? OVRInput.Controller.LTouch
            : OVRInput.Controller.RTouch;

        StartCoroutine(Vibrar(controller));
    }

    IEnumerator Vibrar(OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(1f, vibracaoIntensidade, controller);
        yield return new WaitForSeconds(vibracaoDuracao);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    void OnPeixeCompleto()
    {
        if (peixeInteracao != null) peixeInteracao.enabled = false;
        estado = 1;
    }

    void OnCaixoteCompleto()
    {
        if (caixoteInteracao != null) caixoteInteracao.enabled = false;
        estado = 2;
    }

    void OnLeiteCompleto()
    {
        if (leiteInteracao != null) leiteInteracao.enabled = false;
        estado = 3;
    }
}