using System;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.ViewModel.Tooling
{
    /// <summary>
    /// 根据算子实例创建对应 ViewModel 的工厂实现。
    /// </summary>
    public sealed class OperatorViewModelFactory : IOperatorViewModelFactory
    {
        private readonly IReadOnlyDictionary<Type, IOperatorViewModelRegistration> registrations;

        public OperatorViewModelFactory(IEnumerable<IOperatorViewModelRegistration> registrations)
        {
            ArgumentNullException.ThrowIfNull(registrations);
            this.registrations = registrations.ToDictionary(item => item.OperatorType);
        }

        /// <summary>
        /// 为指定算子创建对应的 ViewModel。
        /// </summary>
        public object Create(IOperator @operator)
        {
            ArgumentNullException.ThrowIfNull(@operator);
            return registrations.TryGetValue(@operator.GetType(), out var registration)
                ? registration.Create(@operator)
                : throw new ArgumentException(
                    $"No ViewModel is registered for operator type '{@operator.GetType()}'.",
                    nameof(@operator));
        }
    }
}
