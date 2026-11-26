using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvationJourney.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStateController))]
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedForce = -2f;

        [Header("Stamina")]
        [SerializeField] private float sprintStaminaCost = 15f;
        [SerializeField] private float staminaRegenRate = 10f;
        [SerializeField] private float staminaRegenDelay = 1.5f;
        [SerializeField] private float sprintRecoveryThreshold = 20f;

        private CharacterController characterController;
        private PlayerStateController playerState;

        private Vector2 movementInput;
        private float verticalVelocity;

        private bool sprintRequested;
        private bool sprintExhausted;
        private float staminaRegenTimer;

        public bool IsGrounded { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsSprinting { get; private set; }

        public bool IsExhausted => sprintExhausted;

        private void Awake()
        {
            characterController =
                GetComponent<CharacterController>();

            playerState =
                GetComponent<PlayerStateController>();
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateSprintState();
            UpdateMovement();
            UpdateStaminaRegeneration();
        }

        private void ReadMovementInput()
        {
            movementInput = Vector2.zero;
            sprintRequested = false;

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            if (keyboard.wKey.isPressed ||
                keyboard.upArrowKey.isPressed)
            {
                movementInput.y += 1f;
            }

            if (keyboard.sKey.isPressed ||
                keyboard.downArrowKey.isPressed)
            {
                movementInput.y -= 1f;
            }

            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
            {
                movementInput.x -= 1f;
            }

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
            {
                movementInput.x += 1f;
            }

            movementInput = Vector2.ClampMagnitude(
                movementInput,
                1f
            );

            sprintRequested =
                keyboard.leftShiftKey.isPressed ||
                keyboard.rightShiftKey.isPressed;

            IsMoving =
                movementInput.sqrMagnitude > 0.01f;
        }

        private void UpdateSprintState()
        {
            IsSprinting = false;

            if (sprintExhausted &&
                playerState.Stamina >= sprintRecoveryThreshold &&
                !sprintRequested)
            {
                sprintExhausted = false;
            }

            if (!sprintRequested ||
                !IsMoving ||
                sprintExhausted)
            {
                return;
            }

            float staminaCost =
                sprintStaminaCost * Time.deltaTime;

            if (playerState.SpendStamina(staminaCost))
            {
                IsSprinting = true;

                staminaRegenTimer =
                    staminaRegenDelay;

                if (playerState.Stamina <= 0f)
                {
                    sprintExhausted = true;
                }
            }
            else
            {
                playerState.SpendStamina(
                    playerState.Stamina
                );

                sprintExhausted = true;

                staminaRegenTimer =
                    staminaRegenDelay;
            }
        }

        private void UpdateMovement()
        {
            IsGrounded =
                characterController.isGrounded;

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedForce;
            }

            verticalVelocity +=
                gravity * Time.deltaTime;

            Vector3 movementDirection = new Vector3(
                movementInput.x,
                0f,
                movementInput.y
            );

            if (IsMoving)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        movementDirection
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        1f - Mathf.Exp(
                            -rotationSpeed *
                            Time.deltaTime
                        )
                    );
            }

            float currentSpeed =
                IsSprinting
                    ? sprintSpeed
                    : walkSpeed;

            Vector3 velocity =
                movementDirection * currentSpeed;

            velocity.y = verticalVelocity;

            CollisionFlags collisionFlags =
                characterController.Move(
                    velocity * Time.deltaTime
                );

            IsGrounded =
                (collisionFlags &
                 CollisionFlags.Below) != 0;

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedForce;
            }
        }

        private void UpdateStaminaRegeneration()
        {
            if (IsSprinting)
                return;

            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -=
                    Time.deltaTime;

                return;
            }

            playerState.RestoreStamina(
                staminaRegenRate * Time.deltaTime
            );
        }
    }
}