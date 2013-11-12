// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the BaseTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.File;
using Cirrious.MvvmCross.Plugins.File.Wpf;
using Cirrious.MvvmCross.Plugins.ResourceLoader.Wpf;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Cirrious.MvvmCross.Plugins.Sqlite.Wpf;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;
using Quran.Core.Tests.Mocks;

namespace Quran.Core.Tests
{
    /// <summary>
    /// Defines the BaseTest type.
    /// </summary>
    public abstract class BaseTest : MvxIoCSupportingTest, IDisposable
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
            this.ClearAll();

            this.mockDispatcher = new MockDispatcher();

            Ioc.RegisterSingleton<IMvxViewDispatcher>(this.mockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(this.mockDispatcher);
            Ioc.RegisterSingleton<IMvxTrace>(new TestTrace());
            Ioc.RegisterSingleton<IMvxSettings>(new MvxSettings());
            Ioc.RegisterSingleton<IMvxResourceLoader>(new MvxWpfResourceLoader());
            Ioc.RegisterSingleton<IMvxFileStore>(new MvxWpfFileStore());
            Ioc.RegisterSingleton<ISQLiteConnectionFactory>(new MvxWpfSqLiteConnectionFactory());
            System.Windows.Application.ResourceAssembly = Assembly.GetAssembly(typeof (BaseTest));

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
