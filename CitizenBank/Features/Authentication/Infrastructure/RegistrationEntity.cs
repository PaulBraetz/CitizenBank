﻿namespace CitizenBank.Features.Authentication.Infrastructure;

using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(Name))]
public class RegistrationEntity
{
    public required String Name { get; set; }
    public required HashedPasswordEntity Password { get; set; }
    public Registration ToRegistration() =>
        new(Name: Name,
            Password: Password.ToHashedPassword());
    public static RegistrationEntity FromRegistration(Registration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        return new()
        {
            Name = registration.Name,
            Password = HashedPasswordEntity.FromHashedPassword(registration.Password)
        };
    }
}