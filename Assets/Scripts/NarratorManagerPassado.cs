using UnityEngine;
using System.Collections;

public class NarratorManagerPassado : MonoBehaviour
{

    [Header("Papéis")]
    public PapeisVoar papeisVoar;

    [Header("Audio Clips — em ordem")]
    public AudioClip labEnter;
    public AudioClip infoBilliardBalls;
    public AudioClip infoLeo;
    public AudioClip infoMoneyPrize;
    public AudioClip faint;

    [Header("Audio Clips PT — mesma ordem")]
    public AudioClip labEnterPt;
    public AudioClip infoBilliardBallsPt;
    public AudioClip infoLeoPt;
    public AudioClip infoMoneyPrizePt;
    public AudioClip faintPt;

    [Header("Scripts de Interação")]
    public PoolBallsInteracao poolBallsInteracao;
    public InventorInteracao inventorInteracao;
    public MoneyPrizeInteracao moneyPrizeInteracao;

    [Header("Textos de Info")]
    public GameObject infoBalls;
    public GameObject infoInventor;
    public GameObject infoMoney;

    [Header("Textos de Info PT")]
    public GameObject infoBallsPt;
    public GameObject infoInventorPt;
    public GameObject infoMoneyPt;

    [Header("Setas de Direção")]
    public GameObject arrowBalls;
    public GameObject arrowInventor;
    public GameObject arrowMoney;

    [Header("NPC")]
    public Animator npcAnimator;

    [Header("Portal")]
    public GameObject portal;
    public AudioClip somPortal;

    [Header("Pausas")]
    public float pausaAposIntro = 3f;
    public float pausaEntreInteracoes = 1f;
    public float pausaAposPortalAbrir = 6f;

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
    private AudioSource audioSourceInfo;
    private bool hoverBalls = false;
    private bool hoverInventor = false;
    private bool hoverMoney = false;
    private Transform maoEsquerda;
    private Transform maoDireita;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        audioSourceInfo = gameObject.AddComponent<AudioSource>();
        audioSourceInfo.spatialBlend = 0f;

        if (poolBallsInteracao != null) poolBallsInteracao.enabled = false;
        if (inventorInteracao != null) inventorInteracao.enabled = false;
        if (moneyPrizeInteracao != null) moneyPrizeInteracao.enabled = false;
        if (portal != null) portal.SetActive(false);

        if (infoBalls != null) infoBalls.SetActive(false);
        if (infoInventor != null) infoInventor.SetActive(false);
        if (infoMoney != null) infoMoney.SetActive(false);
        if (infoBallsPt != null) infoBallsPt.SetActive(false);
        if (infoInventorPt != null) infoInventorPt.SetActive(false);
        if (infoMoneyPt != null) infoMoneyPt.SetActive(false);

        if (arrowBalls != null) arrowBalls.SetActive(false);
        if (arrowInventor != null) arrowInventor.SetActive(false);
        if (arrowMoney != null) arrowMoney.SetActive(false);

        GameObject le = GameObject.Find("LeftHandAnchor");
        GameObject re = GameObject.Find("RightHandAnchor");
        if (le != null) maoEsquerda = le.transform;
        if (re != null) maoDireita = re.transform;

        // Ligar apenas o onAnimacaoIniciou de cada interação
        if (poolBallsInteracao != null)
            poolBallsInteracao.onAnimacaoIniciou = () => { hoverBalls = true; VibrarMaoPerto(poolBallsInteracao.transform); };
        if (inventorInteracao != null)
            inventorInteracao.onAnimacaoIniciou = () => { hoverInventor = true; VibrarMaoPerto(inventorInteracao.transform); };
        if (moneyPrizeInteracao != null)
            moneyPrizeInteracao.onAnimacaoIniciou = () => { hoverMoney = true; VibrarMaoPerto(moneyPrizeInteracao.transform); };

        StartCoroutine(Sequencia());
    }

    IEnumerator Sequencia()
    {
        // 1. Intro
        yield return StartCoroutine(TocarEsperar(L(labEnter, labEnterPt)));
        yield return new WaitForSeconds(pausaAposIntro);

        // 2. Bolas
        if (arrowBalls != null) arrowBalls.SetActive(true);
        TocarSomArrows();
        if (poolBallsInteracao != null) poolBallsInteracao.enabled = true;

        Coroutine lembreteBalls = StartCoroutine(LembreteSom(() => hoverBalls));
        yield return new WaitUntil(() => hoverBalls);
        StopCoroutine(lembreteBalls);
        if (arrowBalls != null) arrowBalls.SetActive(false);
        GameObject iBalls = LG(infoBalls, infoBallsPt);
        if (iBalls != null) iBalls.SetActive(true);
        yield return StartCoroutine(TocarEsperarInfo(L(infoBilliardBalls, infoBilliardBallsPt)));
        if (iBalls != null) iBalls.SetActive(false);
        if (poolBallsInteracao != null) poolBallsInteracao.enabled = false;

        yield return new WaitForSeconds(pausaEntreInteracoes);

        // 3. Inventor
        if (arrowInventor != null) arrowInventor.SetActive(true);
        TocarSomArrows();
        if (inventorInteracao != null) inventorInteracao.enabled = true;

        Coroutine lembreteInventor = StartCoroutine(LembreteSom(() => hoverInventor));
        yield return new WaitUntil(() => hoverInventor);
        StopCoroutine(lembreteInventor);
        if (arrowInventor != null) arrowInventor.SetActive(false);
        GameObject iInventor = LG(infoInventor, infoInventorPt);
        if (iInventor != null) iInventor.SetActive(true);
        yield return StartCoroutine(TocarEsperarInfo(L(infoLeo, infoLeoPt)));
        if (iInventor != null) iInventor.SetActive(false);
        if (inventorInteracao != null) inventorInteracao.enabled = false;

        yield return new WaitForSeconds(pausaEntreInteracoes);

        // 4. MoneyPrize
        if (arrowMoney != null) arrowMoney.SetActive(true);
        TocarSomArrows();
        if (moneyPrizeInteracao != null) moneyPrizeInteracao.enabled = true;

        Coroutine lembreteMoney = StartCoroutine(LembreteSom(() => hoverMoney));
        yield return new WaitUntil(() => hoverMoney);
        StopCoroutine(lembreteMoney);
        if (arrowMoney != null) arrowMoney.SetActive(false);
        GameObject iMoney = LG(infoMoney, infoMoneyPt);
        if (iMoney != null) iMoney.SetActive(true);
        yield return StartCoroutine(TocarEsperarInfo(L(infoMoneyPrize, infoMoneyPrizePt)));
        if (iMoney != null) iMoney.SetActive(false);
        if (moneyPrizeInteracao != null) moneyPrizeInteracao.enabled = false;

        yield return new WaitForSeconds(pausaEntreInteracoes);

        // NPC cai
        if (npcAnimator != null)
            npcAnimator.SetTrigger("morrer");

        if (papeisVoar != null)
            papeisVoar.IniciarVoo();

        // 5. Faint — portal abre a meio
        AudioClip cFaint = L(faint, faintPt);
        audioSource.clip = cFaint;
        audioSource.Play();
        yield return new WaitForSeconds(cFaint.length / 2f);

        if (portal != null)
        {
            portal.SetActive(true);
            if (somPortal != null)
                audioSourceInfo.PlayOneShot(somPortal);
        }

        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(pausaAposPortalAbrir);

        if (portal != null)
        {
            PortalTeleporte pt = portal.GetComponent<PortalTeleporte>();
            if (pt != null) pt.Armar();
        }
    }

    IEnumerator LembreteSom(System.Func<bool> ativado)
    {
        while (!ativado())
        {
            yield return new WaitForSeconds(intervaloLembrete);
            if (ativado()) yield break;
            TocarSomArrows();
        }
    }

    void TocarSomArrows()
    {
        if (somArrows != null)
            audioSourceInfo.PlayOneShot(somArrows, volumeArrows);
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
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitWhile(() => audioSource.isPlaying);
    }

    IEnumerator TocarEsperarInfo(AudioClip clip)
    {
        if (clip == null) yield break;
        audioSourceInfo.clip = clip;
        audioSourceInfo.Play();
        yield return new WaitWhile(() => audioSourceInfo.isPlaying);
    }
}