namespace CitizenBank.Features.Authentication;

class DoesCitizenExistSettings : IDoesCitizenExistSettings
{
    public required String QueryUrlFormat { get; set; }
}
