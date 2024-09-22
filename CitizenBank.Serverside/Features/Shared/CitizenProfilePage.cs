namespace CitizenBank.Features.Shared;
using System;
using System.Diagnostics;

using HtmlAgilityPack;

sealed class CitizenProfilePage : IEquatable<CitizenProfilePage?>
{
    public CitizenProfilePage(String rawProfilePageHtml, ICitizenProfilePageSettings settings)
    {
        _html = new(() =>
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawProfilePageHtml);
            return doc.DocumentNode;
        });
        _name = new(() =>
        {
            var node = _html.Value.SelectSingleNode(settings.NamePath);
            if(node == null)
                return CitizenName.Empty;
            var result = node.InnerText;
            TryFreeHtmlSource(_bio!, _imagePath!);
            return result;
        });
        _bio = new(() =>
        {
            var node = _html.Value.SelectSingleNode(settings.BioPath);
            if(node == null)
                return CitizenBio.Empty;
            var result = node.InnerHtml;
            TryFreeHtmlSource(_name!, _imagePath!);
            return result;
        });
        _imagePath = new(() =>
        {
            var node = _html.Value.SelectSingleNode(settings.ImagePath);
            if(node == null)
                return CitizenImagePath.Empty;
            var src = node.GetAttributeValue(name: "src", def: String.Empty);
            var result = $"{settings.ImageBasePath}{src}";
            TryFreeHtmlSource(_name!, _bio!);
            return result;
        });
    }

    private Lazy<HtmlNode>? _html;
    private readonly Lazy<CitizenName> _name;
    private readonly Lazy<CitizenBio> _bio;
    private readonly Lazy<CitizenImagePath> _imagePath;
    public CitizenName Name => _name.Value;
    public CitizenBio Bio => _bio.Value;
    public CitizenImagePath ImagePath => _imagePath.Value;

    private void TryFreeHtmlSource<T1, T2>(Lazy<T1> source1, Lazy<T2> source2)
    {
        if(source1.IsValueCreated && source2.IsValueCreated)
            _html = null;
    }

    public override Boolean Equals(Object? obj) => Equals(obj as CitizenProfilePage);
    public Boolean Equals(CitizenProfilePage? other) => other is not null && Name == other.Name && Bio == other.Bio && ImagePath == other.ImagePath;
    public override Int32 GetHashCode() => HashCode.Combine(Name, Bio, ImagePath);
}