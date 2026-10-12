using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Datos del Arma (ScriptableObject)")]
    [SerializeField] WeaponPositionData weaponData;

    [Header("Recoil - Dinámica")]
    // current = offset visible aplicado sobre la pose base.
    // El disparo empuja "current" instantáneamente (kick) y luego
    // SmoothDamp lo reconduce a 0 de forma crítica (sin rebote, sin lag doble).
    private Vector3 kickRotation;
    private Vector3 kickRotationVelocity;
    private Vector3 kickPosition;
    private Vector3 kickPositionVelocity;

    private int consecutiveShots;
    private float lastShotTime = -999f;

    [Header("Trigger / Cadencia")]
    private bool triggerHeld;
    private float nextFireTime;

    private bool rotationErrorPending;
    private float rotationErrorTimer;

    [SerializeField] Transform EjeArma;
    private Vector3 RestPos;
    private Vector3 RestRot;
    private Vector3 SightPos;
    private Vector3 ActualPos;
    private Vector3 ActualRot;

    [Header("Graphic Stats")]
    [SerializeField] Transform ShellPoint;
    [SerializeField] GameObject shell;
    [SerializeField] ParticleSystem boquilla;
    [SerializeField] ParticleSystem side;
    [SerializeField] Light light;
    [SerializeField] Transform[] Anclas = new Transform[3]; //Gatillo,agarre,recarga;
    [SerializeField] ProcHands procHands;

    [Header("Weapon Stats")]
    [SerializeField] bool isAuto;
    [SerializeField] bool isEmpty;
    [SerializeField] byte ammo;
    [SerializeField] byte MaxAmmo;

    [Header("Sound Stats")]
    private AudioClip shotClip;
    private AudioClip emptyClip;
    private AudioClip reloadClip;
    AudioSource source;

    [Header("Debug Settings")]
    [SerializeField] bool Shot;
    [SerializeField] bool isReload;
    [SerializeField] bool gatillo;
    [SerializeField] bool effOff;

    private Vector3 ReloadRot;
    private Vector3 ReloadPos;

    void Start()
    {
        source = GetComponent<AudioSource>();

        if (weaponData != null)
        {
            ApplyPositionData(weaponData);
        }
        else
        {
            // Fallback si no se asignó ScriptableObject: usa la pose actual del transform.
            ActualPos = RestPos = EjeArma.localPosition;
            ActualRot = RestRot = EjeArma.localEulerAngles;
            ammo = 8;
            MaxAmmo = ammo;
        }

        if (boquilla && source && side)
        {
            effOff = false;
        }
        else { effOff = true; }
    }

    /// <summary>
    /// Asigna al Weapon todos los datos de un WeaponPositionData:
    /// posiciones, parámetros de recoil, munición y audio.
    /// Llamar al equipar un arma distinta.
    /// </summary>
    public void ApplyPositionData(WeaponPositionData data)
    {
        weaponData = data;

        RestPos = data.RestPos;
        RestRot = data.RestRot;
        SightPos = data.SightPos;
        ReloadPos = data.ReloadPos;
        ReloadRot = data.ReloadRot;

        ActualPos = RestPos;
        ActualRot = RestRot;

        MaxAmmo = data.MaxAmmo;
        ammo = MaxAmmo;

        shotClip = data.shotClip;
        emptyClip = data.emptyClip;
        reloadClip = data.reloadClip;

        // Reinicia el estado de recoil para que no arrastre valores del arma anterior.
        kickRotation = Vector3.zero;
        kickRotationVelocity = Vector3.zero;
        kickPosition = Vector3.zero;
        kickPositionVelocity = Vector3.zero;
        consecutiveShots = 0;
    }

    void Update()
    {
        // El disparo automático vive en Update (no FixedUpdate) porque depende
        // de Time.time con precisión de frame, no de la física.
        if (weaponData == null) return;

        if (isAuto && triggerHeld && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    /// <summary>
    /// Llamar cuando se pulsa el gatillo (input down).
    /// En semi-auto dispara una vez (respetando fireRate); en auto arma el disparo continuo.
    /// </summary>
    public bool PullTrigger()
    {
        triggerHeld = true;
        if (!isAuto && Time.time >= nextFireTime)
        {
            Shoot();
            return true;
        }
        return false;
    }

    /// <summary>Llamar cuando se suelta el gatillo (input up).</summary>
    public void ReleaseTrigger()
    {
        triggerHeld = false;
    }

    void FixedUpdate()
    {
        if (weaponData == null) return;

        // Retorno crítico del kick hacia 0: sube rápido en el disparo,
        // decae suave sin oscilar (evita el efecto "gomoso" del doble Lerp).
        kickRotation = Vector3.SmoothDamp(kickRotation, Vector3.zero, ref kickRotationVelocity, weaponData.settleTime);
        kickPosition = Vector3.SmoothDamp(kickPosition, Vector3.zero, ref kickPositionVelocity, weaponData.settleTime);

        transform.localEulerAngles = ActualRot + kickRotation;
        transform.localPosition = ActualPos + kickPosition;

        if (rotationErrorPending)
        {
            rotationErrorTimer += Time.fixedDeltaTime;
            if (rotationErrorTimer >= weaponData.rotationErrorDelayTime)
            {
                ApplyRotationErrorDelay();
                rotationErrorPending = false;
                rotationErrorTimer = 0f;
            }
        }

        if (gatillo)
        {
            gatillo = false;
            ApplyRecoil();
            ApplyEffect();
        }
    }

    public void ApplyRecoil()
    {
        if (weaponData == null) return;

        // Reinicia la ráfaga si ha pasado demasiado tiempo desde el último disparo.
        if (Time.time - lastShotTime > weaponData.burstResetTime)
        {
            consecutiveShots = 0;
        }
        lastShotTime = Time.time;

        // Cuantos más disparos consecutivos, más sube el cañón (curva editable por arma).
        float climb = weaponData.consecutiveShotClimb.Evaluate(consecutiveShots);
        consecutiveShots++;

        // Impulso instantáneo (kick): sube y se desvía lateralmente.
        kickRotation += new Vector3(
            -Random.Range(weaponData.recoilAmountZ * 0.6f, weaponData.recoilAmountZ) * climb,
            Random.Range(-weaponData.recoilAmountY, weaponData.recoilAmountY),
            0f
        );

        // Empuje hacia atrás en el eje local del arma (culatazo).
        kickPosition += Vector3.back * weaponData.recoilBack;

        rotationErrorPending = true;
        Shot = true;
    }

    public void ApplyRotationErrorDelay()
    {
        if (weaponData == null) return;

        kickRotation += new Vector3(
            Random.Range(-weaponData.rotationErrorAmount, weaponData.rotationErrorAmount),
            Random.Range(-weaponData.rotationErrorAmount, weaponData.rotationErrorAmount),
            0f
        );

        kickPosition += new Vector3(
            Random.Range(-weaponData.rotationErrorAmount * 0.2f, weaponData.rotationErrorAmount * 0.2f),
            Random.Range(-weaponData.rotationErrorAmount * 0.2f, weaponData.rotationErrorAmount * 0.2f),
            0f
        );
    }

    public void Swing(bool isRun)
    {
        if (weaponData == null) return;

        // Perlin noise en vez de Sin puro: evita el patrón periódico detectable
        // de una oscilación senoidal y da una respiración/paso más orgánico.
        float speed = isRun ? weaponData.runSwingAmount : weaponData.swingAmount;
        float t = Time.time * speed;

        float swingX = (Mathf.PerlinNoise(t, 0.37f) - 0.5f) * 1f;
        float swingY = (Mathf.PerlinNoise(0.91f, t) - 0.5f) * 1f;

        kickRotation += new Vector3(swingX, swingY, 0f) * Time.fixedDeltaTime * speed;
    }

    public void ApplyEffect()
    {
        GameObject g = Instantiate(shell, ShellPoint.position, ShellPoint.rotation);
        g.GetComponent<Rigidbody>().AddForce(g.transform.right, ForceMode.Impulse);
        Destroy(g, 25f);
    }

    public void Shoot()
    {
        if (weaponData != null)
        {
            nextFireTime = Time.time + 1f / weaponData.fireRate;
        }

        if (ammo > 0)
        {
            gatillo = true;
            ammo--;
            if (effOff) return;
            source.clip = shotClip;
            source.Play();
            boquilla.Play();
            side.Play();
            StartCoroutine(LightEffect());
        }
        else
        {
            source.clip = emptyClip;
        }
    }

    IEnumerator LightEffect()
    {
        //Intensity changes over time 15-->10>5>2>0
        yield return new WaitForSeconds(0.1f);
        light.enabled = true;
        light.intensity = 15;
        yield return new WaitForSeconds(0.1f);
        light.intensity = 10;
        yield return new WaitForSeconds(0.1f);
        light.intensity = 5;
        light.enabled = false;
    }

    public void Reload()
    {
        StartCoroutine(ReloAnim());
    }

    IEnumerator ReloAnim()
    {
        ActualPos = ReloadPos;
        ActualRot = ReloadRot;
        float a = 0.1f;
        procHands.SetLeftDestination(Anclas[2]);
        procHands.SetLeftWeight(a);
        ammo = MaxAmmo;
        while (a < 1)
        {
            a = a + 0.1f;
            procHands.SetLeftWeight(a);
            yield return new WaitForSeconds(0.1f);
            Debug.Log("estoy");
        }
        a = 0;
        procHands.SetLeftWeight(a);
        procHands.SetLeftDestination(Anclas[1]);
        Unsight();
        while (a < 1)
        {
            a = a + 0.1f;
            procHands.SetLeftWeight(a);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Apuntar()
    {
        ActualPos = SightPos;
        ActualRot = RestRot;
    }

    public void Unsight()
    {
        ActualPos = RestPos;
        ActualRot = RestRot;
    }

    internal void PutAway()
    {
        procHands.SetLeftWeight(0);
        procHands.SetRightWeight(0);
    }
}