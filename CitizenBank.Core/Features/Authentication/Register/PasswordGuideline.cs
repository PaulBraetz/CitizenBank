namespace CitizenBank.Features.Authentication.Register;

using System.Collections.Immutable;

sealed class PasswordGuideline(ImmutableArray<IPasswordRule> rules) : IPasswordGuideline
{
    public PasswordValidity Assess(ClearPassword password) =>
        new([.. rules.Select(r => r.Matches(password) ? null : r).OfType<IPasswordRule>()]);
}