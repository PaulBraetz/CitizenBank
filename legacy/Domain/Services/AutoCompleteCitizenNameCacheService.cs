namespace TaskforceGenerator.Domain.Authentication.Services;
using TaskforceGenerator.Domain.Authentication.Requests;

internal sealed class AutoCompleteCitizenNameCacheService :
    IService<LoadAutoCompleteCitizenName, String?>,
    IService<CacheAutoCompleteCitizenName>
{
    //TODO: implement?

    public ValueTask<String?> Execute(LoadAutoCompleteCitizenName query) =>
        throw new NotImplementedException();
    public ValueTask<ServiceResult> Execute(CacheAutoCompleteCitizenName command) =>
        throw new NotImplementedException();
}
