using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{

    // ------------------------ Scene Names ---------------------------

    private const string winningScreenName = "PlayerWinningScreen";

    // ------------------------ Player's Character Variables ---------------------------

    private Animator playerAnimator;
    private Rigidbody2D playerRigidBody;
    private Collider2D playerCollider;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float hurtForce = 10f;

    // ------------------------ Animation State Machine Variables ---------------------------

    private enum State { idle, running, jumping, falling };
    private State playerState;

    // Inspector variables
    // ------------------------ General Variables ---------------------------

    [SerializeField] private LayerMask ground;


    /// <summary>
    /// The function initializes the player's character variables. The Start function is called before the first frame 
    /// update.
    /// </summary>
    void Start()
    {
        // Initialize parameters
        playerRigidBody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        playerState = State.idle;
    }

    /// <summary>
    /// The function Handles the player's movement and animation. The function is called once per frame.
    /// </summary>
    void Update()
    {
        Movement();
        // Change the player's states accordingly
        AnimationState();
        // Change the animation according to the player's states
        playerAnimator.SetInteger("state", (int)playerState);
    }

    /// <summary>
    /// The function handles the player collisions wiht the environment. 
    /// The function is called when the player collides with something.
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        // If the player touches the spikes restart level
        if (other.gameObject.tag == "Danger")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (other.gameObject.tag == "Destination")
        {
            SceneManager.LoadScene(winningScreenName);
        }
    }

    /// <summary>
    /// The function gives the player velocity according to its actions. 
    /// The function is called every frame by the Update function.
    /// </summary>
    private void Movement()
    {
        // Input.GetAxis("Horizontal") gives a value between -1 and 1.
        // If the player moved left the value will be bewteen (-1)-0 and if the player moved right the value will be bewteen 0-1 
        // and 0 if the player didn't move on the horizontal axis.
        float hDirection = Input.GetAxis("Horizontal");

        // If the player moved left
        if (hDirection < 0)
        {
            // If the player is trying to stop falling by running on a surface on the side of the surface - don't move (not allowed)
            if (playerRigidBody.velocity.y < -0.3f && playerCollider.IsTouchingLayers(ground))
            {
                return;
            }
            // We dont want to affect the y velocity so it stays the same
            playerRigidBody.velocity = new Vector2(-speed, playerRigidBody.velocity.y);
            // The x is -1 so the sprite will turn left (we don't touch the y) - the player is facing left
            transform.localScale = new Vector3(-1, 1);
        }
        // If the user moved right
        else if (hDirection > 0)
        {
            // If the player is trying to stop falling by running on a surface on the side of the surface - don't move (not allowed)
            if (playerRigidBody.velocity.y < -0.3f && playerCollider.IsTouchingLayers(ground))
            {
                return;
            }
            // We dont want to affect the y velocity so it stays the same
            playerRigidBody.velocity = new Vector2(speed, playerRigidBody.velocity.y);
            // The x is 1 so the sprite will turn right (we don't touch the y) - the player is facing right
            transform.localScale = new Vector3(1, 1);
        }

        // Here we use the Jump blockButton that is declared in the input manager system
        if (Input.GetButtonDown("Jump") && playerCollider.IsTouchingLayers(ground))
        {
            Jump();
        }
    }

    /// <summary>
    /// The function handles the jump movement of the player - gives the player velocity upwards and changes its states.
    /// </summary>
    private void Jump()
    {
        // We dont want to affect the x velocity
        playerRigidBody.velocity = new Vector2(playerRigidBody.velocity.x, jumpForce);
        playerState = State.jumping;
    }

    /// <summary>
    /// The function changes the player's animation states according to its velocity.
    /// The function is called every frame by the Update function.
    /// </summary>
    private void AnimationState()
    {
        // Jump and fall animations have priority over running and idle animations because they are first in the if statements
        if (playerState == State.jumping)
        {
            // If the player stops jumping and starts falling
            if (playerRigidBody.velocity.y < 0.1f)
            {
                playerState = State.falling;
            }
        }
        // If the player isn't jumping and isn't touching the ground it falls
        else if (!playerCollider.IsTouchingLayers(ground))  // good to check if the player is falling
        {
            // If the player has velocity down change to falling animation 
            playerState = State.falling;
        }
        else if(playerState == State.falling)
        {
            // If we gameOn falling change states to idle
            if (playerCollider.IsTouchingLayers(ground))
            {
                playerState = State.idle;
            }
        }
        // If the player has velocity in the x axis change to running states
        else if (Mathf.Abs(playerRigidBody.velocity.x) > 2f)
        {
            playerState = State.running;
        }
        // If the player isn't moving change to idle states
        else
        {
            playerState = State.idle;
        }
    }

}
