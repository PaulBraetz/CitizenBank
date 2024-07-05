namespace TaskforceGenerator.Domain.Authentication.Services;

using System.Text;

using TaskforceGenerator.Domain.Authentication.Requests;
using TaskforceGenerator.Domain.Authentication.Results;

/// <summary>
/// Service for creating new passwords.
/// </summary>
public sealed class CreatePasswordService : IService<CreatePassword, OneOf<Password, PasswordCreationGuidelineViolatedResult>>
{
    /// <inheritdoc/>
    public async ValueTask<OneOf<Password, PasswordCreationGuidelineViolatedResult>> Execute(CreatePassword query)
    {
        var clearBytes = Encoding.UTF8.GetBytes(query.ClearPassword);
        using var argon = new Argon2id(clearBytes)
        {
            DegreeOfParallelism = query.Parameters.Numerics.DegreeOfParallelism,
            Iterations = query.Parameters.Numerics.Iterations,
            MemorySize = query.Parameters.Numerics.MemorySize,
            Salt = query.Parameters.Data.Salt,
            KnownSecret = query.Parameters.Data.KnownSecret,
            AssociatedData = query.Parameters.Data.AssociatedData
        };
        var hash = await argon.GetBytesAsync(query.Parameters.Numerics.OutputLength);
        var result = new Password(hash, query.Parameters);

        return result;
    }
}
