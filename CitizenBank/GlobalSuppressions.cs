﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("RhoMicro.CodeAnalysis.UnionsGenerator", "RUG0008:Union Type Option Ignored", Justification = "Unnecessary warning that does not indicate actual problems.")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Managed via SynchronizationContextDiscardingDecorator.")]
