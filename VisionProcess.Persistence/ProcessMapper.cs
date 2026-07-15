using OpenCvSharp;
using System.Collections.ObjectModel;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Helpers;
using VisionProcess.Common.Primitives;
using VisionProcess.Persistence.Operators;
using VisionProcess.Runtime.Models;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 在运行时模型 <see cref="ProcessModel"/> 与持久化 DTO <see cref="ProcessDto"/> 之间双向映射。
    /// </summary>
    public sealed class ProcessMapper
    {
        private readonly IOperatorCatalog operatorCatalog;

        public ProcessMapper(IOperatorCatalog operatorCatalog)
        {
            this.operatorCatalog = operatorCatalog
                ?? throw new ArgumentNullException(nameof(operatorCatalog));
        }

        public ProcessDto ToDto(ProcessModel process)
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

        public ProcessModel ToModel(ProcessDto dto)
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

        private OperationDto ToDto(OperationModel operation)
        {
            OperationDto dto = operation is OperationGroupModel group
                ? new OperationGroupDto { GroupSize = ToDto(group.GroupSize) }
                : new OperationDto();

            dto.Id = operation.Id;
            dto.Operator = null;
            dto.OperatorState = operation.Operator is null ? null : ToStateDto(operation.Operator);
            dto.Inputs = operation.Inputs.Select(ToDto).ToList();
            dto.Outputs = operation.Outputs.Select(ToDto).ToList();
            dto.Location = ToDto(operation.Location);
            dto.IsSelected = operation.IsSelected;
            dto.Size = ToDto(operation.Size);
            dto.Records = ToRecordDtos(operation.Operator?.Records);
            return dto;
        }

        private OperationModel ToModel(OperationDto dto)
        {
            var @operator = dto.OperatorState is not null
                ? ToModel(dto.OperatorState)
                : dto.Operator is null ? null : ToModel(dto.Operator);
            OperationModel operation = dto is OperationGroupDto groupDto
                ? new OperationGroupModel(dto.Id) { GroupSize = ToModel(groupDto.GroupSize), Operator = @operator }
                : new OperationModel(dto.Id) { Operator = @operator };

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
            RestoreRecords(operation.Operator?.Records, dto.Records);
            return operation;
        }

        private static List<RecordDto> ToRecordDtos(ObservableCollection<Record>? records)
        {
            if (records is null)
                return new List<RecordDto>();

            return records.Select(r => new RecordDto
            {
                Title = r.Title,
                ImageData = r.DisplayImage is Mat mat ? EncodeImage(mat) : null
            }).ToList();
        }

        private static byte[]? EncodeImage(Mat mat)
        {
            try
            {
                Cv2.ImEncode(".png", mat, out byte[] buffer);
                return buffer;
            }
            catch
            {
                // 编码失败（例如不支持的格式）时放弃保存该图像。
                return null;
            }
        }

        private static void RestoreRecords(ObservableCollection<Record>? records, List<RecordDto>? recordDtos)
        {
            if (records is null || recordDtos is null || recordDtos.Count == 0)
                return;

            foreach (var (record, dto) in records.Zip(recordDtos))
            {
                if (dto.ImageData is not null && dto.ImageData.Length > 0)
                {
                    try
                    {
                        record.DisplayImage = Cv2.ImDecode(dto.ImageData, ImreadModes.Unchanged);
                    }
                    catch
                    {
                        // 解码失败时保持为空。
                    }
                }
            }
        }

        private OperatorStateDto ToStateDto(IOperator @operator)
        {
            if (!operatorCatalog.TryGet(@operator.GetType(), out var descriptor))
            {
                throw new NotSupportedException(
                    $"Operator type '{@operator.GetType()}' is not registered in the module catalog.");
            }

            return new OperatorStateDto
            {
                OperatorId = descriptor.Id,
                StateVersion = descriptor.StateVersion,
                Payload = descriptor.SerializeState(@operator),
                Name = @operator.Name,
                RunStatus = ToDto(@operator.RunStatus)
            };
        }

        private IOperator ToModel(OperatorStateDto dto)
        {
            var descriptor = operatorCatalog.Get(dto.OperatorId);
            var @operator = descriptor.DeserializeState(dto.StateVersion, dto.Payload);
            RestoreCommonState(@operator, dto.Name, dto.RunStatus);
            return @operator;
        }

        private static IOperator? ToModel(OperatorDto dto)
        {
            var @operator = LegacyOperatorMapper.ToModel(dto);

            if (@operator is null)
                return null;

            RestoreCommonState(@operator, dto.Name, dto.RunStatus);
            return @operator;
        }

        private static RunStatusDto ToDto(RunStatus runStatus)
        {
            return new RunStatusDto
            {
                LastTime = runStatus.LastTime,
                Message = runStatus.Message,
                ProcessingTime = runStatus.ProcessingTime,
                Result = runStatus.Result
            };
        }

        private static RunStatus ToModel(RunStatusDto dto)
        {
            return new RunStatus
            {
                LastTime = dto.LastTime,
                Message = dto.Message,
                ProcessingTime = dto.ProcessingTime,
                Result = dto.Result
            };
        }

        private static void RestoreCommonState(
            IOperator @operator,
            string? name,
            RunStatusDto runStatusDto)
        {
            @operator.Name = name;
            var runStatus = @operator.RunStatus;
            runStatus.LastTime = runStatusDto.LastTime;
            runStatus.Message = runStatusDto.Message;
            runStatus.ProcessingTime = runStatusDto.ProcessingTime;
            runStatus.Result = runStatusDto.Result;
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

        private static PointDto ToDto(VpPoint point) => new PointDto { X = point.X, Y = point.Y };

        private static VpPoint ToModel(PointDto dto) => new VpPoint(dto.X, dto.Y);

        private static SizeDto ToDto(VpSize size) => new SizeDto { Width = size.Width, Height = size.Height };

        private static VpSize ToModel(SizeDto dto) => new VpSize(dto.Width, dto.Height);
    }
}
