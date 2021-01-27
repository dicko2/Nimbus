using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Nimbus.Tests.Common.Stubs
{
#if NETCOREAPP2_0
    /// <summary>
    /// Applies a timeout in milliseconds to a test. 
    /// When applied to a method, the test is cancelled if the timeout is exceeded. 
    /// When applied to a class or assembly, the default timeout is set for all contained test methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false,
        Inherited = false)]
    public class TimeoutAttribute : PropertyAttribute, IApplyToContext
    {
        private readonly int _timeout;

        /// <summary>
        /// Construct a TimeoutAttribute given a time in milliseconds
        /// </summary>
        /// <param name="timeout">The timeout value in milliseconds</param>
        public TimeoutAttribute(int timeout)
            : base(timeout)
        {
            _timeout = timeout;
        }


        void IApplyToContext.ApplyToContext(TestExecutionContext context)
        {
            context.TestCaseTimeout = _timeout;
        }

    }
#endif
}
