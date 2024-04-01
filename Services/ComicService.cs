using System.Drawing;
using System.Dynamic;
using ComicsBlazor.Components;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoloX.BlazorJsBlob;
using Size = System.Drawing.Size;

namespace ComicsBlazor.Services;



public class ComicService(BlobManagerService blobManagerService, ComicsJsInterop interopService, Blazored.LocalStorage.ILocalStorageService localStorage, ILogger<ComicService> logger)
{
    private readonly BlobManagerService blobManagerService = blobManagerService;
    private readonly ComicsJsInterop interopService = interopService;
    private readonly Blazored.LocalStorage.ILocalStorageService localStorage = localStorage;
    private readonly ILogger<ComicService> _logger = logger;

    public Func<int, Task<PageData?>>? GetPage { private get; set; } = null;
    public Action<string>? SaveBookmark { private get; set; } = null;

    private string _title { get; set; } = string.Empty;
    public string Title { get => _title; set { if (_title != value) { _title = value; TitleChanged?.Invoke(); } } }
    public Action? TitleChanged { get; set; }
    public Action? ThemeChanged { get; set; }
    public Action? ToggleSettings { get; set; }

    public Action? DisplayModeChanged { get; set; }
    public Action? SettingsChanged { get; set; }

    internal SettingsModel Settings { get; set; } = new();

    public List<Page> Pages { get; set; } = [];

    private int _currentPage = 0;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                Task.Run(async () => await LoadAtPosition());
                SaveBookmark?.Invoke(value.ToString());
                CurrentPageChanged?.Invoke();
            }
        }

    }

    public Action? PagesChanged { get; set; }
    public Action? CurrentPageChanged { get; set; }

    public void Init(ComicMetadata meta)
    {
        Title = meta.Title ?? string.Empty;
        Pages = meta.Pages.Select(p => new Page(p)).ToList();
        RecalculatePageNumbers();

        Settings.SettingsChanged += HandleSettingsChange;
    }

    public async Task InitInteropAsync()
    {
        interopService.OnWindowResized += WindowResized;
        interopService.OnKeyDown += KeyDown;
        await interopService.Init();

    }

    public async Task InitSettings()
    {
        var settings = await localStorage.GetItemAsync<SettingsModel>("ComicsBlazor.Settings");
        if (settings != null)
        {
            this.Settings.Background = settings.Background;
            this.Settings.BackgroundColor = settings.BackgroundColor;
            this.Settings.CreaseShadow = settings.CreaseShadow;
            this.Settings.Direction = settings.Direction;
            this.Settings.DisplayMode = settings.DisplayMode;
            this.Settings.DisplayRotation = settings.DisplayRotation;
            this.Settings.FlipDisplay = settings.FlipDisplay;
            this.Settings.PreloadAfter = settings.PreloadAfter;
            this.Settings.PreloadBefore = settings.PreloadBefore;
            this.Settings.Scale = settings.Scale;
            this.Settings.Scrollbars = settings.Scrollbars;
            this.Settings.Theme = settings.Theme;
        }
    }

    private void KeyDown(string key)
    {
        if (key == "ArrowLeft")
            MoveBack();
        else if (key == "ArrowRight")
            MoveNext();

        if (key == "r")
        {
            Settings.DisplayRotation = Settings.DisplayRotation.Next();
        }

        if (key == "l")
        {
            Settings.DisplayRotation = Settings.DisplayRotation.Prev();
        }

        if (key == "f")
        {
            Settings.FlipDisplay = Settings.FlipDisplay.Next();
        }

        if (key == "d")
        {
            Settings.Direction = Settings.Direction.Next();
        }

        if (key == "1")
        {
            Settings.DisplayMode = DisplayMode.SinglePage;
        }

        if (key == "2")
        {
            Settings.DisplayMode = DisplayMode.DoublePage;
        }

        if (key == "s")
        {
            Settings.Scrollbars = Settings.Scrollbars.Next();
        }

        if (key == "b")
        {
            Settings.Scale = ScaleMode.Best;
        }

        if (key == "w")
        {
            Settings.Scale = ScaleMode.Width;
        }

        if (key == "h")
        {
            Settings.Scale = ScaleMode.Height;
        }

        if (key == "n")
        {
            Settings.Scale = ScaleMode.Native;
        }

    }

    private async void HandleSettingsChange(string settingName)
    {
        if (settingName == nameof(Settings.DisplayMode) ||
        settingName == nameof(Settings.DisplayRotation) ||
        settingName == nameof(Settings.FlipDisplay) ||
        settingName == nameof(Settings.Scrollbars) ||
        settingName == nameof(Settings.Scale) ||
        settingName == nameof(Settings.Background) ||
        settingName == nameof(Settings.BackgroundColor) ||
        settingName == nameof(Settings.CreaseShadow)
        )
        {
            DisplayModeChanged?.Invoke();
        }
        if (settingName == nameof(Settings.Direction)
           )
        {
            SettingsChanged?.Invoke();
        }
        if (settingName == nameof(Settings.Theme)
           )
        {
            ThemeChanged?.Invoke();
        }

        await localStorage.SetItemAsync("ComicsBlazor.Settings", Settings);
    }

    private void WindowResized(Size size)
    {
        DisplayMode? calculatedDisplayMode = null;
        if (size.Width < size.Height)
            calculatedDisplayMode = DisplayMode.SinglePage;
        var page = Pages.FirstOrDefault(p => p.BlobUri != null); // get first loaded image
        if (page != null)
        {
            var ratio = (size.Height - 20) / (double?)page.ImageHeight;
            var limitWidth = page.ImageWidth * ratio;

            if (size.Width < limitWidth * 2) // can't fit 2 pages
                calculatedDisplayMode = DisplayMode.SinglePage;
        }


        if (Settings._forcedDisplayMode != calculatedDisplayMode)
        {
            if (calculatedDisplayMode == null && Settings._chosenDisplayMode == DisplayMode.DoublePage) // double page
            {
                // move to odd page
                if (Pages[CurrentPage].RealPageNumber % 2 == 0 && CurrentPage > 0)
                {
                    CurrentPage--;
                }
            }

            Settings._forcedDisplayMode = calculatedDisplayMode;

            DisplayModeChanged?.Invoke();
        }
    }


    public async Task<Page?> LoadPage(int position)
    {
        var page = Pages[position];


        if (GetPage != null)
        {
            var pageData = await GetPage(page.PageNumber);
            if (pageData != null)
            {
                IBlob blob = await blobManagerService.AddBlobAsync(page.Key, pageData.Data, pageData.ContentType);

                page.BlobUri = blob.Uri;

                try
                {
                    _logger.LogCritical("IMAGESHARP HERE");

                    using (Image<Rgba32> image = Image.Load<Rgba32>(pageData.Data))
                    {
                        _logger.LogCritical("IMAGESHARP HERE1");
                        page.ImageWidth = (uint)image.Width;
                        page.ImageHeight = (uint)image.Height;
                        page.DoublePage = page.ImageWidth > page.ImageHeight;
                        _logger.LogCritical("IMAGESHARP HERE2");

                        (page.ColorLeft, page.ColorRight) = GetMainColorFromLeftStrip(image);
                        _logger.LogCritical($"{page.ColorLeft} {page.ColorRight}");
                        _logger.LogCritical("IMAGESHARP HERE3");

                        if (page.DoublePage)
                            RecalculatePageNumbers();

                        _logger.LogCritical("IMAGESHARP OUT");

                    }
                    PagesChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("IMAGESHARP EXCEPTION");
                    _logger.LogCritical(ex, "IMAGESHARP EXCEPTION");

                }

                return page;
            }
        }
        else
        {
            Console.WriteLine("GetPage function not provided");
        }
        return page;
    }

    private (string, string) GetMainColorFromLeftStrip(Image<Rgba32> image)
    {
        Dictionary<Rgba32, int> colorOccurrencesLeft = new Dictionary<Rgba32, int>();
        Dictionary<Rgba32, int> colorOccurrencesRight = new Dictionary<Rgba32, int>();

        // Loop through the first 10 pixels from the left side of the image
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                var currentColor = image[x, y];

                if (colorOccurrencesLeft.ContainsKey(currentColor))
                {
                    colorOccurrencesLeft[currentColor]++;
                }
                else
                {
                    colorOccurrencesLeft[currentColor] = 1;
                }
            }

            for (int x = image.Width - 1; x >= image.Width - 10; x--)
            {
                var currentColor = image[x, y];

                if (colorOccurrencesRight.ContainsKey(currentColor))
                {
                    colorOccurrencesRight[currentColor]++;
                }
                else
                {
                    colorOccurrencesRight[currentColor] = 1;
                }
            }
        }

        // Find the most frequent color
        var mainColorLeft = colorOccurrencesLeft.OrderByDescending(c => c.Value).First().Key;
        var mainColorRight = colorOccurrencesRight.OrderByDescending(c => c.Value).First().Key;

        return ($"#{mainColorLeft.R:X2}{mainColorLeft.G:X2}{mainColorLeft.B:X2}", $"#{mainColorRight.R:X2}{mainColorRight.G:X2}{mainColorRight.B:X2}");
    }

    private void RecalculatePageNumbers()
    {
        int i = 0;
        foreach (var p in Pages)
        {
            p.RealPageNumber = i;
            if (p.DoublePage)
                i++;
            i++;
        }
    }

    public async Task LoadAtPosition()
    {
        // load current page
        if (CurrentPage < Pages.Count && Pages[CurrentPage].BlobUri == null)
            await this.LoadPage(CurrentPage);

        if (Settings.PreloadAfter >= 1)
            if (CurrentPage + 1 < Pages.Count && Pages[CurrentPage + 1].BlobUri == null)
                await this.LoadPage(CurrentPage + 1);
        if (Settings.PreloadBefore >= 1)
            if (CurrentPage - 1 >= 0 && Pages[CurrentPage - 1].BlobUri == null)
                await this.LoadPage(CurrentPage - 1);

        // prev/next 2 pages loaded, we can display
        CurrentPageChanged?.Invoke();

        // continue loading the rest
        for (int i = 2; i <= Settings.PreloadAfter; i++)
        {
            if (CurrentPage + i < Pages.Count && Pages[CurrentPage + i].BlobUri == null)
                await this.LoadPage(CurrentPage + i);
        }
        for (int i = 2; i <= Settings.PreloadBefore; i++)
        {
            if (CurrentPage - i >= 0 && Pages[CurrentPage - i].BlobUri == null)
                await this.LoadPage(CurrentPage - i);
        }
    }

    public void GoToPage(Page page)
    {
        var plannedPage = Pages.FindIndex(p => p.PageNumber == page.PageNumber);


        if (Settings.DisplayMode == DisplayMode.DoublePage)
        {
            // move to odd page
            if (page.RealPageNumber % 2 == 0 && plannedPage > 0)
            {
                plannedPage--;
            }

        }

        CurrentPage = plannedPage;
    }


    public bool CanViewRightPage()
    {
        if (Settings.DisplayMode == DisplayMode.DoublePage && this.CurrentPage + 1 < this.Pages.Count &&
       !(this.Pages[this.CurrentPage].DoublePage ||
       this.Pages[this.CurrentPage].PageType == PageType.FrontCover ||
        this.Pages[this.CurrentPage + 1].DoublePage))
            return true;
        return false;
    }

    public void MoveNext(bool forced = false)
    {
        if (Settings.Direction == NavigationMode.RightToLeft && !forced)
        {
            MoveBack(true);
            return;
        }

        if (Settings.DisplayMode == DisplayMode.DoublePage)
        {
            if (CurrentPage + 1 >= Pages.Count)
                return;
            if (Pages[CurrentPage].DoublePage || Pages[CurrentPage].PageType == PageType.FrontCover || Pages[CurrentPage + 1].DoublePage)
            {
                //stop here
                CurrentPage = CurrentPage + 1;
                return;
            }

            if (CurrentPage + 2 >= Pages.Count)
                return;
            CurrentPage = CurrentPage + 2;
        }
        else
        {
            if (CurrentPage + 1 >= Pages.Count)
                return;
            CurrentPage = CurrentPage + 1;
        }

    }


    public void MoveBack(bool forced = false)
    {
        if (Settings.Direction == NavigationMode.RightToLeft && !forced)
        {
            MoveNext(true);
            return;
        }

        if (Settings.DisplayMode == DisplayMode.DoublePage)
        {
            if (CurrentPage - 1 < 0)
                return;
            if (Pages[CurrentPage - 1].DoublePage || CurrentPage + 1 == Pages.Count /*last page*/)
            {
                //stop here
                CurrentPage = CurrentPage - 1;
                return;
            }


            if (CurrentPage - 2 < 0)
            {
                CurrentPage = CurrentPage - 1;
                return;
            }

            if (Pages[CurrentPage - 2].DoublePage)
            {
                CurrentPage = CurrentPage - 1;
            }
            else
                CurrentPage = CurrentPage - 2;

        }
        else
        {
            if (CurrentPage - 1 < 0)
                return;
            CurrentPage = CurrentPage - 1;
        }
    }


}


public class Page : ComicMetadata.PageInfo
{
    public Page() { }
    public Page(ComicMetadata.PageInfo pi)
    {
        this.Bookmark = pi.Bookmark;
        this.DoublePage = pi.DoublePage;
        this.ImageHeight = pi.ImageHeight;
        this.ImageSize = pi.ImageSize;
        this.ImageWidth = pi.ImageWidth;
        this.Key = pi.Key;
        this.PageNumber = pi.PageNumber;
        this.PageType = pi.PageType;
    }

    public Uri? BlobUri { get; set; }

    public string PageClass => this.PageType switch
    {
        PageType.FrontCover => " front-cover",
        _ => ""
    }
    + (this.DoublePage ? " double-page" : "")
    + (this.RealPageNumber % 2 == 0 ? " even" : " odd")
    + (this.BlobUri != null ? " loaded" : "");


    public int? RealPageNumber { get; set; }
    public string RealPageNumberString => DoublePage ? (RealPageNumber != null ? $"{RealPageNumber} - {RealPageNumber + 1}" : "") : RealPageNumber.ToString() ?? string.Empty;

    public string ColorLeft { get; internal set; } = string.Empty;
    public string ColorRight { get; internal set; } = string.Empty;
}

