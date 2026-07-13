// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public enum CameraType { Perspective, Orthographic }
    public enum ClearFlags { Skybox, SolidColor, DepthOnly }

    public class Camera
    {
        public string EntityId { get; set; }
        public CameraType Type { get; set; }
        public float FieldOfView { get; set; }
        public float OrthographicSize { get; set; }
        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }
        public Rect ViewportRect { get; set; }
        public ClearFlags ClearFlags { get; set; }
        public Color BackgroundColor { get; set; }
        public int Depth { get; set; }
        public float CullingMask { get; set; }
        public bool OcclusionCulling { get; set; }
        public bool HDR { get; set; }
        public int MSAA { get; set; }
        public int TargetDisplay { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Camera(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Type = CameraType.Perspective;
            FieldOfView = 60f;
            OrthographicSize = 5f;
            NearClipPlane = 0.3f;
            FarClipPlane = 1000f;
            ViewportRect = new Rect(0, 0, 1, 1);
            ClearFlags = ClearFlags.Skybox;
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            Depth = 0;
            CullingMask = -1;
            OcclusionCulling = true;
            HDR = false;
            MSAA = 1;
            TargetDisplay = 0;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Type == CameraType.Perspective 
                ? Matrix4x4.Perspective(FieldOfView * Mathf.Deg2Rad, GetAspectRatio(), NearClipPlane, FarClipPlane)
                : Matrix4x4.Orthographic(-OrthographicSize * GetAspectRatio(), OrthographicSize * GetAspectRatio(), 
                                          -OrthographicSize, OrthographicSize, NearClipPlane, FarClipPlane);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.LookAt(Position, Position + Rotation * Vector3.Forward, Rotation * Vector3.Up);
        }

        public float GetAspectRatio()
        {
            return ViewportRect.Width / ViewportRect.Height;
        }

        public Ray ScreenPointToRay(Vector3 point)
        {
            var viewMatrix = GetViewMatrix();
            var projMatrix = GetProjectionMatrix();
            var viewProj = projMatrix * viewMatrix;
            var invViewProj = Matrix4x4.Inverse(viewProj);

            var ndc = new Vector3(
                (point.X - ViewportRect.X) / ViewportRect.Width * 2f - 1f,
                (point.Y - ViewportRect.Y) / ViewportRect.Height * 2f - 1f,
                1f
            );

            var worldPos = invViewProj.MultiplyPoint(ndc);
            return new Ray(Position, (worldPos - Position).Normalized());
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            var viewMatrix = GetViewMatrix();
            var projMatrix = GetProjectionMatrix();
            var viewProj = projMatrix * viewMatrix;

            var clipPos = viewProj.MultiplyPoint(position);
            var ndc = new Vector3(clipPos.X, clipPos.Y, clipPos.Z);

            return new Vector3(
                (ndc.X + 1f) * 0.5f * ViewportRect.Width + ViewportRect.X,
                (ndc.Y + 1f) * 0.5f * ViewportRect.Height + ViewportRect.Y,
                ndc.Z
            );
        }
    }

    public struct Rect
    {
        public float X, Y, Width, Height;
        public Rect(float x, float y, float width, float height) { X = x; Y = y; Width = width; Height = height; }
    }

    public static class Mathf
    {
        public const float Deg2Rad = 0.0174532924f;
        public const float Rad2Deg = 57.29578f;
    }
}
