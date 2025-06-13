using System;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Registration;
using Jobbr.Server.ForkedExecution.BackChannel;
using Jobbr.Server.ForkedExecution.Execution;

namespace Jobbr.Server.ForkedExecution
{
    /// <summary>
    /// Extensions for <see cref="IJobbrBuilder"/>.
    /// </summary>
    public static class JobbrBuilderExtensions
    {
        /// <summary>
        /// Configure forked execution.
        /// </summary>
        /// <param name="builder">Builder where the configuration is placed.</param>
        public static void AddForkedExecution(this IJobbrBuilder builder)
        {
            AddForkedExecution(builder, _ => { });
        }

        /// <summary>
        /// Configure forked execution based on specific configuration.
        /// </summary>
        /// <param name="builder">Builder where the configuration is placed.</param>
        /// <param name="config">Forked execution configuration.</param>
        public static void AddForkedExecution(this IJobbrBuilder builder, Action<ForkedExecutionConfiguration> config)
        {
            var defaultConfig = new ForkedExecutionConfiguration();

            config(defaultConfig);

            builder.Add<IDateTimeProvider>(new UtcNowTimeProvider());
            builder.Add<IPeriodicTimer>(new FixedIntervalTimer());
            builder.Add<ForkedExecutionConfiguration>(defaultConfig);

            builder.Register<IJobExecutor>(typeof(ForkedJobExecutor));
            builder.Register<IJobRunContextFactory>(typeof(JobRunContextFactory));

            builder.RegisterForCollection<IJobbrComponent>(typeof(BackChannelWebHost));
            builder.RegisterForCollection<IConfigurationValidator>(typeof(ConfigurationValidator));
        }
    }
}
