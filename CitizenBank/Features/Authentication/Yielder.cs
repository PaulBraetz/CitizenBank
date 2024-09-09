//the following file is derived from:
//https://source.dot.net/#Microsoft.AspNetCore.Cryptography.KeyDerivation/PBKDF2/ManagedPbkdf2Provider.cs
#pragma warning disable 

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.AspNetCore.Cryptography.KeyDerivation.PBKDF2;

sealed class Yielder
{
    public Yielder(TimeSpan yieldInterval, TimeSpan yieldTime)
    {
        _yieldInterval = yieldInterval;
        _yieldTime = yieldTime;
        _yield = true;
    }
    public Yielder() { }

    private readonly Boolean _yield;
    private readonly TimeSpan _yieldInterval;
    private readonly TimeSpan _yieldTime;
    private DateTime _lastTimeStamp = DateTime.Now;

    public async ValueTask Yield()
    {
        if(!_yield || DateTime.Now - _lastTimeStamp <= _yieldInterval)
            return;

        await Task.Delay(_yieldTime);

        _lastTimeStamp = DateTime.Now;

        return;
    }
}
