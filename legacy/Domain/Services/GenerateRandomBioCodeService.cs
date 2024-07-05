namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Factory for creating instances of <see cref="BioCode"/>.
/// </summary>
public sealed class GenerateRandomBioCodeService : IService<GenerateRandomBioCode, BioCode>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="tokenCount">The amount of random tokens in codes created.</param>
    public GenerateRandomBioCodeService(Int32 tokenCount)
    {
        _tokenCount = tokenCount;
    }

    private readonly Int32 _tokenCount;

    /// <inheritdoc/>
    public ValueTask<BioCode> Execute(GenerateRandomBioCode query)
    {
        var chars = Enumerable
            .Repeat(0, _tokenCount)
            .Select(i => Random.Shared.Next('A', 'Z'))
            .Where(c => !Char.IsControl((Char)c))
            .Select((c, i) => (c, i))
            .GroupBy(t => t.i / 4)
            .Select(g => (k: g.Key, cs: g.Select(t => (Char)t.c)))
            .SelectMany(g => g.k > 0 ? g.cs.Prepend('-') : g.cs);

        var result = (BioCode)String.Concat(chars);

        return ValueTask.FromResult(result);
    }
}
