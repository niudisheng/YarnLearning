using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public BaseBullet bulletPrefab;
    [SerializeField] private BulletData bulletData;
    private PlayerInput playerInput;
    private Rigidbody2D rb;
    [SerializeField] private float speed;

    [SerializeField]
    private Vector2 direction
    {
        set=> Move(value);
    }

    private System.Action<InputAction.CallbackContext> moveAction;
    private System.Action<InputAction.CallbackContext> moveCanceledAction;
    private System.Action<InputAction.CallbackContext> shootAction;
    
    private void Awake()
    {
        playerInput = new PlayerInput();
        rb = GetComponent<Rigidbody2D>(); 
        
        // 存储回调引用以便正确取消订阅
        moveAction = ctx =>
        {
            direction = ctx.ReadValue<Vector2>();
        };
        moveCanceledAction = ctx =>
        {
            direction = Vector2.zero;
        };
        shootAction = ctx => Shoot();
    }
    
    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        playerInput.Enable();
        playerInput.Map1.Move.performed += moveAction;
        playerInput.Map1.Move.canceled += moveCanceledAction;
        playerInput.Map1.Shoot.performed += shootAction;
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Map1.Move.performed -= moveAction;
            playerInput.Map1.Move.canceled -= moveCanceledAction;
            playerInput.Map1.Shoot.performed -= shootAction;
            playerInput.Disable();
        }
    }

    

    private void Move(Vector2 direction1)
    {
        Debug.Log(direction1);
        rb.linearVelocity = direction1 * speed;
    }

    [ContextMenu("Shoot")]
    public void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<BaseBullet>().Init(bulletData);
    }
}
