//the following file is derived from:
//https://source.dot.net/#Microsoft.AspNetCore.Cryptography.KeyDerivation/PBKDF2/ManagedPbkdf2Provider.cs

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;
#pragma warning restore IDE0130 // Namespace does not match folder structure
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using CitizenBank.Features.Authentication;

/// <summary>
/// A PBKDF2 provider which utilizes the managed hash algorithm classes as PRFs.
/// This isn't the preferred provider since the implementation is slow, but it is provided as a fallback.
/// </summary>
public sealed class YieldingManagedPbkdf2Provider(Yielder yielder)
{
    public async Task<Byte[]> DeriveKey(
        String password,
        PrehashedPasswordParameters parameters,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        ct.ThrowIfCancellationRequested();

        var salt = parameters.Salt;
        var iterationCount = parameters.Iterations;
        var prf = parameters.Prf;
        var numBytesRequested = parameters.HashSize;

        Debug.Assert(password != null);
        Debug.Assert(iterationCount > 0);
        Debug.Assert(numBytesRequested > 0);

        // PBKDF2 is defined in NIST SP800-132, Sec. 5.3.
        // http://csrc.nist.gov/publications/nistpubs/800-132/nist-sp800-132.pdf

        var retVal = new Byte[numBytesRequested];
        var numBytesWritten = 0;
        var numBytesRemaining = numBytesRequested;

        // For each block index, U_0 := Salt || block_index
        var saltWithBlockIndex = new Byte[checked(salt.Length + sizeof(UInt32))];
        Buffer.BlockCopy(salt, 0, saltWithBlockIndex, 0, salt.Length);

        using(var hashAlgorithm = PrfToManagedHmacAlgorithm(prf, password))
        {
            for(UInt32 blockIndex = 1; numBytesRemaining > 0; blockIndex++)
            {
                ct.ThrowIfCancellationRequested();

                // write the block index out as big-endian
                saltWithBlockIndex[^4] = (Byte)( blockIndex >> 24 );
                saltWithBlockIndex[^3] = (Byte)( blockIndex >> 16 );
                saltWithBlockIndex[^2] = (Byte)( blockIndex >> 8 );
                saltWithBlockIndex[^1] = (Byte)blockIndex;

                // U_1 = PRF(U_0) = PRF(Salt || block_index)
                // T_blockIndex = U_1
#pragma warning disable IDE1006 // Naming Styles
                var U_iter = hashAlgorithm.ComputeHash(saltWithBlockIndex); // this is U_1
                var T_blockIndex = U_iter;
#pragma warning restore IDE1006 // Naming Styles

                for(var iter = 1; iter < iterationCount; iter++)
                {
                    ct.ThrowIfCancellationRequested();

                    U_iter = hashAlgorithm.ComputeHash(U_iter);
                    XorBuffers(src: U_iter, dest: T_blockIndex);
                    // At this point, the 'U_iter' variable actually contains U_{iter+1} (due to indexing differences).

                    if(iter % 100 == 0)
                    {
                        await yielder.Yield();
                    }
                }

                // At this point, we're done iterating on this block, so copy the transformed block into retVal.
                var numBytesToCopy = Math.Min(numBytesRemaining, T_blockIndex.Length);
                Buffer.BlockCopy(T_blockIndex, 0, retVal, numBytesWritten, numBytesToCopy);
                numBytesWritten += numBytesToCopy;
                numBytesRemaining -= numBytesToCopy;
            }
        }

        // retVal := T_1 || T_2 || ... || T_n, where T_n may be truncated to meet the desired output length
        return retVal;
    }

    private static KeyedHashAlgorithm PrfToManagedHmacAlgorithm(KeyDerivationPrf prf, String password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            return prf switch
            {
                KeyDerivationPrf.HMACSHA1 => throw new NotSupportedException($"Weak PRF '{prf}' is not supported."),
                KeyDerivationPrf.HMACSHA256 => new HMACSHA256(passwordBytes),
                KeyDerivationPrf.HMACSHA512 => new HMACSHA512(passwordBytes),
                _ => throw new CryptographicException("Assertion failed:  Unrecognized PRF."),
            };
        } finally
        {
            // The HMAC ctor makes a duplicate of this key; we clear original buffer to limit exposure to the GC.
            Array.Clear(passwordBytes, 0, passwordBytes.Length);
        }
    }

    private static void XorBuffers(Byte[] src, Byte[] dest)
    {
        // Note: dest buffer is mutated.
        Debug.Assert(src.Length == dest.Length);
        for(var i = 0; i < src.Length; i++)
        {
            dest[i] ^= src[i];
        }
    }
}