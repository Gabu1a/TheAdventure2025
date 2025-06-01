using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

namespace TheAdventure;

public unsafe class GameRenderer
{
    private Sdl _sdl;
    private Renderer* _renderer;
    private GameWindow _window;
    private Camera _camera;

    private Dictionary<int, IntPtr> _texturePointers = new();
    private Dictionary<int, TextureData> _textureData = new();
    private int _textureId;
    private int _gameOverTextTextureId = -1;
    private int _restartTextTextureId = -1;


    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _sdl = sdl;

        _renderer = (Renderer*)window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        _window = window;
        var windowSize = window.Size;
        _camera = new Camera(windowSize.Width, windowSize.Height);
        _gameOverTextTextureId = LoadTexture("Assets/GameOverText.png", out var _);
        _restartTextTextureId = LoadTexture("Assets/RetryText.png", out var _);
    }

    public void SetWorldBounds(Rectangle<int> bounds)
    {
        _camera.SetWorldBounds(bounds);
    }

    public void CameraLookAt(int x, int y)
    {
        _camera.LookAt(x, y);
    }

    public Rectangle<int> RenderGameOverScreen()
    {
        SetDrawColor(20, 20, 20, 255);
        ClearScreen();

        var windowWidth = _window.Size.Width;
        var windowHeight = _window.Size.Height;

        // Game Over Background 
        int bannerWidth = 630;
        int bannerHeight = 100;
        int bannerX = (windowWidth - bannerWidth) / 2;
        int bannerY = (windowHeight - bannerHeight) / 2 - 100;

        SetDrawColor(255, 0, 0, 255);
        var gameOverRect = new Rectangle<int>(bannerX, bannerY, bannerWidth, bannerHeight);
        _sdl.RenderFillRect(_renderer, &gameOverRect);

        // Game Over Text
        if (_gameOverTextTextureId != -1)
        {
            const int sourceTextWidth = 710;
            const int sourceTextHeight = 104;

            float scale = Math.Min(bannerWidth / (float)sourceTextWidth, bannerHeight / (float)sourceTextHeight);
            int scaledWidth = (int)(sourceTextWidth * scale);
            int scaledHeight = (int)(sourceTextHeight * scale);

            int textX = bannerX + (bannerWidth - scaledWidth) / 2;
            int textY = bannerY + (bannerHeight - scaledHeight) / 2;

            var destRect = new Rectangle<int>(textX, textY, scaledWidth, scaledHeight);
            RenderTexture(_gameOverTextTextureId, new Rectangle<int>(0, 0, sourceTextWidth, sourceTextHeight), destRect);
        }

        // Retry Button Background
        int buttonWidth = 200;
        int buttonHeight = 60;
        int buttonX = (windowWidth - buttonWidth) / 2;
        int buttonY = (windowHeight - buttonHeight) / 2 + 20;

        SetDrawColor(100, 100, 100, 255);
        var restartRect = new Rectangle<int>(buttonX, buttonY, buttonWidth, buttonHeight);
        _sdl.RenderFillRect(_renderer, &restartRect);

        // Retry Text
        if (_restartTextTextureId != -1)
        {
            const int sourceTextWidth = 200;
            const int sourceTextHeight = 200;

            float scale = Math.Min(buttonWidth / (float)sourceTextWidth, buttonHeight / (float)sourceTextHeight);
            int scaledWidth = (int)(sourceTextWidth * scale);
            int scaledHeight = (int)(sourceTextHeight * scale);

            int textX = buttonX + (buttonWidth - scaledWidth) / 2;
            int textY = buttonY + (buttonHeight - scaledHeight) / 2;

            var destRect = new Rectangle<int>(textX, textY, scaledWidth, scaledHeight);
            RenderTexture(_restartTextTextureId, new Rectangle<int>(0, 0, sourceTextWidth, sourceTextHeight), destRect);
        }

        return restartRect;
    }

    public int LoadTexture(string fileName, out TextureData textureInfo)
    {
        using (var fStream = new FileStream(fileName, FileMode.Open))
        {
            var image = Image.Load<Rgba32>(fStream);
            textureInfo = new TextureData()
            {
                Width = image.Width,
                Height = image.Height
            };
            var imageRAWData = new byte[textureInfo.Width * textureInfo.Height * 4];
            image.CopyPixelDataTo(imageRAWData.AsSpan());
            fixed (byte* data = imageRAWData)
            {
                var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(data, textureInfo.Width,
                    textureInfo.Height, 8, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
                if (imageSurface == null)
                {
                    throw new Exception("Failed to create surface from image data.");
                }

                var imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                if (imageTexture == null)
                {
                    _sdl.FreeSurface(imageSurface);
                    throw new Exception("Failed to create texture from surface.");
                }

                _sdl.FreeSurface(imageSurface);

                _textureData[_textureId] = textureInfo;
                _texturePointers[_textureId] = (IntPtr)imageTexture;
            }
        }
        return _textureId++;
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst,
        RendererFlip flip = RendererFlip.None, double angle = 0.0, Point center = default)
    {
        if (_texturePointers.TryGetValue(textureId, out var imageTexture))
        {
            var translatedDst = _camera.ToScreenCoordinates(dst);
            _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, in src,
                in translatedDst,
                angle,
                in center, flip);
        }
    }

    public Vector2D<int> ToWorldCoordinates(int x, int y)
    {
        return _camera.ToWorldCoordinates(new Vector2D<int>(x, y));
    }

    public void SetDrawColor(byte r, byte g, byte b, byte a)
    {
        _sdl.SetRenderDrawColor(_renderer, r, g, b, a);
    }

    public void ClearScreen()
    {
        _sdl.RenderClear(_renderer);
    }

    public void PresentFrame()
    {
        _sdl.RenderPresent(_renderer);
    }

    public void CameraShake(float duration, float intensity)
    {
        _camera.TriggerShake(duration, intensity);
    }

    public void UpdateCamera(float deltaTime)
    {
        _camera.Update(deltaTime);
    }
}
