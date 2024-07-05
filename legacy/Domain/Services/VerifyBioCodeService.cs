namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Service for verifying a bio code.
/// </summary>
public sealed class VerifyBioCodeService : IService<VerifyBioCode, OneOf<VerifyBioCodeResult, CitizenNotRegisteredResult, CitizenNotExistingResult>>
{
    private readonly IService<LoadBio, OneOf<IBio, CitizenNotExistingResult>> _bioService;
    private readonly IService<ReconstituteConnection, OneOf<ICitizenConnection, CitizenNotRegisteredResult>> _connectionService;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="bioService">The service to use for retrieving a citizens bio.</param>
    /// <param name="citizenService">The service to use for retrieving a citizen with a given name.</param>
    public VerifyBioCodeService(IService<LoadBio, OneOf<IBio, CitizenNotExistingResult>> bioService,
                                IService<ReconstituteConnection, OneOf<ICitizenConnection, CitizenNotRegisteredResult>> citizenService)
    {
        _bioService = bioService;
        _connectionService = citizenService;
    }

    /// <inheritdoc/>
    public async ValueTask<OneOf<VerifyBioCodeResult, CitizenNotRegisteredResult, CitizenNotExistingResult>> Execute(VerifyBioCode query)
    {
        var reconstituteResult = await new ReconstituteConnection(
            query.Name,
            query.CancellationToken).Using(_connectionService);

        if(reconstituteResult.TryPickT1(out var error, out var citizen))
        {
            return error!;
        }

        if(citizen.Code != query.RequiredCode)
        {
            return VerifyBioCodeResult.CitizenMismatch;
        }

        var bio = await new LoadBio(query.Name, query.CancellationToken).Using(_bioService);

        var result = bio.Match<OneOf<VerifyBioCodeResult, CitizenNotRegisteredResult, CitizenNotExistingResult>>(
            bio => bio.Contains(query.RequiredCode) ?
                VerifyBioCodeResult.Match :
                VerifyBioCodeResult.BioMismatch,
            notExisting => notExisting);

        return result;
    }
}
