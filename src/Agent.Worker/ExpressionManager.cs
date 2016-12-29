using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Agent.Util;
using DT = Microsoft.VisualStudio.Services.DistributedTask.Expressions;

namespace Microsoft.VisualStudio.Services.Agent.Worker
{
    [ServiceLocator(Default = typeof(ExpressionManager))]
    public interface IExpressionManager : IAgentService
    {
        bool Evaluate(IExecutionContext context, string condition);
    }

    public sealed class ExpressionManager : AgentService, IExpressionManager
    {
        public bool Evaluate(IExecutionContext executionContext, string condition)
        {
            ArgUtil.NotNull(executionContext, nameof(executionContext));

            // Parse the condition.
            var expressionTrace = new TraceWriter(executionContext);
            var parser = new DT.Parser();
            var extensions = new DT.IExtensionInfo[]
            {
                new DT.ExtensionInfo<AlwaysNode>(name: Constants.Expressions.Always, minParameters: 0, maxParameters: 0),
                new DT.ExtensionInfo<SucceededNode>(name: Constants.Expressions.Succeeded, minParameters: 0, maxParameters: 0),
                new DT.ExtensionInfo<SucceededOrFailedNode>(name: Constants.Expressions.SucceededOrFailed, minParameters: 0, maxParameters: 0),
                new DT.ExtensionInfo<VariablesNode>(name: Constants.Expressions.Variables, minParameters: 1, maxParameters: 1),
            };
            DT.Node tree = parser.CreateTree(condition, expressionTrace, extensions) ?? new SucceededNode();

            // Evaluate the tree.
            var evaluationContext = new DT.EvaluationContext(expressionTrace, state: executionContext);
            return tree.Evaluate(evaluationContext).ConvertToBoolean(evaluationContext);
        }

        public sealed class TraceWriter : DT.ITraceWriter
        {
            private readonly IExecutionContext _executionContext;

            public TraceWriter(IExecutionContext executionContext)
            {
                ArgUtil.NotNull(executionContext, nameof(executionContext));
                _executionContext = executionContext;
            }

            public void Info(string message)
            {
                _executionContext.Output(message);
            }

            public void Verbose(string message)
            {
                _executionContext.Debug(message);
            }
        }

        public sealed class AlwaysNode : DT.FunctionNode
        {
            protected sealed override object EvaluateCore(DT.EvaluationContext evaluationContext)
            {
                return true;
            }
        }

        public sealed class SucceededNode : DT.FunctionNode
        {
            protected sealed override object EvaluateCore(DT.EvaluationContext evaluationContext)
            {
                var executionContext = evaluationContext.State as IExecutionContext;
                ArgUtil.NotNull(executionContext, nameof(executionContext));
                TaskResult jobStatus = executionContext.Variables.Agent_JobStatus ?? TaskResult.Succeeded;
                return jobStatus == TaskResult.Succeeded ||
                    jobStatus == TaskResult.SucceededWithIssues;
            }
        }

        public sealed class SucceededOrFailedNode : DT.FunctionNode
        {
            protected sealed override object EvaluateCore(DT.EvaluationContext evaluationContext)
            {
                var executionContext = evaluationContext.State as IExecutionContext;
                ArgUtil.NotNull(executionContext, nameof(executionContext));
                TaskResult jobStatus = executionContext.Variables.Agent_JobStatus ?? TaskResult.Succeeded;
                return jobStatus == TaskResult.Succeeded ||
                    jobStatus == TaskResult.SucceededWithIssues ||
                    jobStatus == TaskResult.Failed;
            }
        }

        public sealed class VariablesNode : DT.FunctionNode
        {
            protected sealed override object EvaluateCore(DT.EvaluationContext evaluationContext)
            {
                var executionContext = evaluationContext.State as IExecutionContext;
                ArgUtil.NotNull(executionContext, nameof(executionContext));
                string variableName = Parameters[0].Evaluate(evaluationContext).ConvertToString(evaluationContext);
                return executionContext.Variables.Get(variableName);
            }
        }
    }
}