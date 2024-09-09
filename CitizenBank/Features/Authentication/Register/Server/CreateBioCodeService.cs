namespace CitizenBank.Features.Authentication.Register.Server;

using System.Threading.Tasks;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class CreateBioCodeService(IBioCodeExistenceChecker existsService)
{
    const String _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    [ServiceMethod]
    async ValueTask<BioCode> CreateBioCode(CitizenName name, CancellationToken ct)
    {
        var buffer = new Char[16];
        BioCode code;
        Boolean exists;
        do
        {
            for(var i = 0; i < buffer.Length; i++)
                buffer[i] = _alphabet[Random.Shared.Next(_alphabet.Length)];

            code = new String(buffer);
            exists = await existsService.DoesBioCodeExist(name, code, ct);
        } while(exists);

        return code;
    }
}
