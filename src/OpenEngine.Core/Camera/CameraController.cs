// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Camera
{
    public enum CameraMode { FreeLook, Follow, Orbit, Cinematic }
    public enum CameraTargetMode { None, Target, Smooth }

    public class CameraController
    {
        public CameraMode Mode { get; set; }
        public CameraTargetMode TargetMode { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float FieldOfView { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public float MoveSpeed { get; set; }
        public float LookSpeed { get; set; }
        public float ZoomSpeed { get; set; }
        public Vector3 TargetPosition { get; set; }
        public float SmoothTime { get; set; }
        public bool LockCursor { get; set; }

        private Vector3 _velocity;
        private Quaternion _targetRotation;

        public CameraController()
        {
            Mode = CameraMode.FreeLook;
            TargetMode = CameraTargetMode.None;
            Position = new Vector3(0f, 5f, -10f);
            Rotation = Quaternion.LookRotation(Vector3.Forward, Vector3.Up);
            FieldOfView = 60f;
            NearClip = 0.3f;
            FarClip = 1000f;
            MoveSpeed = 10f;
            LookSpeed = 2f;
            ZoomSpeed = 5f;
            TargetPosition = Vector3.Zero;
            SmoothTime = 0.1f;
            LockCursor = false;
            _velocity = Vector3.Zero;
            _targetRotation = Rotation;
        }

        public void Update(float deltaTime)
        {
            switch (Mode)
            {
                case CameraMode.FreeLook:
                    UpdateFreeLook(deltaTime);
                    break;
                case CameraMode.Follow:
                    UpdateFollow(deltaTime);
                    break;
                case CameraMode.Orbit:
                    UpdateOrbit(deltaTime);
                    break;
                case CameraMode.Cinematic:
                    UpdateCinematic(deltaTime);
                    break;
            }

            ApplySmoothDamping(deltaTime);
        }

        private void UpdateFreeLook(float deltaTime)
        {
            // Free look camera movement
        }

        private void UpdateFollow(float deltaTime)
        {
            if (TargetMode == CameraTargetMode.Target)
            {
                var targetOffset = Rotation * new Vector3(0f, 2f, -5f);
                var targetPos = TargetPosition + targetOffset;
                
                if (TargetMode == CameraTargetMode.Smooth)
                {
                    Position = Vector3.SmoothDamp(Position, targetPos, ref _velocity, SmoothTime);
                }
                else
                {
                    Position = targetPos;
                }

                Rotation = Quaternion.LookRotation((TargetPosition - Position).Normalized(), Vector3.Up);
            }
        }

        private void UpdateOrbit(float deltaTime)
        {
            if (TargetMode == CameraTargetMode.Target)
            {
                var direction = (Position - TargetPosition).Normalized();
                var distance = Vector3.Distance(Position, TargetPosition);
                Position = TargetPosition + direction * distance;
            }
        }

        private void UpdateCinematic(float deltaTime)
        {
            // Cinematic camera movement
        }

        private void ApplySmoothDamping(float deltaTime)
        {
            if (TargetMode == CameraTargetMode.Smooth)
            {
                Rotation = Quaternion.Slerp(Rotation, _targetRotation, deltaTime / SmoothTime);
            }
        }

        public void SetTarget(Vector3 target)
        {
            TargetPosition = target;
            TargetMode = CameraTargetMode.Target;
        }

        public void ClearTarget()
        {
            TargetMode = CameraTargetMode.None;
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            var moveDir = Rotation * direction;
            Position += moveDir * MoveSpeed * deltaTime;
        }

        public void Rotate(Vector2 rotation, float deltaTime)
        {
            var pitch = Quaternion.AngleAxis(rotation.Y * LookSpeed * deltaTime, Vector3.Right);
            var yaw = Quaternion.AngleAxis(rotation.X * LookSpeed * deltaTime, Vector3.Up);
            Rotation = yaw * pitch * Rotation;
        }

        public void Zoom(float amount, float deltaTime)
        {
            var zoomDir = Rotation * Vector3.Forward;
            Position += zoomDir * amount * ZoomSpeed * deltaTime;
        }

        public void SetMode(CameraMode mode)
        {
            Mode = mode;
        }

        public void Reset()
        {
            Position = new Vector3(0f, 5f, -10f);
            Rotation = Quaternion.LookRotation(Vector3.Forward, Vector3.Up);
            _velocity = Vector3.Zero;
        }
    }

    public static class Vector3Extensions
    {
        public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime)
        {
            return current + (target - current) * (1f - (float)Math.Exp(-1f / smoothTime));
        }
    }
}
