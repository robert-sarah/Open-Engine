// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Camera
{
    public enum CinemachineBlendStyle { Cut, EaseInOut, EaseIn, EaseOut, Custom }

    public class CinemachineVirtualCamera
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float FieldOfView { get; set; }
        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }
        public bool FollowTarget { get; set; }
        public string TargetEntityId { get; set; }
        public Vector3 FollowOffset { get; set; }
        public bool LookAtTarget { get; set; }
        public string LookAtEntityId { get; set; }
        public float Damping { get; set; }
        public float ShoulderOffset { get; set; }
        public CameraType Type { get; set; }

        public CinemachineVirtualCamera()
        {
            Priority = 10;
            FieldOfView = 60f;
            NearClipPlane = 0.3f;
            FarClipPlane = 1000f;
            FollowOffset = new Vector3(0, 2, -5);
            Damping = 0.5f;
            ShoulderOffset = 0.5f;
            Type = CameraType.ThirdPerson;
        }

        public void Update(float deltaTime, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (FollowTarget && !string.IsNullOrEmpty(TargetEntityId))
            {
                Vector3 desiredPosition = targetPosition + (targetRotation * FollowOffset);
                Position = Vector3.Lerp(Position, desiredPosition, Damping * deltaTime);
            }

            if (LookAtTarget && !string.IsNullOrEmpty(LookAtEntityId))
            {
                Vector3 lookDirection = Vector3.Normalize(targetPosition - Position);
                Rotation = Quaternion.LookRotation(lookDirection, Vector3.Up);
            }
        }
    }

    public class CinemachineBrain
    {
        private CinemachineVirtualCamera _activeCamera;
        private CinemachineVirtualCamera _nextCamera;
        private float _blendTime;
        private float _blendDuration;
        private CinemachineBlendStyle _blendStyle;
        private bool _isBlending;

        public CinemachineVirtualCamera ActiveCamera => _activeCamera;
        public bool IsBlending => _isBlending;

        public CinemachineBrain()
        {
            _blendStyle = CinemachineBlendStyle.EaseInOut;
            _blendDuration = 1f;
        }

        public void AddCamera(CinemachineVirtualCamera camera)
        {
            if (_activeCamera == null || camera.Priority > _activeCamera.Priority)
            {
                SwitchCamera(camera);
            }
        }

        public void SwitchCamera(CinemachineVirtualCamera camera, CinemachineBlendStyle blendStyle = CinemachineBlendStyle.EaseInOut, float duration = 1f)
        {
            if (_activeCamera == camera) return;

            _nextCamera = camera;
            _blendStyle = blendStyle;
            _blendDuration = duration;
            _blendTime = 0f;
            _isBlending = true;
        }

        public void Update(float deltaTime, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (_isBlending)
            {
                UpdateBlend(deltaTime);
            }

            _activeCamera?.Update(deltaTime, targetPosition, targetRotation);
        }

        private void UpdateBlend(float deltaTime)
        {
            _blendTime += deltaTime;

            if (_blendTime >= _blendDuration)
            {
                _activeCamera = _nextCamera;
                _isBlending = false;
                _nextCamera = null;
            }
            else
            {
                float t = _blendTime / _blendDuration;
                t = ApplyBlendStyle(t);

                // Blend camera properties
                if (_activeCamera != null && _nextCamera != null)
                {
                    _activeCamera.Position = Vector3.Lerp(_activeCamera.Position, _nextCamera.Position, t);
                    _activeCamera.Rotation = Quaternion.Slerp(_activeCamera.Rotation, _nextCamera.Rotation, t);
                    _activeCamera.FieldOfView = Math.Lerp(_activeCamera.FieldOfView, _nextCamera.FieldOfView, t);
                }
            }
        }

        private float ApplyBlendStyle(float t)
        {
            switch (_blendStyle)
            {
                case CinemachineBlendStyle.Cut:
                    return 1f;
                case CinemachineBlendStyle.EaseInOut:
                    return t * t * (3f - 2f * t);
                case CinemachineBlendStyle.EaseIn:
                    return t * t;
                case CinemachineBlendStyle.EaseOut:
                    return 1f - (1f - t) * (1f - t);
                default:
                    return t;
            }
        }
    }

    public enum CameraType { ThirdPerson, FirstPerson, Isometric, TopDown, SideScrolling }

    public class CameraShake
    {
        public float Amplitude { get; set; }
        public float Frequency { get; set; }
        public float Duration { get; set; }
        public float ElapsedTime { get; set; }
        public bool IsActive { get; set; }

        public CameraShake(float amplitude, float frequency, float duration)
        {
            Amplitude = amplitude;
            Frequency = frequency;
            Duration = duration;
            IsActive = true;
        }

        public Vector3 GetShakeOffset()
        {
            if (!IsActive) return Vector3.Zero;

            float t = ElapsedTime / Duration;
            if (t >= 1f)
            {
                IsActive = false;
                return Vector3.Zero;
            }

            float decay = 1f - t;
            float x = (float)System.Math.Sin(ElapsedTime * Frequency * 2 * System.Math.PI) * Amplitude * decay;
            float y = (float)System.Math.Cos(ElapsedTime * Frequency * 2 * System.Math.PI) * Amplitude * decay;

            return new Vector3(x, y, 0);
        }

        public void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }
    }
}
