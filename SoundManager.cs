using System.Media;

namespace TheAdventure;

public static class SoundManager
{
    private static SoundPlayer? _explosion;
    private static SoundPlayer? _death;
    private static SoundPlayer? _click;

    public static void LoadSounds()
    {
        _explosion = new SoundPlayer("Assets/Sounds/explosion.wav");
        _death = new SoundPlayer("Assets/Sounds/death.wav");
        _click = new SoundPlayer("Assets/Sounds/click.wav");

        _explosion.Load();
        _death.Load();
        _click.Load();
    }

    public static void PlayExplosion()
    {
        _explosion?.Play();
    }

    public static void PlayDeath()
    {
        _death?.Play();
    }

    public static void PlayClick()
    {
        _click?.Play();
    }
}