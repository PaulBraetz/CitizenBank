namespace CitizenBank.Features.Shared;
using System;

sealed class LoadCitizenProfilePageSettings : ILoadCitizenProfilePageSettings
{
    public required String QueryUrlFormat { get; set; }
    public required CitizenProfilePageSettings ProfilePageSettings { get; set; }
    ICitizenProfilePageSettings ILoadCitizenProfilePageSettings.ProfilePageSettings => ProfilePageSettings;
}
