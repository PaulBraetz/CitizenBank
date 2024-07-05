namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Queries;
using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for ensuring a citizen is available to the system.
/// </summary>
public sealed class EnsureCitizenConnectionService : IService<EnsureCitizenConnection, ServiceResult>
{
    private readonly IService<CheckConnectionExists, Boolean> _existsService;
    private readonly IService<OpenConnection, OneOf<ServiceResult, ConnectionAlreadyCommittedResult>> _openConnectionService;
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="openConnectionService">The service to use when opening a connection.</param>
    /// <param name="existsService">The service to use when first checking whether a connection already exists.</param>
    public EnsureCitizenConnectionService(IService<OpenConnection, OneOf<ServiceResult, ConnectionAlreadyCommittedResult>> openConnectionService, IService<CheckConnectionExists, Boolean> existsService)
    {
        _openConnectionService = openConnectionService;
        _existsService = existsService;
    }

    /// <inheritdoc/>
    public async ValueTask<ServiceResult> Execute(EnsureCitizenConnection command)
    {
        var (name, token) = command;
        if(token.IsCancellationRequested)
        {
            return ServiceResult.CompliantlyCancelled;
        }

        var exists = await new CheckConnectionExists(name, token).Using(_existsService);
        if(exists)
        {
            return ServiceResult.CompliantlyCancelled;
        }

        var openResult = await new OpenConnection(name, token).Using(_openConnectionService);
        var result = openResult.IsT0 ?
            openResult.AsT0 :
            ServiceResult.Completed;

        return result;
    }
}
