using System;
using System.Globalization;
using Quran.Core.Utils;

namespace Quran.Core.Common
{
    public class RepeatInfo
    {
        public RepeatAmount RepeatAmount { get; set; }

        public int RepeatCount { get; set; }

        /// <summary>
        /// Parses repeat string in the format:
        /// [RepeatAmount]-[num]-times
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static RepeatInfo FromString(string pattern)
        {
            var repeatInfo = new RepeatInfo {RepeatAmount = RepeatAmount.None, RepeatCount = 0};
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                var splitPattern = pattern.Split('-');
                if (splitPattern.Length == 3)
                {
                    try
                    {
                        repeatInfo.RepeatAmount = (RepeatAmount) Enum.Parse(typeof (RepeatAmount), splitPattern[0]);
                        if (splitPattern[1] == "infinite")
                        {
                            repeatInfo.RepeatCount = int.MaxValue;
                        }
                        else
                        {
                            repeatInfo.RepeatCount = int.Parse(splitPattern[1]);
                        }
                        
                        if (repeatInfo.RepeatAmount == RepeatAmount.None)
                        {
                            repeatInfo.RepeatCount = 0;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            return repeatInfo;
        }

        /// <summary>
        /// Returns string in the following pattern [RepeatAmount]-[num]-times
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string times;
            if (RepeatCount != int.MaxValue)
            {
                times = RepeatCount.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                times = "infinite";
            }
            return string.Format("{0}-{1}-times", RepeatAmount, times);
        }
    }
}
