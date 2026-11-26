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
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedForce = -2f;

        private CharacterController characterController;
        private Vector2 movementInput;
        private float verticalVelocity;

        public bool IsGrounded { get; private set; }
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateMovement();
        }

        private void ReadMovementInput()
        {
            movementInput = Vector2.zero;

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
        }

        private void UpdateMovement()
        {
            IsGrounded = characterController.isGrounded;

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedForce;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 movementDirection = new Vector3(
                movementInput.x,
                0f,
                movementInput.y
            );

            IsMoving = movementDirection.sqrMagnitude > 0.01f;

            if (IsMoving)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(movementDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(
                        -rotationSpeed * Time.deltaTime
                    )
                );
            }

            Vector3 velocity = movementDirection * walkSpeed;
            velocity.y = verticalVelocity;

            CollisionFlags collisionFlags =
                characterController.Move(
                    velocity * Time.deltaTime
                );

            IsGrounded =
                (collisionFlags & CollisionFlags.Below) != 0;

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedForce;
            }
        }
    }
}