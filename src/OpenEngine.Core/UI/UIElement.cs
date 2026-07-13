// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.UI
{
    public abstract class UIElement
    {
        public string Name { get; set; }
        public RectTransform RectTransform { get; set; }
        public bool Enabled { get; set; }
        public int SortingOrder { get; set; }
        public List<UIElement> Children { get; set; }
        public UIElement Parent { get; set; }

        public UIElement()
        {
            RectTransform = new RectTransform();
            Enabled = true;
            SortingOrder = 0;
            Children = new List<UIElement>();
        }

        public virtual void Update(float deltaTime)
        {
            if (!Enabled) return;
            foreach (var child in Children)
            {
                child.Update(deltaTime);
            }
        }

        public virtual void Render()
        {
            if (!Enabled) return;
            foreach (var child in Children)
            {
                child.Render();
            }
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                child.Parent = this;
                Children.Add(child);
            }
        }

        public void RemoveChild(UIElement child)
        {
            child.Parent = null;
            Children.Remove(child);
        }
    }

    public class Image : UIElement
    {
        public Sprite Sprite { get; set; }
        public Color Color { get; set; }
        public Material Material { get; set; }
        public bool RaycastTarget { get; set; }
        public FillMethod FillMethod { get; set; }
        public float FillAmount { get; set; }
        public bool FillClockwise { get; set; }
        public int FillOrigin { get; set; }
        public ImageType Type { get; set; }
        public bool PreserveAspect { get; set; }

        public Image()
        {
            Color = Color.White;
            RaycastTarget = true;
            FillAmount = 1f;
            FillClockwise = true;
            FillOrigin = 0;
            Type = ImageType.Simple;
            PreserveAspect = false;
        }

        public override void Render()
        {
            if (!Enabled || Sprite == null) return;
            // Render image with sprite and material
            base.Render();
        }
    }

    public class Text : UIElement
    {
        public string TextContent { get; set; }
        public Font Font { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public TextAlignment Alignment { get; set; }
        public HorizontalWrapMode HorizontalOverflow { get; set; }
        public VerticalWrapMode VerticalOverflow { get; set; }
        public bool RaycastTarget { get; set; }
        public float LineSpacing { get; set; }
        public float RichText { get; set; }
        public bool AlignByGeometry { get; set; }

        public Text()
        {
            TextContent = "";
            FontSize = 14;
            Color = Color.White;
            Alignment = TextAlignment.Left;
            HorizontalOverflow = HorizontalWrapMode.Wrap;
            VerticalOverflow = VerticalWrapMode.Truncate;
            RaycastTarget = false;
            LineSpacing = 1f;
            RichText = 1f;
            AlignByGeometry = false;
        }

        public override void Render()
        {
            if (!Enabled) return;
            // Render text with font
            base.Render();
        }
    }

    public class Button : UIElement
    {
        public Image TargetGraphic { get; set; }
        public Transition Transition { get; set; }
        public ColorBlock Colors { get; set; }
        public SpriteState SpriteState { get; set; }
        public AnimationTriggers AnimationTriggers { get; set; }
        public bool Interactable { get; set; }

        public event Action OnClick;
        public event Action OnPointerEnter;
        public event Action OnPointerExit;
        public event Action OnPointerDown;
        public event Action OnPointerUp;

        public Button()
        {
            Transition = Transition.ColorTint;
            Colors = new ColorBlock();
            SpriteState = new SpriteState();
            AnimationTriggers = new AnimationTriggers();
            Interactable = true;
        }

        public void Click()
        {
            if (Interactable)
            {
                OnClick?.Invoke();
            }
        }

        public void SetInteractable(bool value)
        {
            Interactable = value;
        }
    }

    public class InputField : UIElement
    {
        public Text TextComponent { get; set; }
        public Image Image { get; set; }
        public string Text { get; set; }
        public int CharacterLimit { get; set; }
        public ContentType ContentType { get; set; }
        public LineType LineType { get; set; }
        public Placeholder Placeholder { get; set; }
        public Caret Caret { get; set; }

        public event Action<string> OnValueChanged;
        public event Action<string> OnEndEdit;
        public event Action OnSelect;
        public event Action OnDeselect;

        public InputField()
        {
            CharacterLimit = 0;
            ContentType = ContentType.Standard;
            LineType = LineType.SingleLine;
            Placeholder = new Placeholder();
            Caret = new Caret();
        }

        public void SetText(string value)
        {
            Text = value;
            OnValueChanged?.Invoke(value);
        }
    }

    public class Scrollbar : UIElement
    {
        public Image Handle { get; set; }
        public RectTransform HandleRect { get; set; }
        public ScrollDirection Direction { get; set; }
        public float Value { get; set; }
        public float Size { get; set; }
        public int NumberOfSteps { get; set; }

        public event Action<float> OnValueChanged;

        public Scrollbar()
        {
            Direction = ScrollDirection.BottomToTop;
            Value = 1f;
            Size = 1f;
            NumberOfSteps = 0;
        }

        public void SetValue(float value)
        {
            Value = Math.Clamp(value, 0f, 1f);
            OnValueChanged?.Invoke(Value);
        }
    }

    public class ScrollView : UIElement
    {
        public RectTransform Content { get; set; }
        public Scrollbar HorizontalScrollbar { get; set; }
        public Scrollbar VerticalScrollbar { get; set; }
        public MovementType MovementType { get; set; }
        public Elasticity Elasticity { get; set; }
        public Inertia Inertia { get; set; }
        public bool DecelerationRate { get; set; }
        public ScrollbarVisibility HorizontalScrollbarVisibility { get; set; }
        public ScrollbarVisibility VerticalScrollbarVisibility { get; set; }
        public bool Horizontal { get; set; }
        public bool Vertical { get; set; }

        public ScrollView()
        {
            MovementType = MovementType.Elastic;
            Elasticity = 0.1f;
            Inertia = true;
            DecelerationRate = 0.135f;
            HorizontalScrollbarVisibility = ScrollbarVisibility.Auto;
            VerticalScrollbarVisibility = ScrollbarVisibility.Auto;
            Horizontal = true;
            Vertical = true;
        }
    }

    public class Toggle : UIElement
    {
        public Image TargetGraphic { get; set; }
        public ToggleGroup Group { get; set; }
        public bool IsOn { get; set; }
        public ToggleTransition Transition { get; set; }
        public Graphic TargetGraphic { get; set; }

        public event Action<bool> OnValueChanged;

        public Toggle()
        {
            IsOn = false;
            Transition = ToggleTransition.Fade;
        }

        public void SetIsOn(bool value)
        {
            if (IsOn != value)
            {
                IsOn = value;
                OnValueChanged?.Invoke(value);
            }
        }
    }

    public class Slider : UIElement
    {
        public RectTransform FillRect { get; set; }
        public RectTransform HandleRect { get; set; }
        public SliderDirection Direction { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float WholeNumbers { get; set; }
        public float Value { get; set; }

        public event Action<float> OnValueChanged;

        public Slider()
        {
            Direction = SliderDirection.LeftToRight;
            MinValue = 0f;
            MaxValue = 1f;
            WholeNumbers = 0f;
            Value = 0f;
        }

        public void SetValue(float value)
        {
            Value = WholeNumbers > 0 ? Math.Round(value) : value;
            Value = Math.Clamp(Value, MinValue, MaxValue);
            OnValueChanged?.Invoke(Value);
        }
    }

    public class Dropdown : UIElement
    {
        public Text CaptionText { get; set; }
        public Text ItemText { get; set; }
        public Image CaptionImage { get; set; }
        public Image ItemImage { get; set; }
        public List<Dropdown.OptionData> Options { get; set; }
        public int Value { get; set; }

        public event Action<int> OnValueChanged;

        public Dropdown()
        {
            Options = new List<OptionData>();
            Value = 0;
        }

        public void AddOptions(List<string> options)
        {
            foreach (var option in options)
            {
                Options.Add(new OptionData(option));
            }
        }

        public void ClearOptions()
        {
            Options.Clear();
        }

        public class OptionData
        {
            public string Text { get; set; }
            public Sprite Image { get; set; }

            public OptionData(string text, Sprite image = null)
            {
                Text = text;
                Image = image;
            }
        }
    }

    // Supporting types
    public enum ImageType { Simple, Sliced, Tiled, Filled }
    public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
    public enum TextAlignment { Left, Center, Right }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }
    public enum Transition { None, ColorTint, SpriteSwap, Animation }
    public enum ScrollDirection { LeftToRight, RightToLeft, BottomToTop, TopToBottom }
    public enum MovementType { Unrestricted, Elastic, Clamped }
    public enum ToggleTransition { None, Fade }
    public enum SliderDirection { LeftToRight, RightToLeft, BottomToTop, TopToBottom }
    public enum ScrollbarVisibility { Permanent, Auto, Hidden }
    public enum ContentType { Standard, IntegerNumber, DecimalNumber, Alphanumeric, Name, Email, Password, Pin, Custom }
    public enum LineType { SingleLine, MultiLineNewline, MultiLineSubmit }

    public struct ColorBlock
    {
        public Color NormalColor, HighlightedColor, PressedColor, SelectedColor, DisabledColor;
        public float ColorMultiplier, FadeDuration;

        public ColorBlock()
        {
            NormalColor = Color.White;
            HighlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            PressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            SelectedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            ColorMultiplier = 1f;
            FadeDuration = 0.1f;
        }
    }

    public struct SpriteState
    {
        public Sprite HighlightedSprite, PressedSprite, SelectedSprite, DisabledSprite;
    }

    public struct AnimationTriggers
    {
        public string NormalTrigger, HighlightedTrigger, PressedTrigger, SelectedTrigger, DisabledTrigger;
    }

    public class Placeholder
    {
        public string Text { get; set; }
        public Color Color { get; set; }
    }

    public class Caret
    {
        public Color Color { get; set; }
        public int SelectionColor { get; set; }
    }

    public class Sprite { }
    public class Font { }
    public class Graphic { }
    public class ToggleGroup { }
}
