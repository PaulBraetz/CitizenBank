namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Factory for creating instances of <see cref="IBio"/>.
/// </summary>
public sealed class BioFactory : IService<CreateBio, IBio>
{
    /// <inheritdoc/>
    public ValueTask<IBio> Execute(CreateBio query)
    {
        IBio result = new Bio(query.BioText);

        return ValueTask.FromResult(result);
    }
}
