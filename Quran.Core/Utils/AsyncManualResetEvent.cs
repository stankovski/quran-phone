using System.Threading;
using System.Threading.Tasks;

namespace Quran.Core.Utils
{
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() { return m_tcs.Task; }

        public bool IsComplete {
            get
            {
                var tcs = m_tcs;
                return tcs.Task.IsCompleted;
            }
        }

        public void Set() { m_tcs.TrySetResult(true); }

        public void Reset()
        {
            while (true)
            {
                var tcs = m_tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                {
                    return;
                }
            }
        }
    }
}
