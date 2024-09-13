namespace CitizenBank.Features.Authentication;

/// <summary>
/// The numerical values used in password parameters.
/// </summary>
/// <param name="Iterations">The number of iterations to apply to the hash. </param>
/// <param name="DegreeOfParallelism">The number of lanes to use while processing the hash. </param>
/// <param name="MemorySize">The number of 1kiB memory blocks to use while processing the hash. </param>
/// <param name="OutputLength">The amount of bytes in the hash value calculated. </param>
public sealed record PasswordParameterNumerics(Int32 Iterations, Int32 DegreeOfParallelism, Int32 MemorySize, Int32 OutputLength);
