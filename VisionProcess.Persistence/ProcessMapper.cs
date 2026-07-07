using Newtonsoft.Json;
using System.Windows;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Contracts;
using VisionProcess.Core.Helpers;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Tooling;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 在运行时模型 <see cref="ProcessModel"/> 与持久化 DTO <see cref="ProcessDto"/> 之间双向映射。
    /// </summary>
    public static class ProcessMapper
    {
        private static readonly JsonSerializerSettings JsonSettings = CreateJsonSettings();

        public static ProcessDto ToDto(ProcessModel process)
        {
            var dto = new ProcessDto
            {
                Operations = process.Operations.Select(ToDto).ToList(),
                Connections = process.Connections.Select(ToDto).ToList(),
                SelectedOperationIds = process.SelectedOperations.Select(o => o.Id).ToList(),
                SelectedOperationId = process.SelectedOperation?.Id
            };
            return dto;
        }

        public static ProcessModel ToModel(ProcessDto dto)
        {
            var process = new ProcessModel();

            var operationMap = new Dictionary<Guid, OperationModel>();
            foreach (var operationDto in dto.Operations)
            {
                var operation = ToModel(operationDto);
                process.Operations.Add(operation);
                operationMap[operation.Id] = operation;
            }

            foreach (var connectionDto in dto.Connections)
            {
                if (connectionDto.Input is null || connectionDto.Output is null)
                    continue;

                var inputOperation = operationMap.GetValueOrDefault(connectionDto.Input.OwnerId);
                var outputOperation = operationMap.GetValueOrDefault(connectionDto.Output.OwnerId);
                if (inputOperation is null || outputOperation is null)
                    continue;

                var inputConnector = inputOperation.Inputs.FirstOrDefault(c => c.ValuePath == connectionDto.Input.ValuePath);
                var outputConnector = outputOperation.Outputs.FirstOrDefault(c => c.ValuePath == connectionDto.Output.ValuePath);
                if (inputConnector is null || outputConnector is null)
                    continue;

                inputConnector.IsConnected = true;
                outputConnector.IsConnected = true;
                outputConnector.ValueObservers.Add(inputConnector);
                process.Connections.Add(new ConnectionModel { Input = inputConnector, Output = outputConnector });
            }

            if (dto.SelectedOperationId.HasValue && operationMap.TryGetValue(dto.SelectedOperationId.Value, out var selected))
            {
                process.SelectedOperation = selected;
            }

            foreach (var id in dto.SelectedOperationIds)
            {
                if (operationMap.TryGetValue(id, out var op))
                {
                    process.SelectedOperations.Add(op);
                }
            }

            return process;
        }

        private static OperationDto ToDto(OperationModel operation)
        {
            OperationDto dto = operation is OperationGroupModel group
                ? new OperationGroupDto { GroupSize = ToDto(group.GroupSize) }
                : new OperationDto();

            dto.Id = operation.Id;
            dto.Operator = operation.Operator is null ? null : ToDto(operation.Operator);
            dto.Inputs = operation.Inputs.Select(ToDto).ToList();
            dto.Outputs = operation.Outputs.Select(ToDto).ToList();
            dto.Location = ToDto(operation.Location);
            dto.IsSelected = operation.IsSelected;
            dto.Size = ToDto(operation.Size);
            return dto;
        }

        private static OperationModel ToModel(OperationDto dto)
        {
            OperationModel operation = dto is OperationGroupDto groupDto
                ? new OperationGroupModel(dto.Id) { GroupSize = ToModel(groupDto.GroupSize), Operator = dto.Operator is null ? null : ToModel(dto.Operator) }
                : new OperationModel(dto.Id) { Operator = dto.Operator is null ? null : ToModel(dto.Operator) };

            foreach (var inputDto in dto.Inputs)
            {
                operation.Inputs.Add(ToModel(inputDto, operation));
            }

            foreach (var outputDto in dto.Outputs)
            {
                operation.Outputs.Add(ToModel(outputDto, operation));
            }

            operation.Location = ToModel(dto.Location);
            operation.IsSelected = dto.IsSelected;
            operation.Size = ToModel(dto.Size);
            return operation;
        }

        private static OperatorDto ToDto(IOperator @operator)
        {
            return new OperatorDto
            {
                Kind = ToolRegistry.GetToolName(@operator.GetType()),
                Json = JsonConvert.SerializeObject(@operator, JsonSettings)
            };
        }

        private static IOperator? ToModel(OperatorDto dto)
        {
            var type = ToolRegistry.GetToolType(dto.Kind);
            if (type is null)
                return null;

            return (IOperator?)JsonConvert.DeserializeObject(dto.Json, type, JsonSettings);
        }

        private static ConnectorDto ToDto(ConnectorModel connector)
        {
            return new ConnectorDto
            {
                Title = connector.Title,
                ValueTypeName = connector.ValueType.AssemblyQualifiedName ?? connector.ValueType.FullName ?? string.Empty,
                ValuePath = connector.ValuePath,
                IsInput = connector.IsInput,
                OwnerId = connector.OwnerId,
                IsConnected = connector.IsConnected
            };
        }

        private static ConnectorModel ToModel(ConnectorDto dto, OperationModel owner)
        {
            var valueType = Type.GetType(dto.ValueTypeName) ?? typeof(object);
            return new ConnectorModel(dto.Title, valueType, dto.ValuePath, dto.IsInput, dto.OwnerId, owner)
            {
                IsConnected = dto.IsConnected
            };
        }

        private static ConnectionDto ToDto(ConnectionModel connection)
        {
            return new ConnectionDto
            {
                Input = connection.Input is null ? null : ToDto(connection.Input),
                Output = connection.Output is null ? null : ToDto(connection.Output)
            };
        }

        private static PointDto ToDto(Point point) => new PointDto { X = point.X, Y = point.Y };

        private static Point ToModel(PointDto dto) => new Point(dto.X, dto.Y);

        private static SizeDto ToDto(Size size) => new SizeDto { Width = size.Width, Height = size.Height };

        private static Size ToModel(SizeDto dto) => new Size(dto.Width, dto.Height);

        private static JsonSerializerSettings CreateJsonSettings()
        {
            return new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                Converters = { new OperatorJsonConverter() }
            };
        }
    }
}
