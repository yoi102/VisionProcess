using OpenCvSharp;
using System.Collections.ObjectModel;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Helpers;
using VisionProcess.Common.Primitives;
using VisionProcess.Persistence.Operators;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 在运行时模型 <see cref="ProcessModel"/> 与持久化 DTO <see cref="ProcessDto"/> 之间双向映射。
    /// </summary>
    public static class ProcessMapper
    {
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
            dto.Records = ToRecordDtos(operation.Operator?.Records);
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

        private static OperatorDto ToDto(IOperator @operator)
        {
            OperatorDto dto = @operator switch
            {
                AcquireImageOperator op => ToDto(op),
                ColorConvertOperator op => ToDto(op),
                ImageFilterOperator op => ToDto(op),
                ImageThresholdOperator op => ToDto(op),
                _ => throw new NotSupportedException($"Operator type '{@operator.GetType()}' is not supported for persistence.")
            };

            dto.Name = @operator.Name;
            dto.RunStatus = ToDto(@operator.RunStatus);
            return dto;
        }

        private static IOperator? ToModel(OperatorDto dto)
        {
            IOperator? @operator = dto switch
            {
                AcquireImageOperatorDto acquireDto => ToModel(acquireDto),
                ColorConvertOperatorDto colorDto => ToModel(colorDto),
                ImageFilterOperatorDto filterDto => ToModel(filterDto),
                ImageThresholdOperatorDto thresholdDto => ToModel(thresholdDto),
                _ => null
            };

            if (@operator is null)
                return null;

            @operator.Name = dto.Name;
            var runStatus = @operator.RunStatus;
            runStatus.LastTime = dto.RunStatus.LastTime;
            runStatus.Message = dto.RunStatus.Message;
            runStatus.ProcessingTime = dto.RunStatus.ProcessingTime;
            runStatus.Result = dto.RunStatus.Result;
            return @operator;
        }

        private static AcquireImageOperatorDto ToDto(AcquireImageOperator @operator)
        {
            return new AcquireImageOperatorDto
            {
                CurrentIndex = @operator.CurrentIndex,
                ImagePaths = @operator.ImagePaths ?? new List<string>()
            };
        }

        private static AcquireImageOperator ToModel(AcquireImageOperatorDto dto)
        {
            return new AcquireImageOperator
            {
                CurrentIndex = dto.CurrentIndex,
                ImagePaths = dto.ImagePaths
            };
        }

        private static ColorConvertOperatorDto ToDto(ColorConvertOperator @operator)
        {
            return new ColorConvertOperatorDto
            {
                ColorConversionCodes = @operator.Inputs.ColorConversionCodes
            };
        }

        private static ColorConvertOperator ToModel(ColorConvertOperatorDto dto)
        {
            return new ColorConvertOperator
            {
                Inputs = { ColorConversionCodes = dto.ColorConversionCodes }
            };
        }

        private static ImageFilterOperatorDto ToDto(ImageFilterOperator @operator)
        {
            return new ImageFilterOperatorDto
            {
                BorderType = @operator.Inputs.BorderType,
                FilterType = @operator.Inputs.FilterType,
                KernelHeight = @operator.Inputs.KernelHeight,
                KernelWidth = @operator.Inputs.KernelWidth,
                KernelSize = @operator.Inputs.KernelSize,
                SigmaX = @operator.Inputs.SigmaX,
                SigmaY = @operator.Inputs.SigmaY,
                Diameter = @operator.Inputs.Diameter,
                SigmaColor = @operator.Inputs.SigmaColor,
                SigmaSpace = @operator.Inputs.SigmaSpace
            };
        }

        private static ImageFilterOperator ToModel(ImageFilterOperatorDto dto)
        {
            return new ImageFilterOperator
            {
                Inputs =
                {
                    BorderType = dto.BorderType,
                    FilterType = dto.FilterType,
                    KernelHeight = dto.KernelHeight,
                    KernelWidth = dto.KernelWidth,
                    KernelSize = dto.KernelSize,
                    SigmaX = dto.SigmaX,
                    SigmaY = dto.SigmaY,
                    Diameter = dto.Diameter,
                    SigmaColor = dto.SigmaColor,
                    SigmaSpace = dto.SigmaSpace
                }
            };
        }

        private static ImageThresholdOperatorDto ToDto(ImageThresholdOperator @operator)
        {
            return new ImageThresholdOperatorDto
            {
                MaximumValue = @operator.Inputs.MaximumValue,
                ThresholdType = @operator.Inputs.ThresholdType,
                ThresholdValue = @operator.Inputs.ThresholdValue
            };
        }

        private static ImageThresholdOperator ToModel(ImageThresholdOperatorDto dto)
        {
            return new ImageThresholdOperator
            {
                Inputs =
                {
                    MaximumValue = dto.MaximumValue,
                    ThresholdType = dto.ThresholdType,
                    ThresholdValue = dto.ThresholdValue
                }
            };
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
