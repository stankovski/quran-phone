// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the MockDispatcher type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Quran.Core.Tests.Mocks
{
    using System;
    using System.Collections.Generic;

    using Cirrious.CrossCore.Core;
    using Cirrious.MvvmCross.ViewModels;
    using Cirrious.MvvmCross.Views;

    /// <summary>
    /// Defines the MockDispatcher type.
    /// </summary>
    public class MockDispatcher
        : MvxMainThreadDispatcher, IMvxViewDispatcher
    {
        /// <summary>
        /// The requests
        /// </summary>
        public readonly List<MvxViewModelRequest> Requests = new List<MvxViewModelRequest>();

        /// <summary>
        /// Requests the main thread action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>returns true.</returns>
        public bool RequestMainThreadAction(Action action)
        {
            action();
            return true;
        }

        /// <summary>
        /// Shows the view model.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>return true.</returns>
        public bool ShowViewModel(MvxViewModelRequest request)
        {
            this.Requests.Add(request);
            return true;
        }

        /// <summary>
        /// Changes the presentation.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <returns>an exception.</returns>
        public bool ChangePresentation(MvxPresentationHint hint)
        {
            throw new NotImplementedException();
        }
    }
}
