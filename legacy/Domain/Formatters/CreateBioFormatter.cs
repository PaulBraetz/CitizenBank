namespace TaskforceGenerator.Domain.Authentication.Formatters;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Formatter for instances of <see cref="CreateBio"/> that formats instances according to an ellipsis limit.
/// </summary>
public sealed class CreateBioFormatter : IStaticFormatter<CreateBio>
{
    private const Int32 ELLIPSIS_LIMIT = 100;
    /// <inheritdoc/>
    public String Format(CreateBio value)
    {
        var bioTextEllipsis = value.BioText.Length > ELLIPSIS_LIMIT ?
            $"{value.BioText[..ELLIPSIS_LIMIT]}...{value.BioText[^ELLIPSIS_LIMIT..]}" :
            value.BioText;
        var result =
            $"{nameof(CreateBio)} {{ " +
                $"{nameof(CreateBio.BioText)} = {bioTextEllipsis}, " +
                $"{nameof(CreateBio.CancellationToken)} = {value.CancellationToken} " +
            $"}}";

        return result;
    }
}
