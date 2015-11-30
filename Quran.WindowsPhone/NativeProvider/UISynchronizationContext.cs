using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Quran.WindowsPhone.NativeProvider
{
    /// <summary>
    /// Singleton class providing the default implementation
    /// </summary>
    public class UISynchronizationContext
{
    private DispatcherSynchronizationContext _context;
    private Dispatcher _dispatcher;
    
    #region Singleton implementation

    static readonly UISynchronizationContext instance = new UISynchronizationContext();
    
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static UISynchronizationContext Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    public void Initialize()
    {
        EnsureInitialized();
    }

    readonly object initializationLock = new object();

    void EnsureInitialized()
    {
        if (_dispatcher != null && _context != null)
        {
            return;
        }

        lock (initializationLock)
        {
            if (_dispatcher != null && _context != null)
            {
                return;
            }

            try
            {
                _dispatcher = Deployment.Current.Dispatcher;
                _context = new DispatcherSynchronizationContext(_dispatcher);
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Initialised called from non-UI thread."); 
            }
        }
    }

    public void Initialize(Dispatcher dispatcher)
    {
        if (dispatcher == null)
            throw new ArgumentNullException("dispatcher");

        lock (initializationLock)
        {
            this._dispatcher = dispatcher;
            _context = new DispatcherSynchronizationContext(dispatcher);
        }
    }

    public void InvokeAsynchronously(SendOrPostCallback callback, object state)
    {
        if (callback == null)
            throw new ArgumentNullException("callback");

        EnsureInitialized();

        _context.Post(callback, state);
    }

    public void InvokeAsynchronously(Action action)
    {
        if (action == null)
            throw new ArgumentNullException("action");

        EnsureInitialized();

        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.BeginInvoke(action);
        }
    }

    public void InvokeSynchronously(SendOrPostCallback callback, object state)
    {
        if (callback == null)
            throw new ArgumentNullException("callback");

        EnsureInitialized();

        _context.Send(callback, state);
    }

    public void InvokeSynchronously(Action action)
    {
        if (action == null)
            throw new ArgumentNullException("action");

        EnsureInitialized();

        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _context.Send(delegate { action(); }, null);
        }
    }

    public bool InvokeRequired
    {
        get
        {
            EnsureInitialized();
            return !_dispatcher.CheckAccess();
        }
    }
}
}
