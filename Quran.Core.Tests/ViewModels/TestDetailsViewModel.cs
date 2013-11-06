// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TestDetailsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Quran.Core.Tests.ViewModels
{
    using Core.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Defines the TestDetailsViewModel type.
    /// </summary>
    [TestFixture]
    public class TestDetailsViewModel : BaseTest
    {
        /// <summary>
        /// The first view model.
        /// </summary>
        private DetailsViewModel detailsViewModel;

        /// <summary>
        /// Creates an instance of the object to test.
        /// To allow Ninja automatically create the unit tests
        /// this method should not be changed.
        /// </summary>
        public override void CreateTestableObject()
        {
            this.detailsViewModel = new DetailsViewModel();
        }
    }
}
