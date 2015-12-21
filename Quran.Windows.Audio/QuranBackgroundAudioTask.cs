using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Utils;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Quran.Windows.Audio
{
    public sealed class QuranBackgroundAudioTask : IBackgroundTask
    {
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";
        private const string RequestString = "requestString";

        private SystemMediaTransportControls smtc;
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private BackgroundTaskDeferral deferral; // Used to keep task alive

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");

            // Initialize SystemMediaTransportControls (SMTC) for integration with
            // the Universal Volume Control (UVC).
            //
            // The UI for the UVC must update even when the foreground process has been terminated
            // and therefore the SMTC is configured and updated from the background task.
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += AudioControlButtonPressed;
            smtc.PropertyChanged += AudioControlPropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;

            // Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayerStateChanged;

            // Initialize message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += MessageReceivedFromForeground;

            // Send information to foreground that background task has been started if app is active
            MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            backgroundTaskStarted.Set();

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled); // event may raise immediately before continung thread excecution so must be at the end
        }

        /// <summary>
        /// Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void AudioControlPropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // TODO: If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        /// This function controls the button events from UVC.
        /// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AudioControlButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");

                    // When the background task has been suspended and the SMTC
                    // starts it again asynchronously, some time is needed to let
                    // the task startup process in Run() complete.

                    // Wait for task to start. 
                    // Once started, this stays signaled until shutdown so it won't wait
                    // again unless it needs to.
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                    {
                        throw new Exception("Background Task didnt initialize in time");
                    }

                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }

        void MediaPlayerStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        /// <summary>
        /// Raised when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            // Initializing FileUtils
            await FileUtils.Initialize(true);

            StartPlaybackMessage startPlaybackMessage;
            if (MessageService.TryParseMessage(e.Data, out startPlaybackMessage))
            {
                //Foreground App process has signalled that it is ready for playback
                Debug.WriteLine("Starting Playback");
                StartPlayback();
                return;
            }

            SkipNextMessage skipNextMessage;
            if (MessageService.TryParseMessage(e.Data, out skipNextMessage))
            {
                // User has chosen to skip track from app context.
                Debug.WriteLine("Skipping to next");
                SkipToNext();
                return;
            }

            SkipPreviousMessage skipPreviousMessage;
            if (MessageService.TryParseMessage(e.Data, out skipPreviousMessage))
            {
                // User has chosen to skip track from app context.
                Debug.WriteLine("Skipping to previous");
                SkipToPrevious();
                return;
            }

            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                //var index = playbackList.Items.ToList().FindIndex(i => (Uri)i.Source.CustomProperties[TrackIdKey] == trackChangedMessage.TrackId);
                //Debug.WriteLine("Skipping to track " + index);
                //smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                //playbackList.MoveTo((uint)index);
                // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
                CreatePlaybackList(new AudioRequest(trackChangedMessage.AudioRequest));
                BackgroundMediaPlayer.Current.Play();
                return;
            }

            UpdatePlaylistMessage updatePlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out updatePlaylistMessage))
            {
                CreatePlaybackList(new AudioRequest(updatePlaylistMessage.AudioRequest));
                return;
            }
        }

        /// <summary>
        /// Start playlist and change UVC state
        /// </summary>
        private void StartPlayback()
        {
            try
            {
                BackgroundMediaPlayer.Current.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MovePrevious();

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();

        }

        private const int PLAYLIST_SIZE = 100;
        /// <summary>
        /// Create a playback list from the list of songs received from the foreground app.
        /// </summary>
        /// <param name="songs"></param>
        async void CreatePlaybackList(AudioRequest request)
        {
            // Make a new list and disable looping
            if (playbackList != null)
            {
                playbackList.CurrentItemChanged -= PlaybackListCurrentItemChanged;
            }
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = false;

            // Add playback items to the list
            for (int i = 0; i < PLAYLIST_SIZE; i++)
            {
                var source = await GetTrackFromRequest(request);
                request.GotoNextAyah();
                if (source == null)
                {
                    break;
                }
                playbackList.Items.Add(new MediaPlaybackItem(source));
            }

            // Don't auto start
            BackgroundMediaPlayer.Current.AutoPlay = false;

            // Assign the list to the player
            BackgroundMediaPlayer.Current.Source = playbackList;

            // Add handler for future playlist item changes
            playbackList.CurrentItemChanged += PlaybackListCurrentItemChanged;
        }

        private async Task<MediaSource> GetTrackFromRequest(AudioRequest request)
        {
            var ayah = request.CurrentAyah;
            var title = ayah.Ayah == 0 ? "Bismillah" : QuranUtils.GetSurahAyahString(ayah.Surah, ayah.Ayah);
            var path = AudioUtils.GetLocalPathForAyah(ayah.Ayah == 0 ? new QuranAyah(1, 1) : ayah, request.Reciter);

            if (!File.Exists(path))
            {
                return null;
            }
            else
            {
                var source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(path));
                source.CustomProperties[TrackIdKey] = path;
                source.CustomProperties[TitleKey] = title;
                source.CustomProperties[RequestString] = request.ToString();
                return source;
                //return new AudioTrack(new Uri(path, UriKind.Relative), title, request.Reciter.Name, "Quran", null,
                //    request.ToString(), EnabledPlayerControls.All);
            }
        }

        /// <summary>
        /// Raised when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void PlaybackListCurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            // Get the new item
            var item = args.NewItem;

            // Update the system view
            UpdateUVCOnNewTrack(item);

            // Get the current track
            if (item != null && item.Source.CustomProperties.ContainsKey(RequestString))
            {
                var request = item.Source.CustomProperties[RequestString] as string;
                MessageService.SendMessageToForeground(new TrackChangedMessage(request));
            }      
        }

        private Uri GetTrackId(MediaPlaybackItem item)
        {
            if (item == null)
                return null; // no track playing

            return item.Source.CustomProperties[TrackIdKey] as Uri;
        }

        /// <summary>
        /// Update Universal Volume Control (UVC) using SystemMediaTransPortControl APIs
        /// </summary>
        private void UpdateUVCOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                smtc.DisplayUpdater.Update();
                return;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;

            // TODO: Add image
            //var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
            //if (albumArtUri != null)
            //    smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
            //else
            //    smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
        }

        /// <summary>
        /// Indicate that the background task is completed.
        /// </summary>       
        void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            deferral.Complete();
        }

        /// <summary>
        /// Handles background task cancellation. Task cancellation happens due to:
        /// 1. Another Media app comes into foreground and starts playing music 
        /// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        /// In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                // immediately set not running
                backgroundTaskStarted.Reset();

                // unsubscribe from list changes
                if (playbackList != null)
                {
                    playbackList.CurrentItemChanged -= PlaybackListCurrentItemChanged;
                    playbackList = null;
                }

                // remove handlers for MediaPlayer
                BackgroundMediaPlayer.Current.CurrentStateChanged -= MediaPlayerStateChanged;

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= MessageReceivedFromForeground;
                smtc.ButtonPressed -= AudioControlButtonPressed;
                smtc.PropertyChanged -= AudioControlPropertyChanged;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
    }
}
