// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.UI
{
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public enum ScaleMode { ConstantPixel, ScaleWithScreenSize, ConstantPhysicalSize }

    public class Canvas
    {
        public string EntityId { get; set; }
        public RenderMode RenderMode { get; set; }
        public ScaleMode ScaleMode { get; set; }
        public float PixelsPerUnit { get; set; }
        public int SortingOrder { get; set; }
        public bool OverrideSorting { get; set; }
        public string SortingLayerName { get; set; }
        public int SortingLayerID { get; set; }
        public Camera WorldCamera { get; set; }
        public PlaneDistance PlaneDistance { get; set; }
        public List<UIElement> Elements { get; set; }
        public RectTransform RectTransform { get; set; }

        public Canvas(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            RenderMode = RenderMode.ScreenSpaceOverlay;
            ScaleMode = ScaleMode.ConstantPixel;
            PixelsPerUnit = 100f;
            SortingOrder = 0;
            OverrideSorting = false;
            SortingLayerName = "Default";
            SortingLayerID = 0;
            PlaneDistance = new PlaneDistance(100f, 100f);
            Elements = new List<UIElement>();
            RectTransform = new RectTransform();
        }

        public void AddElement(UIElement element)
        {
            if (element != null)
            {
                Elements.Add(element);
            }
        }

        public void RemoveElement(UIElement element)
        {
            Elements.Remove(element);
        }

        public void Update(float deltaTime)
        {
            foreach (var element in Elements)
            {
                element.Update(deltaTime);
            }
        }

        public void Render()
        {
            foreach (var element in Elements)
            {
                element.Render();
            }
        }
    }

    public struct PlaneDistance
    {
        public float X, Y;
        public PlaneDistance(float x, float y) { X = x; Y = y; }
    }

    public class RectTransform
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 AnchorMin { get; set; }
        public Vector2 AnchorMax { get; set; }
        public Vector2 Pivot { get; set; }
        public Vector2 Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public RectTransform()
        {
            Position = Vector2.Zero;
            Size = new Vector2(100f, 100f);
            AnchorMin = new Vector2(0.5f, 0.5f);
            AnchorMax = new Vector2(0.5f, 0.5f);
            Pivot = new Vector2(0.5f, 0.5f);
            Rotation = Vector2.Zero;
            Scale = Vector2.One;
        }

        public Vector2 GetWorldPosition()
        {
            return Position;
        }

        public Rect GetRect()
        {
            return new Rect(Position.X - Size.X * Pivot.X, Position.Y - Size.Y * Pivot.Y, Size.X, Size.Y);
        }
    }

    public struct Rect
    {
        public float X, Y, Width, Height;
        public Rect(float x, float y, float width, float height) { X = x; Y = y; Width = width; Height = height; }
        public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
    }

    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);
    }
}
