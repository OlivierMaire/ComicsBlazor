using System.Text.Json.Serialization;

internal class SettingsModel
{
    [JsonIgnore]
    public Action<string>? SettingsChanged { get; set; }
    public DisplayMode _chosenDisplayMode = DisplayMode.DoublePage;
    [JsonIgnore]
    public DisplayMode? _forcedDisplayMode = null;
    [JsonIgnore]
    public DisplayMode DisplayMode
    {
        get => _forcedDisplayMode ?? _chosenDisplayMode;
        set
        {
            if (value != _chosenDisplayMode)
            {
                _chosenDisplayMode = value;
                SettingsChanged?.Invoke(nameof(DisplayMode));
            }
        }
    }

    private ThemeMode _theme = ThemeMode.Auto;
    public ThemeMode Theme
    {
        get => _theme;
        set
        {
            if (value != _theme)
            {
                _theme = value;
                SettingsChanged?.Invoke(nameof(Theme));
            }
        }
    }
    [JsonIgnore]
    public string ThemeClass => $"theme-{Theme}";


    private DisplayRotationMode _displayRotation = DisplayRotationMode.Zero;
    public DisplayRotationMode DisplayRotation
    {
        get => _displayRotation;
        set
        {
            if (value != _displayRotation)
            {
                _displayRotation = value;
                SettingsChanged?.Invoke(nameof(DisplayRotation));
            }
        }
    }

    private FlipMode _flipDisplay = FlipMode.Normal;
    public FlipMode FlipDisplay
    {
        get => _flipDisplay;
        set
        {
            if (value != _flipDisplay)
            {
                _flipDisplay = value;
                SettingsChanged?.Invoke(nameof(FlipDisplay));
            }
        }
    }

    private NavigationMode _direction = NavigationMode.LeftToRight;
    public NavigationMode Direction
    {
        get => _direction;
        set
        {
            if (value != _direction)
            {
                _direction = value;
                SettingsChanged?.Invoke(nameof(Direction));
            }
        }
    }

    private ScrollBarMode _scrollbars = ScrollBarMode.Visible;
    public ScrollBarMode Scrollbars
    {
        get => _scrollbars;
        set
        {
            if (value != _scrollbars)
            {
                _scrollbars = value;
                SettingsChanged?.Invoke(nameof(Scrollbars));
            }
        }
    }
    private ScaleMode _scale = ScaleMode.Best;
    public ScaleMode Scale
    {
        get => _scale; set
        {
            if (value != _scale)
            {
                _scale = value;
                SettingsChanged?.Invoke(nameof(Scale));
            }
        }
    }
    private BackgroundMode _background = BackgroundMode.Dynamic;
    public BackgroundMode Background
    {
        get => _background; set
        {
            if (value != _background)
            {
                _background = value;
                SettingsChanged?.Invoke(nameof(Background));
            }
        }
    }
    private string _backgroundColor = "#FFF";
    public string BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (value != _backgroundColor)
            {
                _backgroundColor = value;
                SettingsChanged?.Invoke(nameof(BackgroundColor));
            }
        }
    }

    private bool _creaseShadow = true;
    public bool CreaseShadow
    {
        get => _creaseShadow;
        set
        {
            if (value != _creaseShadow)
            {
                _creaseShadow = value;
                SettingsChanged?.Invoke(nameof(CreaseShadow));
            }
        }
    }

    private int _preloadBefore = 2;
    public int PreloadBefore
    {
        get => _preloadBefore;
        set
        {
            if (value != _preloadBefore)
            {
                _preloadBefore = value;
                SettingsChanged?.Invoke(nameof(PreloadBefore));
            }
        }
    }
    private int _preloadAfter = 3;
    public int PreloadAfter
    {
        get => _preloadAfter;
        set
        {
            if (value != _preloadAfter)
            {
                _preloadAfter = value;
                SettingsChanged?.Invoke(nameof(PreloadAfter));
            }
        }
    }

    [JsonIgnore]
    public string PageViewClass => $" rotate-{(int)DisplayRotation} flip-{FlipDisplay} scroll-{Scrollbars} scale-{Scale} ";

}

public static class Extensions
{

    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }


    public static T Prev<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) - 1;
        return (j < 0) ? Arr[Arr.Length - 1] : Arr[j];
    }
}


public enum DisplayMode
{
    DoublePage,
    SinglePage
}

public enum FlipMode
{
    Normal = 0,
    Vertical = 1,
    Horizontal = 2,
    VerticalHorizontal = 3
}

public enum NavigationMode
{
    LeftToRight,
    RightToLeft
}

public enum ScaleMode
{
    Best,
    Width,
    Height,
    Native
}

public enum ScrollBarMode
{
    Visible,
    Hidden
}
public enum DisplayRotationMode
{
    Zero = 0,
    Ninety = 90,
    HundredEighty = 180,
    TwoHundredSeventy = 270
}

public enum BackgroundMode
{
    Dynamic,
    Solid
}

public enum ThemeMode
{
    Light,
    Dark,
    Auto
}