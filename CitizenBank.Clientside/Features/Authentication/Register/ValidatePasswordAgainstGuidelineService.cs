namespace CitizenBank.Features.Authentication.Register;

using RhoMicro.ApplicationFramework.Aspects;

partial class ValidatePasswordAgainstGuidelineService(IPasswordGuideline passwordGuideline) 
{
    [ServiceMethod]
    PasswordValidity ValidatePasswordAgainstGuideline(ClearPassword password) => passwordGuideline.Assess(password);
}