using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //input system
    InputSystem_Actions input_;
    Vector2 direction;

    //character controller
    CharacterController cc;
    Animator animator;

    //garavity
    float gravity = -9.81f;
    float yVelocity;
    float jumpForce = 3f;
    bool canAction = true;
    bool wasGrounded = false;

    //camera
    public Transform camera;

    private void OnEnable()
    {
        input_ = new InputSystem_Actions();
        input_.Enable();

        input_.Player.Move.performed += OnMove;
        input_.Player.Move.canceled += OnMove;
        input_.Player.Jump.performed += OnJump;
        input_.Player.Attack.performed += OnAttack;
        input_.Player.Dash.performed += OnDash;
    }

    private void OnDisable()
    {
        input_.Player.Move.performed -= OnMove;
        input_.Player.Move.canceled -= OnMove;
        input_.Player.Jump.performed -= OnJump;
        input_.Player.Attack.performed -= OnAttack;
        input_.Player.Dash.performed -= OnDash;
        input_.Disable();
    }
    void OnDash(InputAction.CallbackContext context)
    {

    }
    void OnAttack(InputAction.CallbackContext context)
    {
        int rd = Random.Range(0, 3);
        switch (rd)
        {
            case 0:
                animator.SetTrigger("attack1");
                break;
            case 1:
                animator.SetTrigger("attack2");
                break;
            case 2:
                animator.SetTrigger("attack3");
                break;
        }
    }
    void DelayAfterLanding()
    {
        StartCoroutine(DelayLandingCoroutine());
    }

    IEnumerator DelayLandingCoroutine()
    {
        canAction = false;

        yield return new WaitForSeconds(1f);

        canAction = true;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && cc.isGrounded && canAction)
        {
            animator.SetTrigger("jump");
            yVelocity = jumpForce;
        }
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Phát hiện nhân vật vừa chạm đất
        if (cc.isGrounded && !wasGrounded)
        {
            DelayAfterLanding();
        }

        wasGrounded = cc.isGrounded;

        Vector3 camF = camera.forward;
        Vector3 camR = camera.right;

        camF.y = 0f;
        camR.y = 0f;

        camF.Normalize();
        camR.Normalize();

        Vector3 move = camF * direction.y + camR * direction.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        // Gravity
        if (cc.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * 5f;

        // Khóa chạy trong 1 giây
        if (!canAction)
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        velocity.y = yVelocity;

        cc.Move(velocity * Time.deltaTime);

        // Rotate player
        if (direction.magnitude > 0 && canAction)
        {
            move.y = 0;

            Quaternion look = Quaternion.LookRotation(move, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                360f * Time.deltaTime
            );

            animator.SetBool("speed", true);
        }
        else
        {
            animator.SetBool("speed", false);
        }
    }
}