using QuranPhone.SharpZipLib.Core;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Core
{
    /// <summary>
    ///   PathFilter filters directories and files using a form of <see cref = "System.Text.RegularExpressions.Regex">regular expressions</see>
    ///   by full path name.
    ///   See <see cref = "NameFilter">NameFilter</see> for more detail on filtering.
    /// </summary>
    public class IsolatedPathFilter : IScanFilter
    {
        #region Fields

        private readonly NameFilter nameFilter_;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initialise a new instance of <see cref = "PathFilter"></see>.
        /// </summary>
        /// <param name = "filter">The <see cref = "NameFilter">filter</see> expression to apply.</param>
        public IsolatedPathFilter(string filter)
        {
            nameFilter_ = new NameFilter(filter);
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Test a name to see if it matches the filter.
        /// </summary>
        /// <param name = "name">The name to test.</param>
        /// <returns>True if the name matches, false otherwise.</returns>
        public virtual bool IsMatch(string name)
        {
            bool result = false;

            if (name != null)
            {
                result = nameFilter_.IsMatch(name);
            }
            return result;
        }

        #endregion

    }
}
