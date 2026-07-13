// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Input
{
    public enum KeyCode
    {
        None, Space, Enter, Escape, Tab, Backspace,
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9,
        F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt,
        UpArrow, DownArrow, LeftArrow, RightArrow
    }

    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);
    }

    public class InputSystem
    {
        private Dictionary<KeyCode, bool> _currentKeys;
        private Dictionary<KeyCode, bool> _previousKeys;
        private Vector2 _mousePosition;
        private Vector2 _mouseDelta;
        private Dictionary<int, bool> _mouseButtons;
        private float _scrollDelta;

        public Vector2 MousePosition => _mousePosition;
        public Vector2 MouseDelta => _mouseDelta;

        public InputSystem()
        {
            _currentKeys = new Dictionary<KeyCode, bool>();
            _previousKeys = new Dictionary<KeyCode, bool>();
            _mouseButtons = new Dictionary<int, bool>();
            _mousePosition = Vector2.Zero;
            _mouseDelta = Vector2.Zero;
            _scrollDelta = 0f;
        }

        public void Update()
        {
            _previousKeys = new Dictionary<KeyCode, bool>(_currentKeys);
            _mouseDelta = Vector2.Zero;
            _scrollDelta = 0f;
        }

        public void SetKey(KeyCode key, bool pressed)
        {
            _currentKeys[key] = pressed;
        }

        public void SetMouseButton(int button, bool pressed)
        {
            _mouseButtons[button] = pressed;
        }

        public void SetMousePosition(Vector2 position)
        {
            _mouseDelta = new Vector2(position.X - _mousePosition.X, position.Y - _mousePosition.Y);
            _mousePosition = position;
        }

        public void SetScrollDelta(float delta)
        {
            _scrollDelta = delta;
        }

        public bool GetKey(KeyCode key)
        {
            return _currentKeys.ContainsKey(key) && _currentKeys[key];
        }

        public bool GetKeyDown(KeyCode key)
        {
            return GetKey(key) && (!_previousKeys.ContainsKey(key) || !_previousKeys[key]);
        }

        public bool GetKeyUp(KeyCode key)
        {
            return !GetKey(key) && _previousKeys.ContainsKey(key) && _previousKeys[key];
        }

        public bool GetMouseButton(int button)
        {
            return _mouseButtons.ContainsKey(button) && _mouseButtons[button];
        }

        public bool GetMouseButtonDown(int button)
        {
            return GetMouseButton(button);
        }

        public bool GetMouseButtonUp(int button)
        {
            return !GetMouseButton(button);
        }

        public float GetAxis(string axisName)
        {
            return axisName.ToLower() switch
            {
                "horizontal" => (_currentKeys.ContainsKey(KeyCode.D) && _currentKeys[KeyCode.D] ? 1f : 0f) - 
                                 (_currentKeys.ContainsKey(KeyCode.A) && _currentKeys[KeyCode.A] ? 1f : 0f),
                "vertical" => (_currentKeys.ContainsKey(KeyCode.W) && _currentKeys[KeyCode.W] ? 1f : 0f) - 
                               (_currentKeys.ContainsKey(KeyCode.S) && _currentKeys[KeyCode.S] ? 1f : 0f),
                "mouse x" => _mouseDelta.X,
                "mouse y" => _mouseDelta.Y,
                _ => 0f
            };
        }

        public float GetAxisRaw(string axisName)
        {
            return GetAxis(axisName);
        }
    }
}
