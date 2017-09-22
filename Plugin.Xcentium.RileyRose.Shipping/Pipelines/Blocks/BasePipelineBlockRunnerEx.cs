using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines.Definitions;
 
using Sitecore.Framework.Pipelines;

namespace Plugin.Xcentium.RileyRose.Shipping.Pipelines.Blocks
{
    public abstract class BasePipelineBlockRunnerEx : IPipelineBlockRunner
    {
        protected readonly IBuildablePipelineBlockDefinition[] _definitions;
        protected readonly IServiceProvider _services;

        protected BasePipelineBlockRunnerEx(IServiceProvider services, IEnumerable<IBuildablePipelineBlockDefinition> definitions)
        {
            Condition.Requires<IEnumerable<IBuildablePipelineBlockDefinition>>(definitions, "definitions").IsNotNull<IEnumerable<IBuildablePipelineBlockDefinition>>();
            Condition.Requires<IServiceProvider>(services, "services").IsNotNull<IServiceProvider>();
            this._definitions = definitions.ToArray<IBuildablePipelineBlockDefinition>();
            this._services = services;
        }

        public virtual async Task<TOutput> Run<TOutput>(string name, object input, IPipelineExecutionContext context)
        {
            await Task.Yield();
            Condition.Requires<IPipelineExecutionContext>(context, "context").IsNotNull<IPipelineExecutionContext>();
            object current = input;
            Guid guid = Guid.NewGuid();
            using (context.Logger.BeginScope("{ExecutionId}", (object)guid))
            {
                try
                {
                    context.Logger.LogDebug("[PipelineStarted]");
                    context.Status.Report("[PipelineStarted]");
                    if (!((IEnumerable<IBuildablePipelineBlockDefinition>)this._definitions).Any<IBuildablePipelineBlockDefinition>())
                    {
                        context.Status.Report("Pipeline contains no runnables");
                        context.Logger.LogDebug("Pipeline contains no runnables");
                        current = input;
                    }
                    else
                    {
                        using (context.TraceContext.CaptureTiming("Pipeline: " + name, "Run", "C:\\builds\\10\\s\\src\\Sitecore.Framework.Pipelines.Abstractions\\BasePipelineBlockRunner.cs"))
                        {
                            foreach (IBuildablePipelineBlockDefinition definition in ((IEnumerable<IBuildablePipelineBlockDefinition>)this._definitions).TakeWhile<IBuildablePipelineBlockDefinition>((Func<IBuildablePipelineBlockDefinition, bool>)(_ => !context.IsAborted)))
                            {
                                using (context.TraceContext.CaptureTiming(definition.Name, "Run", "C:\\builds\\10\\s\\src\\Sitecore.Framework.Pipelines.Abstractions\\BasePipelineBlockRunner.cs"))
                                {
                                    IPipelineBlock block = this.BuildBlock(definition);
                                    using (context.Logger.BeginScope("{BlockName} {BlockType}", (object)block.Name, (object)block.GetType().FullName))
                                    {
                                        context.Logger.LogDebug("Block execution started for '{BlockName}'", (object)block.Name);
                                        context.Status.Report(string.Format("[BlockStarted]: {0}", (object)block.Name));
                                        current = await this.InvokeBlock(block, context, current);
                                        context.Logger.LogDebug("Block execution completed for '{BlockName}'", (object)block.Name);
                                        context.Status.Report(string.Format("[BlockCompleted]: {0}", (object)block.Name));
                                        context.LastBlockResult.Report(current);
                                    }
                                    block = (IPipelineBlock)null;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(new EventId(), ex, "Pipeline completed with error", new object[0]);
                    context.Errors.Report(ex);
                }
                finally
                {
                    context.Logger.LogDebug("[PipelineCompleted]");
                    context.Status.Report("[PipelineCompleted]");
                    if (context.IsTraceEnabled)
                        context.CreateTraceReport();
                }
            }
            return current == null || !(current is TOutput) ? default(TOutput) : (TOutput)current;
        }

        protected virtual IPipelineBlock BuildBlock(IBuildablePipelineBlockDefinition definition)
        {
            return definition.Build(this._services);
        }

        protected abstract Task<object> InvokeBlock(IPipelineBlock block, IPipelineExecutionContext context, object current);
    }
}
