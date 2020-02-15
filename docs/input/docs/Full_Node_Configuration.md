Title: Full Node Configuration
Description: Advanced customisation options
Order: 10
---
# Ticketbooth API

## Installation

The Ticketbooth API is created as a full node feature, which means that you can use it with a custom full node configuration. This would, for example, allow you to use the Ticketbooth API with your own custom features, or use it on a seperate network.

The Ticketbooth API comes as a [NuGet package](https://www.nuget.org/packages/Ticketbooth.Api/). Simply installing this in your project is enough to use the API, although unfortunately there is a little extra setup required to properly configure the Swagger documentation.

Since the documentation is generated from the ```Ticketbooth.Api``` assembly, you need to specify in your build configuration to copy the generated files to your output folder.

```xml
<PackageReference Include="Ticketbooth.Api" Version="1.0.0-rc1">
    <CopyToOutputDirectory>lib\netcoreapp2.1\*</CopyToOutputDirectory> <!-- Necessary to retrieve XML document in build output -->
</PackageReference>
```

Next, include these custom targets in your project file, which are used during the build process.

```xml
  <!-- Targets taken from https://snede.net/add-nuget-package-xml-documentation-to-swagger/ -->

<Target Name="CopyPackagesOnBuild" AfterTargets="Build">
  <ItemGroup>
    <PackageReferenceFiles Condition="%(PackageReference.CopyToOutputDirectory) != ''" Include="$(NugetPackageRoot)\%(PackageReference.Identity)\%(PackageReference.Version)\%(PackageReference.CopyToOutputDirectory)" />
    </ItemGroup>
    <Copy SourceFiles="@(PackageReferenceFiles)" DestinationFolder="$(OutDir)" />
</Target>

<Target Name="CopyPackagesOnPublish" BeforeTargets="PrepareForPublish">
  <ItemGroup>
    <PackageReferenceFiles Condition="%(PackageReference.CopyToOutputDirectory) != ''" Include="$(NugetPackageRoot)\%(PackageReference.Identity)\%(PackageReference.Version)\%(PackageReference.CopyToOutputDirectory)" />
    </ItemGroup>
    <Copy SourceFiles="@(PackageReferenceFiles)" DestinationFolder="$(PublishDir)" />
</Target>
```

## Adding to the full node

You can use the extension method ```IFullNodeBuilder.AddTicketboothApi()``` to include the Ticketbooth API in your full node configuration. This will run the API inside a Kestrel server, when running the full node.

## Customisation

The Ticketbooth API can be customised by specifying ```TicketboothApiOptions```. Currently the level of customisation only spans to the port on which to serve the API. By default the port is 39200. There are several ways to set the options, the most common being to set them up in the full node builder extension method.

```csharp
IFullNodeBuilder nodeBuilder = new FullNodeBuilder()
    // ...
    .AddSmartContracts(options =>
    {
        options.UseReflectionExecutor();
        options.UsePoAWhitelistedContracts();
    })
    .UseSmartContractPoAConsensus()
    .UseSmartContractPoAMining()
    .UseSmartContractWallet()
    .AddTicketboothApi(options => {
        options.HttpsPort = 39100;
    })
    .Build();
```

An extension method ```IServiceCollection.ConfigureTicketboothApi``` also exists, if you wish to set them directly on the container. The container for the full node can be accessed through the builder.


```csharp
IFullNodeBuilder nodeBuilder = new FullNodeBuilder()
    // ...
    .AddSmartContracts(options =>
    {
        options.UseReflectionExecutor();
        options.UsePoAWhitelistedContracts();
    })
    .UseSmartContractPoAConsensus()
    .UseSmartContractPoAMining()
    .UseSmartContractWallet()
    .AddTicketboothApi();

nodeBuilder.Services.ConfigureTicketboothApi(options => {
    options.HttpsPort = 39100;
});

nodeBuilder.Build();
```