using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementStates
{
    Stopped,
    Walking,
    Running
}

public class MovementPC : MonoBehaviour
{
    [Header("Move Stats")]
    [Range(0, 100)]
    public float moveSpeed;
    float currentMoveSpeed = 0;
    [Tooltip("Time to reach full speed")]
    [Range(0, 4)]
    public float accelerationLength;
    [Tooltip("Time to reach full stop after walking")]
    [Range(0, 4)]
    public float decelerationLengthWalking;
    [Tooltip("Time to reach full stop after running")]
    [Range(0, 4)]
    public float decelerationLengthRunning;
    [Tooltip("Multiply speed by this when quickly changing ground plane direction")]
    [Range(0,1)]
    public float directionSwitchSpeedMultiplier = .25f;
    [Tooltip("Multiply speed by this when moving backwards")]
    [Range(0, 1)]
    public float backwardsSpeedMultiplier = .5f;

    bool isAccel;
    bool isDecel;
    float accelTimer;
    float accelProgress;
    float speedOrigin;

    [SerializeField] CharacterController cc;
    [SerializeField] PlayerCharacter pc;
    Vector3 groundPlaneMovement;
    Vector3 previousGroundPlaneMovement;
    Vector3 moveDirection;
    Vector2 moveInput;

    public MovementStates movementState = MovementStates.Stopped;


    // ground plane movement
    bool left;
    bool right;
    bool forward;
    bool back;

    void Update()
    {
        // TODO make input manager
        #region Input Manager
        // WASD down
        if (Input.GetKeyDown(KeyCode.W))
        {
            forward = true;
            back = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            left = true;
            right = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            back = true;
            forward = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            right = true;
            left = false;
        }

        // WASD up
        if (Input.GetKeyUp(KeyCode.W)) forward = false;
        if (Input.GetKeyUp(KeyCode.A)) left = false;
        if (Input.GetKeyUp(KeyCode.S)) back = false;
        if (Input.GetKeyUp(KeyCode.D)) right = false;
        #endregion

        if (!forward && !back)
        {
            moveInput.y = 0;
        }
        else if (forward) moveInput.y = 1;
        else moveInput.y = -1;

        if (!left && !right)
        {
            moveInput.x = 0;
        }
        else if (left) moveInput.x = -1;
        else moveInput.x = 1;

        if (moveInput != Vector2.zero)
        {
            // if some input detected
            groundPlaneMovement.x = moveInput.x;
            groundPlaneMovement.z = moveInput.y;

            groundPlaneMovement = Camera.main.transform.TransformDirection(groundPlaneMovement);
            groundPlaneMovement.y = 0;
            groundPlaneMovement.Normalize();

            // if was stopped begin accelerating PC movement
            if (movementState == MovementStates.Stopped)
            {
                if (!isAccel)
                {
                    StartCoroutine("RampUpSpeed");

                    // TODO decide whether to include running
                }
            }
            else
            {
                // if change of direction is detected while moving
                if (previousGroundPlaneMovement != Vector3.zero
                    && Vector3.Dot(groundPlaneMovement, previousGroundPlaneMovement) < 0)
                {
                    SwitchDirection();
                }
            }
        }
        else
        {
            if (movementState == MovementStates.Walking)
            {
                if (!isDecel)
                {
                    StartCoroutine("RampDownSpeedWalking");
                }
            }
            else if (movementState == MovementStates.Running)
            {
                // TODO running?
            }
        }

        previousGroundPlaneMovement = groundPlaneMovement;
    }

    private void LateUpdate()
    {
        moveDirection += groundPlaneMovement;
        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            cc.Move((moveDirection * currentMoveSpeed * Time.deltaTime));
            moveDirection = Vector3.zero;
        }
    }

    #region Lerp Speed
    IEnumerator RampUpSpeed()
    {
        StopCoroutine("RampDownSpeedWalking");
        isAccel = true;
        isDecel = false;
        accelProgress = pc.cameraRotator.camFollowAnim.speed = 1 - (moveSpeed - currentMoveSpeed) / moveSpeed;
        accelTimer = accelerationLength * accelProgress;
        speedOrigin = currentMoveSpeed;
        movementState = MovementStates.Walking;
        pc.cameraRotator.camFollowAnim.SetBool("isWalking", true);

        while (accelProgress < 1)
        {
            accelTimer += Time.deltaTime;
            accelProgress = accelTimer / accelerationLength;
            pc.cameraRotator.camFollowAnim.speed = accelProgress * (back ? backwardsSpeedMultiplier : 1);

            currentMoveSpeed = Mathf.Lerp(speedOrigin, back ? moveSpeed * backwardsSpeedMultiplier : moveSpeed, accelProgress); // if moving backwards apply multiplier

            yield return null;
        }
        pc.cameraRotator.camFollowAnim.speed = back ? backwardsSpeedMultiplier : 1;
        currentMoveSpeed = back ? moveSpeed * backwardsSpeedMultiplier : moveSpeed;
        isAccel = false;
    }

    IEnumerator RampDownSpeedWalking()
    {
        StopCoroutine("RampUpSpeed");
        isDecel = true;
        isAccel = false;
        accelProgress = (moveSpeed - currentMoveSpeed) / moveSpeed;
        pc.cameraRotator.camFollowAnim.speed = accelProgress * (back ? backwardsSpeedMultiplier : 1);
        accelTimer = decelerationLengthWalking * accelProgress;
        speedOrigin = currentMoveSpeed;

        while (accelProgress < 1)
        {
            accelTimer += Time.deltaTime;
            accelProgress = pc.cameraRotator.camFollowAnim.speed = accelTimer / decelerationLengthWalking;

            currentMoveSpeed = Mathf.Lerp(speedOrigin, 0, accelProgress);

            yield return null;
        }

        currentMoveSpeed = pc.cameraRotator.camFollowAnim.speed = 0;
        isDecel = false;
        movementState = MovementStates.Stopped;
        pc.cameraRotator.camFollowAnim.SetBool("isWalking", false);
    }

    void SwitchDirection()
    {
        StopCoroutine("RampDownSpeedWalking");
        StopCoroutine("RampUpSpeed");
        currentMoveSpeed *= directionSwitchSpeedMultiplier;
        StartCoroutine("RampUpSpeed");
    }
    #endregion
}
