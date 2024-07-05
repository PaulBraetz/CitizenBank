namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for creating and committing a citizen connection entity.
/// </summary>
public sealed class OpenConnectionService : 
    IService<OpenConnection, OneOf<ServiceResult, ConnectionAlreadyCommittedResult>>
{
    private readonly IService<GenerateInitialPassword, Password> _passwordService;
    private readonly IService<GenerateRandomBioCode, BioCode> _randomBioCodeService;
    private readonly IService<CreateConnection, ICitizenConnection> _connectionService;
    private readonly IService<CommitConnection, OneOf<ServiceResult, ConnectionAlreadyCommittedResult>> _commitService;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="passwordService">The service to use when generating the initial password.</param>
    /// <param name="randomBioCodeService">The service to use when generating the initial bio code.</param>
    /// <param name="citizenService">The service to use when creating a new citizen.</param>
    /// <param name="commitService">The service to use when committing a new citizen to the infrastructure.</param>
    public OpenConnectionService(IService<GenerateInitialPassword, Password> passwordService,
                                  IService<GenerateRandomBioCode, BioCode> randomBioCodeService,
                                  IService<CreateConnection, ICitizenConnection> citizenService,
                                  IService<CommitConnection, OneOf<ServiceResult, ConnectionAlreadyCommittedResult>> commitService)
    {
        _passwordService = passwordService;
        _randomBioCodeService = randomBioCodeService;
        _connectionService = citizenService;
        _commitService = commitService;
    }

    /// <inheritdoc/>
    public async ValueTask<OneOf<ServiceResult, ConnectionAlreadyCommittedResult>> Execute(OpenConnection command)
    {
        var password = await new GenerateInitialPassword().Using(_passwordService);
        var code = await new GenerateRandomBioCode().Using(_randomBioCodeService);
        var connection = await new CreateConnection(command.CitizenName, code, password, command.CancellationToken).Using(_connectionService);
        var commitResult = await new CommitConnection(connection, command.CancellationToken).Using(_commitService);
        var result = commitResult;

        return result;
    }
}
