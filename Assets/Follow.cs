using UnityEngine;

using UnityEngine.InputSystem;

public class Follow : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;

    [Header("Settings")]
    public float sensitivity = 10f;
    public float minX = -80f;
    public float maxX = 80f;

    public float yRotation = 0f;
    public InputAction LookAct;
    [SerializeField] private InputActionAsset controls;
    
    [SerializeField]Weapon weapon;

    void Awake()
    {
        LookAct     = controls.FindAction("Player/Look");
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        LookAct.Enable();
    }

    private void OnDisable()
    {
        LookAct.Disable();
    }
    
    void FixedUpdate()
    {
        Vector2 input = LookAct.ReadValue<Vector2>();
        Vector2 mouse = new Vector2(input.x* sensitivity * Time.fixedDeltaTime, input.y* sensitivity * Time.fixedDeltaTime);
        if(mouse.x!=0&&weapon)weapon.ApplyRotationErrorDelay();
        yRotation -= mouse.y;
        yRotation = Mathf.Clamp(yRotation, minX, maxX);
        transform.localRotation = Quaternion.Euler(yRotation, -90f, 0f);
        playerBody.Rotate(Vector3.up * mouse.x);        
    }
}


