using RhoMicro.ApplicationFramework.Hosting;
using RhoMicro.ApplicationFramework.Aspects;
using RhoMicro.CodeAnalysis;

[assembly: RootNamespace("CitizenBank")]
[assembly: ServiceSettings(DefaultVisibility = ServiceVisibility.Public)]
[assembly: UnionTypeSettings(ToStringSetting = ToStringSetting.Simple)]