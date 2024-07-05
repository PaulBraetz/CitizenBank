namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for retrieving the citizen connection associated with the users context.
/// </summary>
public sealed class GetContextConnectionService : IService<GetContextConnection, OneOf<ICitizenConnection, NotAuthenticatedResult>>
{
    private readonly IUserContext _context;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="context">The context for which to set the citizen.</param>
    public GetContextConnectionService(IUserContext context)
    {
        _context = context;
    }
    /// <inheritdoc/>
    public ValueTask<OneOf<ICitizenConnection, NotAuthenticatedResult>> Execute(GetContextConnection request)
    {
        request.CancellationToken.ThrowIfCancellationRequested();
        var connection = _context.Citizen;
        var result = connection == null ?
             NotAuthenticatedResult.Instance :
             OneOf<ICitizenConnection, NotAuthenticatedResult>.FromT0(connection!);

        return ValueTask.FromResult(result);
    }
}
