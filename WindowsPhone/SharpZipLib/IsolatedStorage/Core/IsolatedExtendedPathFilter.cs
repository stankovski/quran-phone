using System;
using System.IO;
using QuranPhone.SharpZipLib.Core;
using QuranPhone.SharpZipLib.IsolatedStorage.Core;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Core
{
    /// <summary>
    ///   ExtendedPathFilter filters based on name, file size, and the last write time of the file.
    /// </summary>
    /// <remarks>
    ///   Provides an example of how to customise filtering.
    /// </remarks>
    public class IsolatedExtendedPathFilter : IsolatedPathFilter
    {
        #region Fields

        private DateTime maxDate_ = DateTime.MaxValue;

        private long maxSize_ = long.MaxValue;

        private DateTime minDate_ = DateTime.MinValue;

        private long minSize_;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initialise a new instance of ExtendedPathFilter.
        /// </summary>
        /// <param name = "filter">The filter to apply.</param>
        /// <param name = "minDate">The minimum <see cref = "DateTime" /> to include.</param>
        /// <param name = "maxDate">The maximum <see cref = "DateTime" /> to include.</param>
        public IsolatedExtendedPathFilter(string filter, DateTime minDate, DateTime maxDate)
            : base(filter)
        {
            MinDate = minDate;
            MaxDate = maxDate;
        }

        /// <summary>
        ///   Initialise a new instance of ExtendedPathFilter.
        /// </summary>
        /// <param name = "filter">The filter to apply.</param>
        /// <param name = "minSize">The minimum file size to include.</param>
        /// <param name = "maxSize">The maximum file size to include.</param>
        public IsolatedExtendedPathFilter(string filter, long minSize, long maxSize)
            : base(filter)
        {
            MinSize = minSize;
            MaxSize = maxSize;
        }

        /// <summary>
        ///   Initialise a new instance of ExtendedPathFilter.
        /// </summary>
        /// <param name = "filter">The filter to apply.</param>
        /// <param name = "minSize">The minimum file size to include.</param>
        /// <param name = "maxSize">The maximum file size to include.</param>
        /// <param name = "minDate">The minimum <see cref = "DateTime" /> to include.</param>
        /// <param name = "maxDate">The maximum <see cref = "DateTime" /> to include.</param>
        public IsolatedExtendedPathFilter(string filter, long minSize, long maxSize, DateTime minDate, DateTime maxDate)
            : base(filter)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            MinDate = minDate;
            MaxDate = maxDate;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Get/set the maximum <see cref = "DateTime" /> value that will match for this filter.
        /// </summary>
        /// <remarks>
        ///   Files with a LastWrite time greater than this value are excluded by the filter.
        /// </remarks>
        public DateTime MaxDate
        {
            get { return maxDate_; }

            set
            {
                if (minDate_ > value)
                {
                    throw new ArgumentOutOfRangeException("value", "Exceeds MinDate");
                }

                maxDate_ = value;
            }
        }

        /// <summary>
        ///   Get/set the maximum size/length for a file that will match this filter.
        /// </summary>
        /// <remarks>
        ///   The default value is <see cref = "System.Int64.MaxValue" />
        /// </remarks>
        /// <exception cref = "ArgumentOutOfRangeException">value is less than zero or less than <see cref = "MinSize" /></exception>
        public long MaxSize
        {
            get { return maxSize_; }
            set
            {
                if ((value < 0) || (minSize_ > value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                maxSize_ = value;
            }
        }

        /// <summary>
        ///   Get/set the minimum <see cref = "DateTime" /> value that will match for this filter.
        /// </summary>
        /// <remarks>
        ///   Files with a LastWrite time less than this value are excluded by the filter.
        /// </remarks>
        public DateTime MinDate
        {
            get { return minDate_; }

            set
            {
                if (value > maxDate_)
                {
                    throw new ArgumentOutOfRangeException("value", "Exceeds MaxDate");
                }

                minDate_ = value;
            }
        }

        /// <summary>
        ///   Get/set the minimum size/length for a file that will match this filter.
        /// </summary>
        /// <remarks>
        ///   The default value is zero.
        /// </remarks>
        /// <exception cref = "ArgumentOutOfRangeException">value is less than zero; greater than <see cref = "MaxSize" /></exception>
        public long MinSize
        {
            get { return minSize_; }
            set
            {
                if ((value < 0) || (maxSize_ < value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                minSize_ = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Test a filename to see if it matches the filter.
        /// </summary>
        /// <param name = "name">The filename to test.</param>
        /// <returns>True if the filter matches, false otherwise.</returns>
        /// <exception cref = "System.IO.FileNotFoundException">The <see paramref = "fileName" /> doesnt exist</exception>
        public override bool IsMatch(string name)
        {
            bool result = base.IsMatch(name);

            if (result)
            {
                var fileInfo = new FileInfo(name);
                result =
                    (MinSize <= fileInfo.Length) &&
                    (MaxSize >= fileInfo.Length) &&
                    (MinDate <= fileInfo.LastWriteTime) &&
                    (MaxDate >= fileInfo.LastWriteTime)
                    ;
            }
            return result;
        }

        #endregion

    }
}
