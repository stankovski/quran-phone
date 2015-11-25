// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the BaseTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Quran.Core.Tests.Mocks;

namespace Quran.Core.Tests
{
    /// <summary>
    /// Defines the BaseTest type.
    /// </summary>
    public abstract class BaseTest : IDisposable
    {
        protected BaseTest()
        {
            SetUp();
        }
        /// <summary>
        /// The mock dispatcher.
        /// </summary>
        private MockDispatcher mockDispatcher;

        /// <summary>
        /// Sets up.
        /// </summary>
        public virtual void SetUp()
        {
            this.Initialize();
            this.CreateTestableObject();
        }

        /// <summary>
        /// Initializes this instance.
        /// Any specific setup code for derived classes should override this method.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Terminates this instance.
        /// Any specific termination code for derived classes should override this method.
        /// </summary>
        public virtual void Terminate()
        {
        }

        /// <summary>
        /// Creates the testable object.
        /// </summary>
        public virtual void CreateTestableObject()
        {
        }

        /// <summary>
        /// Tears down.
        /// </summary>
        public virtual void TearDown()
        {
            this.Terminate();
        }

        public void Dispose()
        {
            TearDown();
        }
    }
}
