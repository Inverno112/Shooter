using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour,IDamagable
{

    [Header("Stats")]
    [SerializeField] private byte resistencia;
    [SerializeField] private byte agility;
    [SerializeField] private byte Cansancio;
    [SerializeField] private byte clarity;
    [SerializeField] private byte hunger;
    [SerializeField] private byte thirst;
    [SerializeField] private byte health;
    [SerializeField] private byte life;
    [SerializeField] private byte olor;

  [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravityValue = -9.81f;
    

    [Header("Input")]
    [SerializeField] private InputActionAsset controls;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction fightAction;
    private InputAction rightAction;
    private InputAction reloadAction;
    private InputAction LightAction;
    private InputAction sprintAction;
    private InputAction outAction;

    [Header("References")]
    public CharacterController controller;
    [SerializeField]LayerMask layerMask;
    [SerializeField]LayerMask takeLayer;
    [SerializeField]LayerMask actionLayer;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Effects System")]
    AudioSource playerAS;
    [SerializeField]AudioClip[] playerClips;//take,throw,action,recharge

    [Header("Interact System")]
    [SerializeField]Transform handPoint;
    [SerializeField]Camera cam;
    private int wallet;
    private int chkprice;
    [SerializeField]Weapon weapon;
    [SerializeField]Transform l;
    [SerializeField]Animator animator;
    

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
        fightAction.Enable();
        rightAction.Enable();
        reloadAction.Enable();
        LightAction.Enable();
        sprintAction.Enable();
        outAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        interactAction.Disable();
        fightAction.Disable();
        rightAction.Disable();
        reloadAction.Disable();
        LightAction.Disable();
        sprintAction.Disable();
        outAction.Disable();
    }
    void Awake()
    {
        moveAction     = controls.FindAction("Player/Move");
        jumpAction     = controls.FindAction("Player/Jump");
        interactAction = controls.FindAction("Player/Interact");
        fightAction    = controls.FindAction("Player/Attack");
        rightAction    = controls.FindAction("Player/RightTab");
        reloadAction   = controls.FindAction("Player/Reload");
        LightAction    = controls.FindAction("Player/Light");
        sprintAction   = controls.FindAction("Player/Sprint");
        outAction      = controls.FindAction("Player/PutAway");
    }
    void Start()
    {       
        animator=GetComponent<Animator>();
        controller=GetComponent<CharacterController>();
        playerAS=GetComponent<AudioSource>();
        life=health;
    }

    void Update()//Lógica de interacción
    {
        // Jump logic
        if (groundedPlayer && jumpAction.WasPressedThisFrame())
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
        //Use E to interact, use right button to take items, use left button to throw items
        //system uses raycast to detect interactable objects
        if (moveAction.WasPerformedThisFrame())
        {
            weapon.Swing(false);
            animator.SetInteger("action",1);
        }else{animator.SetInteger("action",0);}
        
        
        if(fightAction.IsPressed())
        {
           if(weapon.PullTrigger())
            Interact(0);
        }
        if(interactAction.IsPressed())
        {            
            Interact(1);
        }
        if(reloadAction.WasPerformedThisFrame())
        {
            weapon.Reload();
            animator.Play("Reload");
        }
        if(rightAction.WasPerformedThisFrame())
        {
            weapon.Apuntar();            
        }
        if(rightAction.WasReleasedThisFrame()){
            weapon.Unsight();
        }
        if (LightAction.WasPerformedThisFrame())
        {
            l.gameObject.SetActive(!l.gameObject.activeSelf);
        }
        if(outAction.WasPerformedThisFrame())
        {
            weapon.PutAway();
        }
    }


    void FixedUpdate()//movimiento
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Slight downward velocity to stay grounded
        }

        // Read input
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move = Vector3.ClampMagnitude(move, 1f);

        // Apply gravity
        playerVelocity.y += gravityValue * Time.fixedDeltaTime;

        // Move the character
        Vector3 finalMove = move * moveSpeed + Vector3.up * playerVelocity.y;
        controller.Move(finalMove * Time.fixedDeltaTime);
        
    }

    private void Interact(byte intType)
    {
        //intType 0=Attack,1=use,2=throw       
        
        RaycastHit hitInfo;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 10f,layerMask))
        {
            switch (intType)
            {
                case 0:
                   hitInfo.collider.GetComponent<IDamagable>()?.Dmg(0, transform);
                    break;
                case 1:
                    hitInfo.collider.GetComponent<IInteractable>()?.Interact(transform);
                    break;
            }
        }
    }

    public void Dmg(byte Dmg, Transform source)
    {
        if(life-Dmg>0)
        {
            life -= Dmg;
        }
        else
        {
            life = 0;
            animator.SetTrigger("Die");            
            Destroy(gameObject,50);
            enabled = false;
        }
    }
}
