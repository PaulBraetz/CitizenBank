using RhoMicro.ApplicationFramework.Hosting;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.CodeAnalysis;

[assembly: RootNamespace("CitizenBank")]
[assembly: ServiceSettings(DefaultVisibility = ServiceVisibility.Internal)]
[assembly: UnionTypeSettings(ToStringSetting = ToStringSetting.Simple)]