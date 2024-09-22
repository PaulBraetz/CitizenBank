namespace CitizenBank.Features.Authentication;

public static class HashedPasswordMinimumParameters
{
    public const Int32 Iterations = 128;
    public const Int32 DegreeOfParallelism=4;
    public const Int32 MemorySize = 1024 * 8;
    public const Int32 OutputLength = 1024;
    public const Int32 AssociatedDataLength = 512;
    public const Int32 KnownSecretLength = 512;
    public const Int32 SaltLength = 512;
}
