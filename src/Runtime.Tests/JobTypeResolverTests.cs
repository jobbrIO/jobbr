using System.Collections.Generic;
using System.Reflection;
using Jobbr.Runtime.Activation;
using Jobbr.SampleTaskLibrary;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Runtime.Tests
{
    [TestClass]
    public class JobTypeResolverTests
    {
        private class JobInExecutingAssembly
        {
        }

        [TestMethod]
        public void TypeFullName_SearchAssemblySet_TypeFound()
        {
            var jobType = typeof(JobInExecutingAssembly).FullName;
            var anotherJobType = typeof(AnotherJobTask);
            var jobTypeSearchAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly(), anotherJobType.Assembly };
            var jobTypeResolver = new JobTypeResolver(NullLoggerFactory.Instance, jobTypeSearchAssemblies);

            var result = jobTypeResolver.ResolveType(jobType);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(JobInExecutingAssembly), result);

            result = jobTypeResolver.ResolveType(anotherJobType.FullName);

            Assert.IsNotNull(result);
            Assert.AreEqual(anotherJobType, result);
        }

        [TestMethod]
        public void AssemblyFullyQualifiedName_NoSearchAssemblySet_TypeFound()
        {
            var jobType = typeof(JobInExecutingAssembly).AssemblyQualifiedName;

            var jobTypeResolver = new JobTypeResolver(NullLoggerFactory.Instance, null);

            var result = jobTypeResolver.ResolveType(jobType);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(JobInExecutingAssembly), result);
        }
    }
}
