// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the BaseTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Quran.Core.Tests
{
    /// <summary>
    /// Defines the BaseTest type.
    /// </summary>
    public abstract class BaseTest
    {
        protected BaseTest()
        {
            QuranApp.NativeProvider = new TestNativeProvider();
        }        
    }
}
