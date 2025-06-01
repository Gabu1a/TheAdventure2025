using Silk.NET.Maths;

namespace TheAdventure;

public class Camera
{
    private int _x;
    private int _y;
    private Rectangle<int> _worldBounds = new();

    private float shakeTime = 0f;
    private float shakeIntensity = 0f;
    private Random random = new Random();
    private Vector2D<int> shakeOffset = new Vector2D<int>(0, 0);


    public int X => _x;
    public int Y => _y;

    public readonly int Width;
    public readonly int Height;

    public Camera(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void SetWorldBounds(Rectangle<int> bounds)
    {
        var marginLeft = Width / 2;
        var marginTop = Height / 2;

        if (marginLeft * 2 > bounds.Size.X)
        {
            marginLeft = 48;
        }

        if (marginTop * 2 > bounds.Size.Y)
        {
            marginTop = 48;
        }

        _worldBounds = new Rectangle<int>(marginLeft, marginTop, bounds.Size.X - marginLeft * 2,
            bounds.Size.Y - marginTop * 2);
        _x = marginLeft;
        _y = marginTop;
    }

    public void LookAt(int x, int y)
    {
        if (_worldBounds.Contains(new Vector2D<int>(_x, y)))
        {
            _y = y;
        }
        if (_worldBounds.Contains(new Vector2D<int>(x, _y)))
        {
            _x = x;
        }
    }

    public void TriggerShake(float duration, float intensity)
    {
        shakeTime = duration;
        shakeIntensity = intensity;
    }


    public Rectangle<int> ToScreenCoordinates(Rectangle<int> rect)
    {
        return rect.GetTranslated(new Vector2D<int>(Width / 2 - X, Height / 2 - Y) + shakeOffset);
    }



    public Vector2D<int> ToWorldCoordinates(Vector2D<int> point)
    {
        return point - new Vector2D<int>(Width / 2 - X, Height / 2 - Y);
    }

    public void Update(float deltaTime)
    {
        if (shakeTime > 0)
        {
            shakeTime -= deltaTime;

            int offsetX = (int)((random.NextDouble() * 2 - 1) * shakeIntensity);
            int offsetY = (int)((random.NextDouble() * 2 - 1) * shakeIntensity);

            shakeOffset = new Vector2D<int>(offsetX, offsetY);
        }
        else
        {
            shakeOffset = new Vector2D<int>(0, 0);
        }
    }
}