namespace TaskforceGenerator.Domain.Authentication.Requests;
using System;

/// <summary>
/// Command for caching citizen names for use by autocomplete features.
/// </summary>
public readonly record struct CacheAutoCompleteCitizenName(
    String CitizenName,
    CancellationToken CancellationToken) : IServiceRequest;
