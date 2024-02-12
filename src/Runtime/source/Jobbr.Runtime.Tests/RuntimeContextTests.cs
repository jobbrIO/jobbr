using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Runtime.Tests
{
    [TestClass]
    public class RuntimeContextTests
    {
        private class CustomActivator : IConfigurableServiceProvider
        {
            public List<object> RegisteredInstances { get; } = new List<object>();

            public object GetService(Type serviceType)
            {
                return null;
            }

            public void RegisterInstance<T>(T instance)
            {
                RegisteredInstances.Add(instance);
            }
        }

        private class TestJob
        {
        }

        private class RunCallBackTestJob
        {
            private static Action _callback;

            private static readonly object CallBackLock = new List<object>();

            public static Action Callback
            {
                set
                {
                    lock (CallBackLock)
                    {
                        if (_callback != null)
                        {
                            Assert.Fail($"Cannot use {nameof(RunCallBackTestJob)} in more than one test simultaneously.");
                        }

                        _callback = value;
                    }
                }
            }

            public static void Reset()
            {
                lock (CallBackLock)
                {
                    _callback = null;
                }
            }

            public void Run()
            {
                lock (CallBackLock)
                {
                    _callback();
                }
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.SetPrincipalPolicy(PrincipalPolicy.UnauthenticatedPrincipal);
        }

        [TestMethod]
        public void ConfigurableServiceProviderIsSet_WhenExecuting_RegistrationIsCalled()
        {
            var serviceProvider = new CustomActivator();

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration { ServiceProvider = serviceProvider });
            runtime.Execute(new ExecutionMetadata { JobType = typeof(TestJob).AssemblyQualifiedName });

            #pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(1, serviceProvider.RegisteredInstances.OfType<RuntimeContext>().Count(), "There should be a single registration of the RuntimeContext");
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void ConfigurableServiceProviderIsSet_WhenExecuting_ContextMatchesMetaInfo()
        {
            var serviceProvider = new CustomActivator();

            const string userName = "michael.schnyder@zuehlke.com";
            const string userDisplay = "Schnyder, Michael";

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration { ServiceProvider = serviceProvider });
            runtime.Execute(new ExecutionMetadata { JobType = typeof(TestJob).AssemblyQualifiedName, UserId = userName, UserDisplayName = userDisplay });

            #pragma warning disable CS0618 // Type or member is obsolete
            var ctx = serviceProvider.RegisteredInstances.OfType<RuntimeContext>().Single();

            Assert.AreEqual(userName, ctx.UserId);
            Assert.AreEqual(userDisplay, ctx.UserDisplayName);
            #pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void CallingThreadPrincipal_WhenUserIsNotSet_DoesNotChange()
        {
            var currentThreadPrincipal = Thread.CurrentPrincipal;

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(TestJob).AssemblyQualifiedName });

            Assert.AreEqual(currentThreadPrincipal, Thread.CurrentPrincipal);
        }

        [TestMethod]
        public void CallingThreadPrincipal_WhenUserIsSet_DoesNotChange()
        {
            var currentThreadPrincipal = Thread.CurrentPrincipal;

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(TestJob).AssemblyQualifiedName, UserId = "bla"});

            Assert.AreEqual(currentThreadPrincipal, Thread.CurrentPrincipal);
        }

        [TestMethod]
        public void ExecutingThreadPrincipal_WhenExecuting_CallbackIsCalled()
        {
            IPrincipal executingThreadPrincipal = null;

            RunCallBackTestJob.Callback = () => executingThreadPrincipal = Thread.CurrentPrincipal;

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(RunCallBackTestJob).AssemblyQualifiedName });

            RunCallBackTestJob.Reset();

            Assert.IsNotNull(executingThreadPrincipal);
        }

        [TestMethod]
        public void ExecutingThreadPrincipal_WhenUserIsNotSet_DoesNotChange()
        {
            IPrincipal executingThreadPrincipal = null;

            RunCallBackTestJob.Callback = () => executingThreadPrincipal = Thread.CurrentPrincipal;

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(RunCallBackTestJob).AssemblyQualifiedName });

            RunCallBackTestJob.Reset();

            Assert.IsNotNull(executingThreadPrincipal);
            Assert.IsNotNull(executingThreadPrincipal.Identity);
            Assert.IsNotNull(Thread.CurrentPrincipal);
            Assert.IsNotNull(Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(executingThreadPrincipal.Identity.Name, Thread.CurrentPrincipal.Identity.Name);
        }

        [TestMethod]
        public void ExecutingThreadPrincipal_WhenUserIsSet_DoesChangePrincipal()
        {
            IPrincipal executingThreadPrincipal = null;

            RunCallBackTestJob.Callback = () => executingThreadPrincipal = Thread.CurrentPrincipal;

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(RunCallBackTestJob).AssemblyQualifiedName, UserId = "anything"});

            RunCallBackTestJob.Reset();
            
            Assert.IsNotNull(executingThreadPrincipal);
            Assert.IsNotNull(executingThreadPrincipal.Identity);
            Assert.IsNotNull(Thread.CurrentPrincipal);
            Assert.IsNotNull(Thread.CurrentPrincipal.Identity);
            Assert.AreNotEqual(executingThreadPrincipal.Identity.Name, Thread.CurrentPrincipal.Identity.Name);
        }

        [TestMethod]
        public void ThreadPrincipal_InExecutingThread_IdentityNameContainsUserId()
        {
            IPrincipal executingThreadPrincipal = null;

            RunCallBackTestJob.Callback = () => executingThreadPrincipal = Thread.CurrentPrincipal;
            const string userName = "michael.schnyder@zuehlke.com";

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(RunCallBackTestJob).AssemblyQualifiedName, UserId = userName});

            RunCallBackTestJob.Reset();

            Assert.IsNotNull(executingThreadPrincipal);
            Assert.IsNotNull(executingThreadPrincipal.Identity);
            Assert.AreEqual(userName, executingThreadPrincipal.Identity.Name);
        }

        [TestMethod]
        public void ThreadPrincipal_InExecutingThread_AuthenticationTypeIsSet()
        {
            IPrincipal executingThreadPrincipal = null;

            RunCallBackTestJob.Callback = () => executingThreadPrincipal = Thread.CurrentPrincipal;
            const string userName = "michael.schnyder@zuehlke.com";

            var runtime = new CoreRuntime(NullLoggerFactory.Instance, new RuntimeConfiguration());
            runtime.Execute(new ExecutionMetadata { JobType = typeof(RunCallBackTestJob).AssemblyQualifiedName, UserId = userName });

            RunCallBackTestJob.Reset();

            Assert.IsNotNull(executingThreadPrincipal);
            Assert.IsNotNull(executingThreadPrincipal.Identity);
            Assert.AreEqual("JobbrIdentity", executingThreadPrincipal.Identity.AuthenticationType);
        }
    }
}
