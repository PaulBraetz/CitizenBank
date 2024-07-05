namespace TaskforceGenerator.Domain.Authentication.Fakes;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

using VerifyBioCodeResultAlias = OneOf.OneOf<
    Requests.VerifyBioCodeResult,
    Domain.Core.Results.CitizenNotRegisteredResult,
    Domain.Core.Results.CitizenNotExistingResult>;

/// <summary>
/// Fake service for verifying a bio code.
/// </summary>
public sealed class VerifyBioCodeAlwaysMatchService :
    IService<VerifyBioCode, VerifyBioCodeResultAlias>
{
    private readonly IService<LoadBio, OneOf<IBio, CitizenNotExistingResult>> _bioService;
    private readonly IService<ReconstituteConnection, OneOf<ICitizenConnection, CitizenNotRegisteredResult>> _connectionService;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="bioService">The service to use for retrieving a citizens bio.</param>
    /// <param name="citizenService">The service to use for retrieving a citizen with a given name.</param>
    public VerifyBioCodeAlwaysMatchService(IService<LoadBio, OneOf<IBio, CitizenNotExistingResult>> bioService,
                                IService<ReconstituteConnection, OneOf<ICitizenConnection, CitizenNotRegisteredResult>> citizenService)
    {
        _bioService = bioService;
        _connectionService = citizenService;
    }

    /// <inheritdoc/>
    public async ValueTask<VerifyBioCodeResultAlias> Execute(VerifyBioCode query)
    {
        //it's a fake! don't worry about not handling results you dummy
        _ = await new ReconstituteConnection(query.Name, query.CancellationToken).Using(_connectionService);
        _ = await new LoadBio(query.Name, query.CancellationToken).Using(_bioService);

        return VerifyBioCodeResult.Match;
    }
}
