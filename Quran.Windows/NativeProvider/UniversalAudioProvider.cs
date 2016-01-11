using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Interfaces;
using Quran.Core.Utils;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;

namespace Quran.Windows.NativeProvider
{
    public class UniversalAudioProvider : IAudioProvider, IDisposable
    {
        const uint E_ABORT = 0x80004004;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        private AutoResetEvent backgroundAudioTaskStarted;
        private readonly CoreDispatcher _dispatcher;
        private TelemetryClient telemetry = new TelemetryClient();

        public UniversalAudioProvider()
        {
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            // Setup the initialization lock
            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        public event TypedEventHandler<IAudioProvider, QuranAudioTrack> TrackChanged;
        public event TypedEventHandler<IAudioProvider, AudioPlayerPlayState> StateChanged;

        public void Play()
        {
            if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Play();
            }
        }

        public void Pause()
        {
            CurrentPlayer.Pause();
        }


        public void Stop()
        {
            if (CurrentPlayer.CurrentState == MediaPlayerState.Playing)
            {
                CurrentPlayer.Pause();
            }

            if (CurrentPlayer.CurrentState != MediaPlayerState.Stopped)
            {
                CurrentTrack = null;
                State = AudioPlayerPlayState.Stopped;
                if (StateChanged != null)
                {
                    StateChanged(this, this.State);
                }
            }
        }

        public void Next()
        {
            MessageService.SendMessageToBackground(new SkipNextMessage());
        }

        public void Previous()
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());
        }

        public AudioPlayerPlayState State
        {
            get; set;
        }

        public QuranAudioTrack CurrentTrack { get; private set; }

        public void SetTrack(QuranAudioTrack request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // Start task
                StartBackgroundAudioTask();                
            }

            MessageService.SendMessageToBackground(new TrackChangedMessage(request));
        }
        
        public TimeSpan Position
        {
            get; set;
        }

        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && --retryCount >= 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "GetCurrentPlayerServerUnavailable" } });
                            // The foreground app uses RPC to communicate with the background process.
                            // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }

        public bool Repeat
        {
            get; set;
        }

        /// <summary>
        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state. Unfortunately,
        /// any attempts to unregister things on BackgroundMediaPlayer.Current will fail with the RPC error once the background task has been lost.
        /// </summary>
        private void ResetAfterLostBackground()
        {
            backgroundAudioTaskStarted.Reset();
            BackgroundMediaPlayer.Shutdown();

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundPlayerMessageReceivedFromBackground;
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundPlayerMessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "ResetPlayerServerUnavailable" } });
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// This event is raised when a message is recieved from BackgroundAudioTask
        /// </summary>
        async void BackgroundPlayerMessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                // When foreground app is active change track based on background message
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentTrack = trackChangedMessage.AudioTrack;

                    if (TrackChanged != null)
                    {
                        TrackChanged(this, CurrentTrack);
                    }
                });
                return;
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                backgroundAudioTaskStarted.Set();
                return;
            }

            TrackEndedMessage trackEndedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackEndedMessage))
            {
                // Do nothing for now
            }
        }

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            bool result = backgroundAudioTaskStarted.WaitOne(10000);
            //Send message to initiate playback
            if (result != true)
            {
                throw new Exception("Background Audio Task didn't start in expected time");
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged -= this.MediaPlayerCurrentStateChanged;
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundPlayerMessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "RemovePlayerServerUnavailable" } });
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayerCurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundPlayerMessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ResetAfterLostBackground();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlayerCurrentStateChanged(MediaPlayer sender, object args)
        {
            MediaPlayerState currentState = MediaPlayerState.Stopped;
            try
            {
                currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == E_ABORT || ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // do nothing
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "CurrentPlayerStateChangedGetState" } });
                }
                else
                {
                    throw;
                }
            }

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var appstate = (AudioPlayerPlayState)(int)currentState;
                if (!(this.State == AudioPlayerPlayState.Stopped && appstate == AudioPlayerPlayState.Paused))
                {
                    this.State = appstate;
                }
                if (StateChanged != null)
                {
                    StateChanged(this, appstate);
                }
            });
        }

        public void Dispose()
        {
            GC.WaitForPendingFinalizers();
            RemoveMediaPlayerEventHandlers();
        }
    }
}
