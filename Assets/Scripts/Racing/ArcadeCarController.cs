using UnityEngine;
using UnityEngine.InputSystem;

namespace BoardRacing
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [SerializeField] private int playerIndex;

        private const float Acceleration = 32f;
        private const float ReverseAcceleration = 18f;
        private const float BrakeForce = 36f;
        private const float TurnRate = 210f;
        private const float MaxForwardSpeed = 16f;
        private const float MaxReverseSpeed = 7f;
        private const float BoostAcceleration = 18f;
        private const float BoostMaxSpeed = 21f;
        private const float LateralGrip = 0.78f;

        private Rigidbody2D body;
        private Vector2 spawnPosition;
        private float spawnRotation;
        private bool inputLocked;
        private float steer;
        private float throttle;
        private float brake;
        private bool boostHeld;

        public int PlayerIndex => playerIndex;

        public void Configure(int assignedPlayerIndex)
        {
            playerIndex = assignedPlayerIndex;
        }

        public void ResetCar(Vector2 position, float rotation)
        {
            body ??= GetComponent<Rigidbody2D>();
            spawnPosition = position;
            spawnRotation = rotation;

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = position;
            body.rotation = rotation;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
        }

        public void SetInputLocked(bool locked)
        {
            inputLocked = locked;
            steer = 0f;
            throttle = 0f;
            brake = 0f;
            boostHeld = false;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 0.9f;
            body.angularDamping = 6f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void Update()
        {
            if (inputLocked)
            {
                return;
            }

            ReadGamepadInput();
            ReadKeyboardFallbackInput();
        }

        private void FixedUpdate()
        {
            if (inputLocked)
            {
                DampToStop();
                return;
            }

            ApplyDriftGrip();
            ApplyDriveForces();
            ApplySteering();
            ClampSpeed();
        }

        private void ReadGamepadInput()
        {
            if (playerIndex >= Gamepad.all.Count)
            {
                steer = 0f;
                throttle = 0f;
                brake = 0f;
                boostHeld = false;
                return;
            }

            var gamepad = Gamepad.all[playerIndex];
            steer = ApplyDeadZone(gamepad.leftStick.x.ReadValue());
            throttle = gamepad.rightTrigger.ReadValue();
            brake = gamepad.leftTrigger.ReadValue();
            boostHeld = gamepad.buttonSouth.isPressed;
        }

        private void ReadKeyboardFallbackInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (playerIndex == 0)
            {
                steer = Mathf.Clamp(steer + ReadAxis(keyboard.aKey.isPressed, keyboard.dKey.isPressed), -1f, 1f);
                throttle = Mathf.Max(throttle, keyboard.wKey.isPressed ? 1f : 0f);
                brake = Mathf.Max(brake, keyboard.sKey.isPressed ? 1f : 0f);
                boostHeld |= keyboard.leftShiftKey.isPressed;
                return;
            }

            steer = Mathf.Clamp(steer + ReadAxis(keyboard.leftArrowKey.isPressed, keyboard.rightArrowKey.isPressed), -1f, 1f);
            throttle = Mathf.Max(throttle, keyboard.upArrowKey.isPressed ? 1f : 0f);
            brake = Mathf.Max(brake, keyboard.downArrowKey.isPressed ? 1f : 0f);
            boostHeld |= keyboard.rightShiftKey.isPressed;
        }

        private void ApplyDriveForces()
        {
            var forward = (Vector2)transform.up;
            var forwardSpeed = Vector2.Dot(body.linearVelocity, forward);

            if (throttle > 0.01f)
            {
                var boost = boostHeld ? BoostAcceleration : 0f;
                body.AddForce(forward * ((Acceleration + boost) * throttle), ForceMode2D.Force);
            }

            if (brake <= 0.01f)
            {
                return;
            }

            if (forwardSpeed > 0.6f)
            {
                body.AddForce(-forward * (BrakeForce * brake), ForceMode2D.Force);
            }
            else
            {
                body.AddForce(-forward * (ReverseAcceleration * brake), ForceMode2D.Force);
            }
        }

        private void ApplySteering()
        {
            var speed = body.linearVelocity.magnitude;
            var steeringGrip = Mathf.InverseLerp(0.8f, 7.5f, speed);
            var direction = Vector2.Dot(body.linearVelocity, transform.up) >= -0.2f ? -1f : 1f;
            body.MoveRotation(body.rotation + direction * steer * TurnRate * steeringGrip * Time.fixedDeltaTime);
        }

        private void ApplyDriftGrip()
        {
            var forward = (Vector2)transform.up;
            var right = (Vector2)transform.right;
            var forwardVelocity = forward * Vector2.Dot(body.linearVelocity, forward);
            var sidewaysVelocity = right * Vector2.Dot(body.linearVelocity, right);
            body.linearVelocity = forwardVelocity + sidewaysVelocity * LateralGrip;
        }

        private void ClampSpeed()
        {
            var forward = (Vector2)transform.up;
            var forwardSpeed = Vector2.Dot(body.linearVelocity, forward);
            var maxForward = boostHeld && throttle > 0.1f ? BoostMaxSpeed : MaxForwardSpeed;

            if (forwardSpeed > maxForward)
            {
                body.linearVelocity = forward * maxForward + Vector2.Perpendicular(forward) * Vector2.Dot(body.linearVelocity, Vector2.Perpendicular(forward));
            }
            else if (forwardSpeed < -MaxReverseSpeed)
            {
                body.linearVelocity = -forward * MaxReverseSpeed + Vector2.Perpendicular(forward) * Vector2.Dot(body.linearVelocity, Vector2.Perpendicular(forward));
            }
        }

        private void DampToStop()
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, 0.08f);
            body.angularVelocity = Mathf.Lerp(body.angularVelocity, 0f, 0.08f);
        }

        private static float ReadAxis(bool negative, bool positive)
        {
            return (positive ? 1f : 0f) - (negative ? 1f : 0f);
        }

        private static float ApplyDeadZone(float value)
        {
            return Mathf.Abs(value) < 0.12f ? 0f : value;
        }
    }
}
