namespace CitizenBank.Features.Authentication.Register.Client;

using RhoMicro.ApplicationFramework.Aspects;

sealed partial class ValidatePasswordAgainstGuidelineService(IPasswordGuideline passwordGuideline) 
{
    [ServiceMethod]
    public PasswordValidity ValidatePasswordAgainstGuideline(ClearPassword password) => passwordGuideline.Assess(password);
}