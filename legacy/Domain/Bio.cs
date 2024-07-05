namespace TaskforceGenerator.Domain.Authentication;
using TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Default implementation of <see cref="IBio"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="bioText">The text posted in the citizen's bio.</param>
public sealed class Bio(String bioText) : IBio
{
    /// <inheritdoc/>
    public Boolean Contains(BioCode code) => bioText.Contains(code);
}
