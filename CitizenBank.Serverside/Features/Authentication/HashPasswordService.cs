﻿namespace CitizenBank.Features.Authentication;

using System.Threading.Tasks;

using Konscious.Security.Cryptography;

using RhoMicro.ApplicationFramework.Aspects;

partial class HashPasswordService
{
    [ServiceMethodImplementation(Request = typeof(HashPassword), Service = typeof(IHashPasswordService))]
    internal static async ValueTask<HashedPassword> HashPassword(PrehashedPassword password, PasswordParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var clearBytes = password.Digest;
        using var argon = new Argon2id(clearBytes)
        {
            DegreeOfParallelism = parameters.Numerics.DegreeOfParallelism,
            Iterations = parameters.Numerics.Iterations,
            MemorySize = parameters.Numerics.MemorySize,
            Salt = parameters.Data.Salt,
            KnownSecret = parameters.Data.KnownSecret,
            AssociatedData = parameters.Data.AssociatedData
        };
        var hash = await argon.GetBytesAsync(parameters.Numerics.OutputLength);
        var result = new HashedPassword(hash, parameters, password.Parameters);

        return result;
    }
}