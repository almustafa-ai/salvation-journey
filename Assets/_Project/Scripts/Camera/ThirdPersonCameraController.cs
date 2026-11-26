using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvationJourney.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Camera Position")]
        [SerializeField] private float followDistance = 4.5f;
        [SerializeField] private float cameraHeight = 2f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Orbit Controls")]
        [SerializeField] private float horizontalSensitivity = 0.15f;
        [SerializeField] private float verticalSensitivity = 0.12f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 65f;

        [Header("Collision Avoidance")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private float collisionPadding = 0.15f;
        [SerializeField] private float minimumDistance = 0.5f;
        [SerializeField] private float collisionRecoveryTime = 0.25f;

        private Vector3 positionVelocity;

        private float yaw;
        private float pitch;
        private float orbitDistance;

        private float currentDistance;
        private float distanceVelocity;

        public bool IsObstructed { get; private set; }

        private void Start()
        {
            if (target == null)
                return;

            yaw = target.eulerAngles.y;

            pitch = Mathf.Atan2(
                cameraHeight,
                followDistance
            ) * Mathf.Rad2Deg;

            pitch = Mathf.Clamp(
                pitch,
                minPitch,
                maxPitch
            );

            orbitDistance = Mathf.Sqrt(
                followDistance * followDistance +
                cameraHeight * cameraHeight
            );

            currentDistance = orbitDistance;

            Vector3 desiredPosition = GetDesiredPosition();

            transform.position = ResolveCollision(
                desiredPosition,
                true
            );

            transform.LookAt(target.position);
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            ReadOrbitInput();
            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        private void ReadOrbitInput()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
                return;

            if (!mouse.rightButton.isPressed)
                return;

            Vector2 mouseDelta =
                mouse.delta.ReadValue();

            yaw +=
                mouseDelta.x * horizontalSensitivity;

            pitch -=
                mouseDelta.y * verticalSensitivity;

            yaw = Mathf.Repeat(yaw, 360f);

            pitch = Mathf.Clamp(
                pitch,
                minPitch,
                maxPitch
            );
        }

        private Vector3 GetDesiredPosition()
        {
            Quaternion orbitRotation =
                Quaternion.Euler(pitch, yaw, 0f);

            Vector3 offset =
                orbitRotation *
                new Vector3(0f, 0f, -orbitDistance);

            return target.position + offset;
        }

        private float GetSafeDistance(
            Vector3 direction,
            float requestedDistance)
        {
            if (Physics.SphereCast(
                target.position,
                collisionRadius,
                direction,
                out RaycastHit hit,
                requestedDistance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(
                    0.01f,
                    hit.distance - collisionPadding
                );
            }

            return requestedDistance;
        }

        private Vector3 ResolveCollision(
            Vector3 desiredPosition,
            bool immediate = false)
        {
            Vector3 offset =
                desiredPosition - target.position;

            float desiredDistance = offset.magnitude;

            if (desiredDistance < 0.001f)
                return desiredPosition;

            Vector3 direction = offset.normalized;

            float safeDistance = GetSafeDistance(
                direction,
                desiredDistance
            );

            IsObstructed =
                safeDistance < desiredDistance;

            float targetDistance = Mathf.Min(
                desiredDistance,
                Mathf.Max(
                    minimumDistance,
                    safeDistance
                )
            );

            // Collision safety takes priority over
            // the preferred minimum camera distance.
            targetDistance = Mathf.Min(
                targetDistance,
                safeDistance
            );

            if (immediate ||
                targetDistance < currentDistance)
            {
                currentDistance = targetDistance;
                distanceVelocity = 0f;
            }
            else
            {
                currentDistance = Mathf.SmoothDamp(
                    currentDistance,
                    targetDistance,
                    ref distanceVelocity,
                    collisionRecoveryTime
                );
            }

            return target.position +
                direction * currentDistance;
        }

        private void UpdateCameraPosition()
        {
            Vector3 desiredPosition =
                GetDesiredPosition();

            Vector3 collisionAdjustedPosition =
                ResolveCollision(desiredPosition);

            Vector3 smoothedPosition =
                Vector3.SmoothDamp(
                    transform.position,
                    collisionAdjustedPosition,
                    ref positionVelocity,
                    positionSmoothTime
                );

            Vector3 offset =
                smoothedPosition - target.position;

            float distance = offset.magnitude;

            if (distance > 0.001f)
            {
                Vector3 direction = offset.normalized;

                float safeDistance = GetSafeDistance(
                    direction,
                    distance
                );

                if (safeDistance < distance)
                {
                    smoothedPosition =
                        target.position +
                        direction * safeDistance;

                    positionVelocity = Vector3.zero;
                    IsObstructed = true;
                }
            }

            transform.position = smoothedPosition;
        }

        private void UpdateCameraRotation()
        {
            Vector3 lookDirection =
                target.position - transform.position;

            if (lookDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion desiredRotation =
                Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(
                    -rotationSpeed * Time.deltaTime
                )
            );
        }
    }
}