
using UnityEngine;

internal interface ITiming
{
    void Timer(float a);
}

internal interface IAction
{
    void Action();
    void LeftAction();
}
internal interface IInteractable
{
    void Interact(Transform player);
}

internal interface IGet
{
        Item GetThing();
}

internal interface  IGive
{
        void GiveThing(Item receiver);
}

internal interface IDamagable
{
        void Dmg(byte Dmg,Transform source);
}

internal interface IDetection
{
    void Detect(Transform player);
}

internal interface IAnimal
{
    void Eat();
    void MoveTo(Vector3 destination);
    void MoveHome();
    void Die();
    void Attack();

    Transform AlphaIn(Transform al);
    void AlphaOut();
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "My Game/Item", order = 1)]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite itemIcon = null;
    public int item_Category = 0;//0=default, 1=seed, 2=food, 3=tool, 4=material
    public int itemValue = 0;
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "My Game/Arma", order = 1)]
public class Arma : Item
{
   public byte Dmg;

   GameObject prefab;
}
[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "My Game/Arma", order = 1)]
public class ArmaRango : Item
{
   [SerializeField] private float range = 50f;                 // Rango máximo del raycast
    [SerializeField] private byte damage = 25;                  // Daño por disparo
    [SerializeField] private byte maxAmmo = 30;                 // Capacidad máxima del cargador
    [SerializeField] private float fireRate = 0.15f;            // Tiempo entre disparos (segundos)
    [SerializeField] private float reloadTime = 2.2f;           // Duración de la recarga
    [SerializeField] private float recoilAmount = 0.08f;        // Intensidad del recoil visual
    [SerializeField] private float recoilRecoverySpeed = 5f;
       
    GameObject prefab;
}


/// <summary>
/// Contenedor de datos de un arma: posiciones/rotaciones de pose,
/// parámetros de recoil, munición y clips de audio.
/// Crear instancias vía: Assets > Create > Weapons > Weapon Position Data
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Position Data")]
public class WeaponPositionData : ScriptableObject
{
    [Header("Pose Base")]
    public Vector3 RestPos;
    public Vector3 RestRot;
    public Vector3 SightPos;

    [Header("Pose de Recarga")]
    public Vector3 ReloadPos;
    public Vector3 ReloadRot;

    [Header("Recoil - Kick (subida instantánea por disparo)")]
    [Range(0.05f, 80f)] public float recoilAmountZ = 8f;
    [Range(0.05f, 15f)] public float recoilAmountY = 2f;
    [Range(0.1f, 5f)] public float recoilBack = 1f;

    [Header("Recoil - Dinámica de retorno")]
    [Tooltip("Tiempo aproximado (segundos) que tarda el kick en asentarse. Menor = más seco/rígido.")]
    [Range(0.01f, 1f)] public float settleTime = 0.12f;

    [Tooltip("Multiplicador del kick vertical según nº de disparos consecutivos (eje X = disparo, eje Y = multiplicador).")]
    public AnimationCurve consecutiveShotClimb = AnimationCurve.Linear(0, 1f, 10, 2.2f);

    [Tooltip("Tiempo sin disparar tras el cual se reinicia el contador de ráfaga.")]
    [Range(0.05f, 1f)] public float burstResetTime = 0.35f;

    [Header("Sway / Respiración")]
    [Range(0.1f, 10f)] public float swingAmount = 5f;
    [Range(0.1f, 10f)] public float runSwingAmount = 3f;
    [Range(0.01f, 5f)] public float rotationErrorAmount = 1f;
    [Range(0f, 1f)] public float rotationErrorDelayTime = 0.05f;

    [Header("Munición")]
    public byte MaxAmmo = 8;

    [Header("Cadencia")]
    [Tooltip("Disparos por segundo. También limita el semi-auto si se pulsa más rápido que esto.")]
    [Range(0.5f, 20f)] public float fireRate = 6f;

    [Header("Audio")]
    public AudioClip shotClip;
    public AudioClip emptyClip;
    public AudioClip reloadClip;
}