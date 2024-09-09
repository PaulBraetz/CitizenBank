namespace CitizenBank.Features.Authentication.Register.Server;

using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.ApplicationFramework.Composition;

[FakeService]
sealed partial class DoesBioCodeExistServiceFake
{
    [ServiceMethod(ServiceInterfaceName ="IBioCodeExistenceChecker")]
    static Boolean DoesBioCodeExist(CitizenName name, BioCode code) => false;
}
