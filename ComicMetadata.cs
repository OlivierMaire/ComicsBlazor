using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ComicsBlazor;
public record ComicMetadata()
{
    public bool IsEmpty { get; set; } = true;
    public string TagOrigin { get; set; } = string.Empty;

    public string? Title { get; set; }
    public string? Series { get; set; }
    public int? Issue { get; set; } // Number
    public int? IssueCount { get; set; } // Count
    public int? Volume { get; set; }
    public int? VolumeCount { get; set; }

    public string? AlternateSeries { get; set; }
    public int? AlternateIssue { get; set; }
    public int? AlternateIssueCount { get; set; }

    public string? Summary { get; set; }  // use same way as Summary in CIX 
    public string? Notes { get; set; }

    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? Day { get; set; }

    public string? Publisher { get; set; }
    public string? Imprint { get; set; }
    public string? Genre { get; set; }
    public string[] Tags { get; set; } = [];
    public string? WebLink { get; set; }
    public int? PageCount { get; set; }
    public string? Language { get; set; }  // 2 letters iso code
    public string? Format { get; set; }
    public bool? BlackAndWhite { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Manga Manga { get; set; } = Manga.Unknown;
    public string[] Characters { get; set; } = [];
    public string? Teams { get; set; }
    public string? Locations { get; set; }
    public string? ScanInfo { get; set; }
    public string? StoryArc { get; set; }
    public string? StoryArcNumber { get; set; }
    public string? SeriesGroup { get; set; }
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public AgeRating? MaturityRating { get; set; }
    public string? CommunityRating { get; set; }
    public string? MainCharacterOrTeam { get; set; }
    public string? Review { get; set; }
    public string? GTIN { get; set; }

    public string? Country { get; set; }

    public PageInfo[] Pages { get; set; } = [];

    // Some CoMet-only items
    public double? Price { get; set; }
    public string? IsVersionOf { get; set; }
    public string? Rights { get; set; }
    public string? Identifier { get; set; }
    public int? LastMark { get; set; }
    public string? CoverImage { get; set; }


    public Dictionary<JobTag, string[]> Credits {get;set;} = [];

    public class PageInfo
    {
        public int PageNumber { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PageType PageType { get; set; } = PageType.Unknown;
        public bool DoublePage { get; set; } = false;
        public long ImageSize { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Bookmark { get; set; } = string.Empty;
        public uint? ImageWidth { get; set; }
        public uint? ImageHeight { get; set; }

    }
}

public enum JobTag
{
    Creator,
    Writer,
    Penciller,
    Inker,
    Colorist,
    Letterer,
    Editor,
    CoverArtist,
    Translator
}

public enum PageType
{
    Unknown,
    FrontCover,
    InnerCover,
    Roundup,
    Story,
    Advertisement,
    Editorial,
    Letters,
    Preview,
    BackCover,
    Other,
    Deleted,
}

public enum Manga
{

    Unknown,
    No,
    Yes,
    YesAndRightToLeft,
}

public enum AgeRating
{
    Unknown,
    [EnumMember(Value = "Adults Only 18+")]
    AdultsOnly18,
    [EnumMember(Value = "Early Childhood")]
    EarlyChildhood,
    Everyone,
    [EnumMember(Value = "Everyone 10+")]
    Everyone10,
    G,
    [EnumMember(Value = "Kids to Adults")]
    KidstoAdults,
    M,
    [EnumMember(Value = "MA15+")]
    MA15,
    [EnumMember(Value = "Mature 17+")]
    Mature17,
    PG,
    [EnumMember(Value = "R18+")]
    R18,
    [EnumMember(Value = "Rating Pending")]
    RatingPending,
    Teen,
    [EnumMember(Value = "X18+")]
    X18,
}

public record PageData (byte[] Data, string ContentType);