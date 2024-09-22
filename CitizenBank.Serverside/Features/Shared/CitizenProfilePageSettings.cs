namespace CitizenBank.Features.Shared;
using System;

sealed class CitizenProfilePageSettings : ICitizenProfilePageSettings
{
    public required String NamePath { get; set; }
    public required String BioPath { get; set; }
    public required String ImagePath { get; set; }
    public required String ImageBasePath { get; set; }
}
