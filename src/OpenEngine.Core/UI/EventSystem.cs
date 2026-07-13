// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.UI
{
    public class EventSystem
    {
        private List<UIElement> _uiElements;
        private UIElement _currentSelected;
        private UIElement _currentHovered;
        private PointerEventData _pointerData;

        public EventSystem()
        {
            _uiElements = new List<UIElement>();
            _pointerData = new PointerEventData();
        }

        public void RegisterElement(UIElement element)
        {
            if (element != null && !_uiElements.Contains(element))
            {
                _uiElements.Add(element);
            }
        }

        public void UnregisterElement(UIElement element)
        {
            _uiElements.Remove(element);
        }

        public void ProcessPointerEvent(Vector2 position, bool isDown, bool isUp)
        {
            _pointerData.Position = position;
            _pointerData.Pressed = isDown;
            _pointerData.Released = isUp;

            if (isDown)
            {
                HandlePointerDown();
            }
            else if (isUp)
            {
                HandlePointerUp();
            }

            HandlePointerMove();
        }

        private void HandlePointerDown()
        {
            var hovered = GetElementAtPosition(_pointerData.Position);
            
            if (hovered != null)
            {
                SetSelectedGameObject(hovered);
                
                if (hovered is Button button)
                {
                    button.OnPointerDown?.Invoke();
                }
            }
        }

        private void HandlePointerUp()
        {
            if (_currentSelected != null)
            {
                if (_currentSelected is Button button)
                {
                    button.OnPointerUp?.Invoke();
                    
                    if (GetElementAtPosition(_pointerData.Position) == _currentSelected)
                    {
                        button.Click();
                    }
                }
            }
        }

        private void HandlePointerMove()
        {
            var hovered = GetElementAtPosition(_pointerData.Position);
            
            if (hovered != _currentHovered)
            {
                if (_currentHovered != null && _currentHovered is Button prevButton)
                {
                    prevButton.OnPointerExit?.Invoke();
                }
                
                if (hovered != null && hovered is Button newButton)
                {
                    newButton.OnPointerEnter?.Invoke();
                }
                
                _currentHovered = hovered;
            }
        }

        private UIElement GetElementAtPosition(Vector2 position)
        {
            foreach (var element in _uiElements)
            {
                if (element.Enabled && element.RectTransform.GetRect().Contains(position))
                {
                    return element;
                }
            }
            return null;
        }

        public void SetSelectedGameObject(UIElement element)
        {
            _currentSelected = element;
        }

        public UIElement GetSelectedGameObject()
        {
            return _currentSelected;
        }

        public void Update()
        {
            // Process continuous events
        }
    }

    public class PointerEventData
    {
        public Vector2 Position { get; set; }
        public Vector2 Delta { get; set; }
        public bool Pressed { get; set; }
        public bool Released { get; set; }
        public int ClickCount { get; set; }
        public int PointerId { get; set; }

        public PointerEventData()
        {
            Position = Vector2.Zero;
            Delta = Vector2.Zero;
            Pressed = false;
            Released = false;
            ClickCount = 0;
            PointerId = -1;
        }
    }

    public interface IEventSystemHandler
    {
        void OnPointerEnter(PointerEventData eventData);
        void OnPointerExit(PointerEventData eventData);
        void OnPointerDown(PointerEventData eventData);
        void OnPointerUp(PointerEventData eventData);
        void OnPointerClick(PointerEventData eventData);
    }
}
