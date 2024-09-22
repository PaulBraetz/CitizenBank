namespace CitizenBank.Features.Shared;
using System;

interface ICitizenProfilePageSettings
{
    String NamePath { get; }
    String BioPath { get; }
    String ImagePath { get; }
    String ImageBasePath { get; }
}
