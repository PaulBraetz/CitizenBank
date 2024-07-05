namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Abstractions;
using TaskforceGenerator.Domain.Authentication.Requests;

/// <summary>
/// Service for setting the connection on user contexts.
/// </summary>
public sealed class SetContextConnectionService : IService<SetContextConnection>
{
    private readonly IUserContext _context;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="context">The context for which to set the citizen.</param>
    public SetContextConnectionService(IUserContext context)
    {
        _context = context;
    }
    /// <inheritdoc/>
    public ValueTask<ServiceResult> Execute(SetContextConnection command)
    {
        if(command.CancellationToken.IsCancellationRequested)
        {
            return ServiceResult.CompliantlyCancelled.Task.AsValueTask();
        }

        _context.Citizen = command.Connection;
        
        return ServiceResult.Completed.Task.AsValueTask();
    }
}
