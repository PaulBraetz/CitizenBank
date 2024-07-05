namespace TaskforceGenerator.Domain.Authentication.Fakes;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Fake implementation for loading a citizen's bio.
/// </summary>
public sealed class LoadBioServiceFake : IService<LoadBio, OneOf<IBio, CitizenNotExistingResult>>
{
    /// <inheritdoc/>
    public ValueTask<OneOf<IBio, CitizenNotExistingResult>> Execute(LoadBio query)
    {
        var result = (IBio)new Bio($"FakeBioCode{Random.Shared.Next(100)}");

        return ValueTask.FromResult(OneOf<IBio, CitizenNotExistingResult>.FromT0(result));
    }
}
