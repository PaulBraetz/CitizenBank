namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Service for creating citizen entities (factory query).
/// </summary>
public sealed class CreateConnectionService : IService<CreateConnection, ICitizenConnection>
{
    /// <inheritdoc/>
    public ValueTask<ICitizenConnection> Execute(CreateConnection query)
    {
        ICitizenConnection citizen = new CitizenConnection(query.CitizenName, query.Code, query.Password);

        return ValueTask.FromResult(citizen);
    }
}
