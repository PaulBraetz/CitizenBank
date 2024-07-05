namespace TaskforceGenerator.Domain.Authentication.Abstractions;

/// <summary>
/// Represents a citizen's bio.
/// </summary>
public interface IBio
{
    /// <summary>
    /// Evaluates whether the bio contains the code provided.
    /// </summary>
    /// <param name="code">The code to check for.</param>
    /// <returns><see langword="true"/> if the bio contains the code; otherwise, <see langword="false"/>.</returns>
    Boolean Contains(BioCode code);
}
