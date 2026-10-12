using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour, IInteractable
{   

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform lightPad;
    [SerializeField] private Transform CamField;
    [SerializeField] private InputActionAsset controls;

    [Header("Movement")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxForwardSpeed = 15f;
    [SerializeField] private float maxBackwardSpeed = 25f;
    [SerializeField] private float maxTurnSpeed = 180f;
    [SerializeField] private float turnAcceleration = 5f;
    [SerializeField] private float brakeForce = 15f;

    private Rigidbody rb;
    [SerializeField]private float currentTurnSpeed;
    [SerializeField]private float currentTargetSpeed;

[Header("State")]
    [SerializeField]private bool driving;
    [SerializeField]private bool carOn;

    [Header("Input")]
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction lightAction;

private AudioSource audioSource;

[Tooltip("Engine sound clips: 0 - Idle, 1 - Driving, 2 - Start")]
[SerializeField]private AudioClip[] engineSounds=new AudioClip[3];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
            driving=false;
        audioSource = GetComponent<AudioSource>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        currentTurnSpeed = 0;
        carOn = false;

        moveAction = controls.FindAction("Player/Move");
        jumpAction = controls.FindAction("Player/Jump");
        interactAction = controls.FindAction("Player/Interact");
        lightAction    = controls.FindAction("Player/Light");
         if (moveAction == null) Debug.LogError("CarController: 'Player/Move' action not found. Check Action Map name.");
    if (jumpAction == null) Debug.LogError("CarController: 'Player/Jump' action not found.");
    if (interactAction == null) Debug.LogError("CarController: 'Player/Interact' action not found.");
    if (lightAction == null) Debug.LogError("CarController: 'Player/Light' action not found.");
    Debug.LogError("CarController: All load");
    OnEnable();

    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
        lightAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
        lightAction.Disable();
    }

    //---------------------------------------------------
    // ENTRAR AL VEHÍCULO
    //---------------------------------------------------

    public void Interact(Transform obj)
    {
        if (driving)
            return;

        player = obj;
        StartCoroutine(EnterVehicle());
        // Ocultamos al jugador
        player.gameObject.SetActive(false);
        CamField.gameObject.SetActive(true);
    }

    IEnumerator EnterVehicle()
    {
        yield return new WaitForSeconds(5f);
        driving = true;
        OnEnable();
        
    }

    //---------------------------------------------------
    // SALIR DEL VEHÍCULO
    //---------------------------------------------------

    private void ExitVehicle()
    {
        
        CamField.gameObject.SetActive(false);
        player.position = exitPoint.position;

        player.gameObject.SetActive(true);

        player = null;
        driving = false;
    }

    //---------------------------------------------------
    // MOVIMIENTO
    //---------------------------------------------------

    private void FixedUpdate()
    {
        if (!carOn)
        return;

        Vector2 input = moveAction.ReadValue<Vector2>();

        float direction = input.y;
        float steering = input.x;
        
            if (direction > 0)
            {
                PlaySound(1,false);
                currentTargetSpeed = Mathf.Lerp(currentTargetSpeed, maxForwardSpeed, acceleration * Time.fixedDeltaTime);
            }
            else if(direction<0)
            {
                PlaySound(1,false);
                currentTargetSpeed = Mathf.Lerp(currentTargetSpeed, -maxBackwardSpeed, acceleration * Time.fixedDeltaTime);
            }
            else{
                PlaySound(0,true);
                currentTargetSpeed = Mathf.Lerp(currentTargetSpeed, 0, brakeForce * Time.fixedDeltaTime);
                if(currentTargetSpeed<1f)currentTargetSpeed=0;
            }
            rb.AddForce(transform.forward * currentTargetSpeed , ForceMode.Acceleration);
            
        
        //--------------------------------
        // Girar
        //--------------------------------
        if(Mathf.Abs(currentTargetSpeed)>5){
            // Incrementar turnSpeed si hay input de steering
            if (Mathf.Abs(steering) > 0.1f)
            {
                currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, maxTurnSpeed, turnAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Reducir turnSpeed si no hay input
                currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, 0, turnAcceleration*10 * Time.fixedDeltaTime);
            }

            float angle = steering * currentTurnSpeed * Time.fixedDeltaTime;

            rb.MoveRotation(
                rb.rotation * Quaternion.Euler(0, angle, 0));
        }

        if (jumpAction.IsPressed())
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                Vector3.zero,
                brakeForce * Time.fixedDeltaTime);
        }
    }
    

    //---------------------------------------------------
    // INPUTS
    //---------------------------------------------------

    private void Update()
    {
        if (!driving)
            return;
        Debug.Log(interactAction.WasPerformedThisFrame());
        if (interactAction.WasPerformedThisFrame())
        {
            Debug.Log("I Was here exit"+driving);
            ExitVehicle();

        }
        
        if (lightAction.WasPerformedThisFrame())
        {
            carOn = !carOn;
            lightPad.gameObject.SetActive(carOn);
            if(carOn)PlaySound(2,true);
            Debug.Log("I Was here CAr"+carOn);
        }
    }
    void PlaySound(byte soundIndex,bool stopIt)
    {
        if(!audioSource)
        {
            return;
        }
        if(stopIt)
        {
            audioSource.Stop();
            return;
        }
        if (soundIndex < 0 || soundIndex >= engineSounds.Length||audioSource.isPlaying)
        {
            Debug.LogWarning("Invalid sound index: " + soundIndex);
            return;
        }
        

        audioSource.clip = engineSounds[soundIndex];
        audioSource.Play();
    }
}


