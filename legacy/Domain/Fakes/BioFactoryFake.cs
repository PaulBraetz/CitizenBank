namespace TaskforceGenerator.Domain.Authentication.Fakes;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Factory producing <see cref="IBio"/> instances that always confirm the bio code passed.
/// /// </summary>
public sealed class BioFactoryFake : IService<CreateBio, IBio>
{
    private sealed class FakeBio : IBio
    {
        public Boolean Contains(BioCode code) => true;
    }

    /// <inheritdoc/>
    public ValueTask<IBio> Execute(CreateBio query)
    {
        IBio result = new FakeBio();

        return ValueTask.FromResult(result);
    }
}
