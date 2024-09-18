namespace CitizenBank;
using System;

static class Exceptions
{
    public static NotSupportedException DefinitionNotSupported<T>() => 
        throw new NotSupportedException($"Using service of type {typeof(T).Name} is not supported. A non-specific implementation was registered (likely accidentally).");
}