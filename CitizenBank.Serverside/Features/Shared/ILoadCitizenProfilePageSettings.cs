namespace CitizenBank.Features.Shared;
using System;

interface ILoadCitizenProfilePageSettings
{
    String QueryUrlFormat { get; }
    ICitizenProfilePageSettings ProfilePageSettings { get; }
}
