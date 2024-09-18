namespace Tests.Integration;

using CitizenBank.Features.Authentication;
using CitizenBank.Features.Authentication.Register;

sealed class NeverMatchingPasswordRuleMock : IPasswordRule {
        public String Name { get; } = "NeverMatchesMock";
        public String Description { get; } = "This rule will never match a given password.";
        public Boolean Matches(ClearPassword password) => false;
    }
