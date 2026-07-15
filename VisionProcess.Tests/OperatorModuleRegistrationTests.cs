using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Persistence;
using VisionProcess.Persistence.Migrations;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;

namespace VisionProcess.Tests;

public sealed class OperatorModuleRegistrationTests
{
    [Fact]
    public void Catalog_RejectsDuplicateOperatorIds()
    {
        var descriptors = new[]
        {
            CreateDescriptor("Custom"),
            CreateDescriptor("Custom")
        };

        var exception = Assert.Throws<ArgumentException>(() => new OperatorCatalog(descriptors));

        Assert.Contains("Duplicate operator id 'Custom'", exception.Message);
    }

    [Fact]
    public void Catalog_AcceptsCustomOperatorWithoutChangingBuiltInRegistration()
    {
        var custom = CreateDescriptor("Custom.Module", "Custom module");
        var catalog = new OperatorCatalog(BuiltInOperatorDescriptors.All.Append(custom));

        Assert.Same(custom, catalog.Get("Custom.Module"));
        Assert.Equal("Custom module", catalog.Get("Custom.Module").DisplayName);
    }

    [Fact]
    public void OperatorFactory_CreatesCustomRegisteredOperator()
    {
        var catalog = new OperatorCatalog([CreateDescriptor("Custom.Module")]);
        var factory = new OperatorFactory(catalog);

        var instance = factory.Create("Custom.Module");

        Assert.IsType<CustomOperator>(instance);
        Assert.True(factory.Exists("Custom.Module"));
        Assert.False(factory.Exists("Missing"));
    }

    [Fact]
    public void ViewModelFactory_UsesCustomRegistrationWithoutFactoryChanges()
    {
        var expected = new CustomOperatorViewModel();
        var factory = new OperatorViewModelFactory(
        [
            new OperatorViewModelRegistration<CustomOperator>(_ => expected)
        ]);

        var result = factory.Create(new CustomOperator());

        Assert.Same(expected, result);
    }

    [Fact]
    public void ProcessMapper_RoundTripsCustomModuleStateWithoutCoreTypeSwitches()
    {
        var descriptor = new OperatorDescriptor(
            "Custom.Module",
            "Custom module",
            typeof(CustomOperator),
            static () => new CustomOperator(),
            stateVersion: 1,
            static op => BitConverter.GetBytes(((CustomOperator)op).Value),
            static (version, payload) => version == 1
                ? new CustomOperator { Value = BitConverter.ToInt32(payload) }
                : throw new NotSupportedException());
        var mapper = new ProcessMapper(new OperatorCatalog([descriptor]));
        var process = new ProcessModel();
        process.Operations.Add(new OperationModel
        {
            Operator = new CustomOperator { Name = "Custom", Value = 42 }
        });

        var dto = mapper.ToDto(process);
        var serializer = new ProcessSerializer(new MigrationRunner([]));
        var bytes = serializer.Serialize(new VersionedContainer
        {
            MainView = new MainViewDto
            {
                Editors = [new EditorDto { Process = dto }]
            }
        });
        var restoredDto = serializer.Deserialize(bytes)
            .MainView!.Editors.Single().Process!;
        var restored = mapper.ToModel(restoredDto);

        Assert.Null(dto.Operations[0].Operator);
        Assert.Equal("Custom.Module", dto.Operations[0].OperatorState?.OperatorId);
        var restoredOperator = Assert.IsType<CustomOperator>(restored.Operations[0].Operator);
        Assert.Equal("Custom", restoredOperator.Name);
        Assert.Equal(42, restoredOperator.Value);
    }

    private static OperatorDescriptor CreateDescriptor(
        string id,
        string displayName = "Custom")
        => new(id, displayName, typeof(CustomOperator), static () => new CustomOperator());

    private sealed class CustomOperator
        : OperatorBase<InputsEmpty, OutputsEmpty, GraphicsEmpty>
    {
        public int Value { get; set; }

        protected override bool InternalExecute(out string message)
        {
            message = "OK";
            return true;
        }
    }

    private sealed class CustomOperatorViewModel;
}
