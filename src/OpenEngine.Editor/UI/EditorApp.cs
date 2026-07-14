// Created By Levi Enama
using System;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using ImGuiNET;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;
using OpenEngine.Core.Persistence;
using OpenEngine.Editor.Graphics;

namespace OpenEngine.Editor.UI;

public class EditorApp : IDisposable
{
    private readonly IWindow _window;
    private GL _gl;
    private IInputContext _input;
    private Graphics.SilkOpenGLRenderer _renderer;
    private Engine.OpenEngineCore _engine;
    private string _selectedEntityId = string.Empty;
    private string _godAlertText = string.Empty;
    private bool _showDemoWindow = false;
    private ImGuiController _imguiController;
    private float _cameraSpeed = 10f;
    private bool _isMouseCaptured = false;
    private Vector2 _lastMousePos;
    private Vector3 _cameraEuler = new(0, 0, 0);

    public EditorApp()
    {
        var options = WindowOptions.Default;
        options.Size = new(1600, 900);
        options.Title = "Open Engine - 3D Universal Simulation";
        options.VSync = true;
        _window = Window.Create(options);
        
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
    }

    public void Run() => _window.Run();

    private void OnLoad()
    {
        _gl = GL.GetApi(_window);
        _input = _window.CreateInput();
        
        _engine = Engine.OpenEngineCore.Instance;
        _engine.Initialize();
        InitializeDemoScene();
        _engine.Start();
        
        _renderer = new Graphics.SilkOpenGLRenderer(_window);
        _renderer.Initialize();
        _imguiController = new ImGuiController(_window, _gl);
        
        _input.Mice[0].Scroll += (_, e) => _renderer.MainCamera.Position += _renderer.MainCamera.Target * e.Y * 2;
        _input.Mice[0].MouseDown += (_, btn) =>
        {
            if (btn == MouseButton.Right)
            {
                _isMouseCaptured = true;
                _input.Mice[0].Cursor.CursorMode = CursorMode.Raw;
                _lastMousePos = _input.Mice[0].Position;
            }
        };
        _input.Mice[0].MouseUp += (_, btn) =>
        {
            if (btn == MouseButton.Right)
            {
                _isMouseCaptured = false;
                _input.Mice[0].Cursor.CursorMode = CursorMode.Normal;
            }
        };
    }

    private void InitializeDemoScene()
    {
        var station = _engine.CreateEntity("Space Station Alpha", "SpaceStation");
        station.Position3D = new(0, 0, 0);
        station.Scale3D = new(3, 2, 3);
        station.Attributes["Energy"] = 1000;
        station.Attributes["Population"] = 500;
        _engine.Physics.AddBody(station, Core.Physics.PhysicsBodyType.Static);

        var ship = _engine.CreateEntity("Explorer-1", "Starship");
        ship.Position3D = new(-10, 0, 0);
        ship.Scale3D = new(1, 0.6f, 2);
        ship.Attributes["Hull"] = 100;
        ship.Attributes["Fuel"] = 200;
        ship.Capabilities.AddRange(new[] { "Move", "Trade", "Scan" });
        _engine.Physics.AddBody(ship, Core.Physics.PhysicsBodyType.Dynamic);

        var asteroid = _engine.CreateEntity("Asteroid A-12", "Asteroid");
        asteroid.Position3D = new(15, 5, -10);
        asteroid.Scale3D = new(1.5f, 1, 1.2f);
        asteroid.Attributes["Minerals"] = 85;
        _engine.Physics.AddBody(asteroid, Core.Physics.PhysicsBodyType.Kinematic);

        var enemy = _engine.CreateEntity("Unknown Warship", "Warship");
        enemy.Position3D = new(20, 0, -20);
        enemy.Scale3D = new(1.2f, 0.8f, 2.5f);
        enemy.Attributes["Armor"] = 150;
        enemy.Attributes["Aggro"] = 0.7f;
        _engine.Physics.AddBody(enemy, Core.Physics.PhysicsBodyType.Dynamic);

        _engine.CreateConnection(ship.Id, station.Id, ConnectionPredicate.DOCKED_AT);
        _engine.CreateConnection(enemy.Id, ship.Id, ConnectionPredicate.HOSTILE_TO);
    }

    private void OnUpdate(double deltaTime)
    {
        float dt = (float)deltaTime;
        _engine.Update();
        
        HandleCameraInput(dt);
        _imguiController.Update(dt);
    }

    private void HandleCameraInput(float dt)
    {
        var kb = _input.Keyboards[0];
        float speed = _cameraSpeed * dt;

        Vector3 forward = new Vector3(
            (float)Math.Sin(_cameraEuler.Y) * (float)Math.Cos(_cameraEuler.X),
            (float)Math.Sin(_cameraEuler.X),
            (float)Math.Cos(_cameraEuler.Y) * (float)Math.Cos(_cameraEuler.X)
        ).Normalized;
        Vector3 right = Vector3.Cross(forward, new Vector3(0, 1, 0)).Normalized;
        Vector3 up = new Vector3(0, 1, 0);

        if (kb.IsKeyPressed(Key.W)) _engine.Camera?.Position += forward * speed;
        if (kb.IsKeyPressed(Key.S)) _engine.Camera?.Position -= forward * speed;
        if (kb.IsKeyPressed(Key.A)) _engine.Camera?.Position -= right * speed;
        if (kb.IsKeyPressed(Key.D)) _engine.Camera?.Position += right * speed;
        if (kb.IsKeyPressed(Key.Q)) _engine.Camera?.Position -= up * speed;
        if (kb.IsKeyPressed(Key.E)) _engine.Camera?.Position += up * speed;

        if (_isMouseCaptured)
        {
            Vector2 mousePos = _input.Mice[0].Position;
            Vector2 delta = mousePos - _lastMousePos;
            _cameraEuler.X -= delta.Y * 0.002f;
            _cameraEuler.Y -= delta.X * 0.002f;
            _cameraEuler.X = Math.Clamp(_cameraEuler.X, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);
            _lastMousePos = mousePos;
        }
    }

    private void OnRender(double deltaTime)
    {
        _renderer?.Clear();
        DrawEditorUI();
        _imguiController.Render();
    }

    private void DrawEditorUI()
    {
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Scene")) { }
                if (ImGui.MenuItem("Save Scene")) _engine.SaveSystem?.SaveGame("save", "Save");
                if (ImGui.MenuItem("Load Scene")) _engine.SaveSystem?.LoadGame("save");
                ImGui.Separator();
                if (ImGui.MenuItem("Exit")) _window.Close();
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.MenuItem("Undo");
                ImGui.MenuItem("Redo");
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Show Demo Window", "", ref _showDemoWindow);
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }

        if (ImGui.Begin("Hierarchy"))
        {
            foreach (var entity in _engine.Entities.Values)
            {
                bool selected = _selectedEntityId == entity.Id;
                if (ImGui.Selectable($"{entity.Name} ({entity.Type})##{entity.Id}", selected))
                {
                    _selectedEntityId = entity.Id;
                }
            }
        }
        ImGui.End();

        if (ImGui.Begin("Inspector"))
        {
            var selected = _selectedEntityId != null ? _engine.GetEntity(_selectedEntityId) : null;
            if (selected != null)
            {
                ImGui.Text($"ID: {selected.Id}");
                ImGui.Text($"Type: {selected.Type}");
                ImGui.Separator();

                ImGui.Text("Transform");
                Vector3 pos = selected.Position3D;
                if (ImGui.DragFloat3("Position", ref pos, 0.5f)) selected.Position3D = pos;

                Vector3 rot = selected.Rotation;
                if (ImGui.DragFloat3("Rotation", ref rot, 0.1f)) selected.Rotation = rot;

                Vector3 scale = selected.Scale;
                if (ImGui.DragFloat3("Scale", ref scale, 0.1f)) selected.Scale = scale;

                ImGui.Separator();
                ImGui.Text("Attributes");
                foreach (var attr in selected.Attributes)
                {
                    float val = attr.Value;
                    if (ImGui.DragFloat(attr.Key, ref val, 1f))
                        selected.Attributes[attr.Key] = val;
                }
            }
        }
        ImGui.End();

        if (ImGui.Begin("God Panel Console"))
        {
            ImGui.InputText("Global Alert", ref _godAlertText, 256);
            if (ImGui.Button("Send Alert") && !string.IsNullOrEmpty(_godAlertText))
            {
                _engine.AddGodPanelAlert(_godAlertText);
                _godAlertText = "";
            }
            ImGui.Separator();
            ImGui.Text("Active Alerts:");
            foreach (var alert in _engine.GodPanelAlerts)
            {
                ImGui.TextWrapped($"⚠️ {alert}");
            }
        }
        ImGui.End();

        if (ImGui.Begin("Simulation Status"))
        {
            ImGui.Te
xt($"Tick: {_engine.TickCount}");
            ImGui.Text($"Entities: {_engine.Entities.Count}");
            ImGui.Text($"Connections: {_engine.Connections.Count}");
            ImGui.Separator();
            ImGui.Text("Created By Levi Enama");
        }
        ImGui.End();

        if (_showDemoWindow)
            ImGui.ShowDemoWindow();
    }

    private void OnClose()
    {
        // Unsubscribe from events
        EngineEventBus.Instance.Unsubscribe(EngineEventType.Log, OnEngineLog);
        EngineEventBus.Instance.Unsubscribe(EngineEventType.Error, OnEngineError);
        EngineEventBus.Instance.Unsubscribe(EngineEventType.Warning, OnEngineWarning);
        EngineEventBus.Instance.Unsubscribe(EngineEventType.EntityCreated, OnEntityCreated);
        EngineEventBus.Instance.Unsubscribe(EngineEventType.EntityRemoved, OnEntityRemoved);
        
        _imguiController.Dispose();
        _renderer?.Dispose();
        _input.Dispose();
        _engine.Shutdown();
        _engine.Dispose();
    }

    private void OnEngineLog(object sender, EngineEventArgs e)
    {
        Console.WriteLine($"[Engine Log] {e.Message}");
    }

    private void OnEngineError(object sender, EngineEventArgs e)
    {
        Console.WriteLine($"[Engine Error] {e.Message}");
    }

    private void OnEngineWarning(object sender, EngineEventArgs e)
    {
        Console.WriteLine($"[Engine Warning] {e.Message}");
    }

    private void OnEntityCreated(object sender, EngineEventArgs e)
    {
        Console.WriteLine($"[Entity Created] {e.EntityId}");
    }

    private void OnEntityRemoved(object sender, EngineEventArgs e)
    {
        Console.WriteLine($"[Entity Removed] {e.EntityId}");
    }

    public void Dispose() => OnClose();
}

public class ImGuiController : IDisposable
{
    private readonly IWindow _window;
    private readonly GL _gl;
    private readonly IntPtr _context;

    private uint _fontTexture;
    private uint _shader;
    private int _shaderTextureLocation;
    private int _shaderProjectionLocation;
    private uint _vbo;
    private uint _vao;
    private uint _elementsBuffer;

    public ImGuiController(IWindow window, GL gl)
    {
        _window = window;
        _gl = gl;
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;
        io.DisplaySize = new System.Numerics.Vector2(window.Size.X, window.Size.Y);
        io.DisplayFramebufferScale = System.Numerics.Vector2.One;

        ImGui.StyleColorsDark();

        CreateFontTexture();
        CreateDeviceObjects();

        window.FramebufferResize += OnFramebufferResize;
        window.MouseMove += OnMouseMove;
        window.MouseDown += OnMouseDown;
        window.MouseUp += OnMouseUp;
        window.MouseScroll += OnMouseScroll;
        window.KeyDown += OnKeyDown;
        window.KeyUp += OnKeyUp;
        window.TypeChar += OnTextInput;
    }

    private unsafe void CreateFontTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

        _fontTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _fontTexture);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();
    }

    private void CreateDeviceObjects()
    {
        const string vertexShaderSource = @"
            #version 330
            uniform mat4 Projection;
            layout(location=0) in vec2 Position;
            layout(location=1) in vec2 UV;
            layout(location=2) in vec4 Color;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = Projection * vec4(Position.xy, 0.0, 1.0);
            }";

        const string fragmentShaderSource = @"
            #version 330
            uniform sampler2D Texture;
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, vertexShader);
        _gl.AttachShader(_shader, fragmentShader);
        _gl.LinkProgram(_shader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        _shaderTextureLocation = _gl.GetUniformLocation(_shader, "Texture");
        _shaderProjectionLocation = _gl.GetUniformLocation(_shader, "Projection");

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _elementsBuffer = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _elementsBuffer);
        _gl.EnableVertexAttribArray(0);
        _gl.EnableVertexAttribArray(1);
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 20, (void*)0);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 20, (void*)8);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, 20, (void*)16);
        _gl.BindVertexArray(0);
    }

    public void Update(double deltaTime)
    {
        ImGui.SetCurrentContext(_context);
        var io = ImGui.GetIO();
        io.DeltaTime = (float)deltaTime;
        io.DisplaySize = new System.Numerics.Vector2(_window.Size.X, _window.Size.Y);
        ImGui.NewFrame();
    }

    public unsafe void Render()
    {
        ImGui.SetCurrentContext(_context);
        ImGui.Render();
        var drawData = ImGui.GetDrawData();
        if (drawData == null) return;

        var io = ImGui.GetIO();
        var frameBufferSize = io.DisplaySize;
        if (frameBufferSize.X <= 0 || frameBufferSize.Y <= 0) return;

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)frameBufferSize.X, (uint)frameBufferSize.Y);
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, frameBufferSize.X, frameBufferSize.Y, 0, -1, 1);

        _gl.UseProgram(_shader);
        _gl.UniformMatrix4(_shaderProjectionLocation, 1, false, (float*)&projectionMatrix);
        _gl.Uniform1(_shaderTextureLocation, 0);
        _gl.BindVertexArray(_vao);

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];
            var vertexSize = cmdList.VtxBuffer.Size * 20;
            var indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)vertexSize, (void*)cmdList.VtxBuffer.Data, BufferUsageARB.StreamDraw);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _elementsBuffer);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)indexSize, (void*)cmdList.IdxBuffer.Data, BufferUsageARB.StreamDraw);

            int indexOffset = 0;
            for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
            {
                var cmd = cmdList.CmdBuffer[i];
                if (cmd.UserCallback != IntPtr.Zero) continue;

                _gl.BindTexture(TextureTarget.Texture2D, (uint)cmd.TextureId);
                var clipRect = new Vector4(cmd.ClipRect.X, cmd.ClipRect.Y, cmd.ClipRect.Z, cmd.ClipRect.W);
                _gl.Scissor((int)clipRect.X, (int)(frameBufferSize.Y - clipRect.W), (uint)(clipRect.Z - clipRect.X), (uint)(clipRect.W - clipRect.Y));
                _gl.DrawElements(PrimitiveType.Triangles, cmd.ElemCount, DrawElementsType.UnsignedShort, (void*)(indexOffset * sizeof(ushort)));

                indexOffset += (int)cmd.ElemCount;
            }
        }

        _gl.Disable(EnableCap.ScissorTest);
    }

    private void OnFramebufferResize(IWindow window, Vector2D<int> size)
    {
        ImGui.GetIO().DisplaySize = new System.Numerics.Vector2(size.X, size.Y);
    }

    private void OnMouseMove(IMouse mouse, Vector2 pos)
    {
        ImGui.GetIO().MousePos = pos;
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        ImGui.GetIO().MouseDown[(int)button] = true;
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        ImGui.GetIO().MouseDown[(int)button] = false;
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        ImGui.GetIO().MouseWheel = scroll.Y;
        ImGui.GetIO().MouseWheelH = scroll.X;
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scanCode)
    {
        ImGui.GetIO().KeysDown[(int)key] = true;
        UpdateModifiers(keyboard);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scanCode)
    {
        ImGui.GetIO().KeysDown[(int)key] = false;
        UpdateModifiers(keyboard);
    }

    private void UpdateModifiers(IKeyboard keyboard)
    {
        var io = ImGui.GetIO();
        io.KeyCtrl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);
        io.KeyShift = keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight);
        io.KeyAlt = keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight);
        io.KeySuper = keyboard.IsKeyPressed(Key.SuperLeft) || keyboard.IsKeyPressed(Key.SuperRight);
    }

    private void OnTextInput(IKeyboard keyboard, char c)
    {
        ImGui.GetIO().AddInputCharacter(c);
    }

    public void Dispose()
    {
        ImGui.SetCurrentContext(_context);
        if (_fontTexture != 0) _gl.DeleteTexture(_fontTexture);
        if (_shader != 0) _gl.DeleteProgram(_shader);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_elementsBuffer != 0) _gl.DeleteBuffer(_elementsBuffer);
        ImGui.DestroyContext(_context);
    }
}
