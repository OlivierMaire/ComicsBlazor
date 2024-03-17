using System.Drawing;
using System.Dynamic;
using ComicsBlazor.Components;
using Microsoft.VisualBasic;
using SkiaSharp;
using SoloX.BlazorJsBlob;

namespace ComicsBlazor.Services;



public class ComicService(BlobManagerService blobManagerService, ComicsJsInterop interopService)
{
    private readonly BlobManagerService blobManagerService = blobManagerService;
    private readonly ComicsJsInterop interopService = interopService;

    public Func<int, Task<PageData?>>? GetPage { private get; set; } = null;

    private string _title { get; set; } = string.Empty;
    public string Title { get => _title; set { if (_title != value) { _title = value; TitleChanged?.Invoke(); } } }
    public Action? TitleChanged { get; set; }

    private DisplayMode _chosenDisplayMode = DisplayMode.DoublePage;
    private DisplayMode? _forcedDisplayMode = null;
    public DisplayMode DisplayMode
    {
        get => _forcedDisplayMode ?? _chosenDisplayMode;
        set { if (value != _chosenDisplayMode) _chosenDisplayMode = value; }
    }
    public Action? DisplayModeChanged { get; set; }

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
                CurrentPageChanged?.Invoke();
            }
        }

    }

    public Action? PagesChanged { get; set; }
    public Action? CurrentPageChanged { get; set; }


    public async Task InitAsync(ComicMetadata meta)
    {
        Title = meta.Title ?? string.Empty;
        Pages = meta.Pages.Select(p => new Page(p)).ToList();
        RecalculatePageNumbers();
    }

    public async Task InitInteropAsync()
    {
        interopService.OnWindowResized += WindowResized;
        await interopService.Init();

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
            Console.WriteLine($"limitWidth: {size.Height}x{limitWidth}");

            if (size.Width < limitWidth * 2) // can't fit 2 pages
                calculatedDisplayMode = DisplayMode.SinglePage;

        }


        if (_forcedDisplayMode != calculatedDisplayMode)
        {
            if (calculatedDisplayMode == null && _chosenDisplayMode == DisplayMode.DoublePage) // double page
            {
                // move to odd page
                if (Pages[CurrentPage].RealPageNumber % 2 == 0 && CurrentPage > 0)
                {
                    CurrentPage--;
                }

            }

            _forcedDisplayMode = calculatedDisplayMode;

            Console.WriteLine($"New Display mode : {DisplayMode}");
            DisplayModeChanged?.Invoke();
        }
    }


    public async Task<Page?> LoadPage(int position)
    {
        Console.WriteLine($"Load Page {position}");
        var page = Pages[position];


        if (GetPage != null)
        {
            var pageData = await GetPage(page.PageNumber);
            if (pageData != null)
            {
                IBlob blob = await blobManagerService.AddBlobAsync(page.Key, pageData.Data, pageData.ContentType);
                Console.WriteLine($"Blob Added {blob.Uri}");

                page.BlobUri = blob.Uri;
                using (SKBitmap sourceBitmap = SKBitmap.Decode(pageData.Data))
                {
                    page.ImageWidth = (uint)sourceBitmap.Width;
                    page.ImageHeight = (uint)sourceBitmap.Height;
                    page.DoublePage = page.ImageWidth > page.ImageHeight;

                    if (page.DoublePage)
                        RecalculatePageNumbers();
                }
                PagesChanged?.Invoke();
                return page;
            }
        }
        else
        {
            Console.WriteLine("GetPage function not provided");
        }
        return page;
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
        Console.WriteLine($"load at position {CurrentPage}");

        if (CurrentPage < Pages.Count && Pages[CurrentPage].BlobUri == null)
            await this.LoadPage(CurrentPage);
        if (CurrentPage + 1 < Pages.Count && Pages[CurrentPage + 1].BlobUri == null)
            await this.LoadPage(CurrentPage + 1);
        if (CurrentPage - 1 >= 0 && Pages[CurrentPage - 1].BlobUri == null)
            await this.LoadPage(CurrentPage - 1);

        // prev/next 2 pages loaded, we can display
        CurrentPageChanged?.Invoke();

        // continue loading the rest
        if (CurrentPage + 2 < Pages.Count && Pages[CurrentPage + 2].BlobUri == null)
            await this.LoadPage(CurrentPage + 2);
        if (CurrentPage - 2 >= 0 && Pages[CurrentPage - 2].BlobUri == null)
            await this.LoadPage(CurrentPage - 2);
    }

    public void GoToPage(Page page)
    {
        var plannedPage = Pages.FindIndex(p => p.PageNumber == page.PageNumber);


        if (this.DisplayMode == DisplayMode.DoublePage)
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
        if (this.DisplayMode == DisplayMode.DoublePage && this.CurrentPage + 1 < this.Pages.Count &&
       !(this.Pages[this.CurrentPage].DoublePage ||
       this.Pages[this.CurrentPage].PageType == PageType.FrontCover ||
        this.Pages[this.CurrentPage + 1].DoublePage))
            return true;
        return false;
    }

    public void MoveNext()
    {
        if (DisplayMode == DisplayMode.DoublePage)
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


    public void MoveBack()
    {
        if (DisplayMode == DisplayMode.DoublePage)
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
    } + (this.DoublePage ? " double-page" : "");


    public int? RealPageNumber { get; set; }
    public string RealPageNumberString => DoublePage ? (RealPageNumber != null ? $"{RealPageNumber} - {RealPageNumber + 1}" : "") : RealPageNumber.ToString() ?? string.Empty;

}

public enum DisplayMode
{
    DoublePage,
    SinglePage
}
