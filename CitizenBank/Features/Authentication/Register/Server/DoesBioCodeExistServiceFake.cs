namespace CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class DoesBioCodeExistServiceFake
{
    [ServiceMethod]
#pragma warning disable IDE0060 // Remove unused parameter
    static Boolean DoesBioCodeExist(CitizenName name, BioCode code) => false;
#pragma warning restore IDE0060 // Remove unused parameter
}
