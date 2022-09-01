// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMediaRecorder.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_MAGICLEAP || UNITY_ANDROID
using UnityEngine.XR.MagicLeap.Native;
#endif

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// APIs for video and audio recording.
    /// </summary>
    public partial class MLMediaRecorder
    {
        /// <summary>
        /// Video source to put in the recorded media.
        /// </summary>
        public enum VideoSource
        {
            /// <summary>
            ///  Camera video source.
            /// </summary>
            Camera = 1,
        };

        /// <summary>
        /// Audio source to put in the recorded media.
        /// </summary>
        public enum AudioSource
        {
            /// <summary>
            ///  Recording voice.
            /// </summary>
            Voice = 0,

            /// <summary>
            ///  Recording ambient sounds.
            /// </summary>
            World,

            /// <summary>
            ///  Recording digital sounds.
            /// </summary>
            Virtual,

            /// <summary>
            ///  Mixed reality mode: digital + ambient.
            /// </summary>
            Mixed,
        };

        /// <summary>
        /// Media recorder events
        /// </summary>
        public enum Event
        {
            /// <summary>
            ///  Media recorder error.
            /// </summary>
            Error = 1,

            /// <summary>
            ///  Media recorder info.
            /// </summary>
            Info = 2,

            /// <summary>
            ///  Media recorder track error.
            /// </summary>
            TrackError = 100,

            /// <summary>
            ///  Media recorder track info.
            /// </summary>
            TrackInfo = 101,
        };

        public enum Info
        {
            Unknown = 1,

            /// <summary>
            ///  Max duration of the clip is reached.
            /// </summary>
            MaxDurationReached = 800,

            /// <summary>
            ///  Max file size is reached.
            /// </summary>
            MaxFileSizeReached = 801,

            /// <summary>
            ///  Max file size is approaching.
            /// </summary>
            MaxFileSizeApproaching = 802,

            /// <summary>
            ///  Next output file has started.
            /// </summary>
            TrackCompletionStatus = 1000,

            /// <summary>
            ///  The info about progress in time.
            /// </summary>
            TrackProgressInTime = 1001,

            /// <summary>
            ///  track info.
            /// </summary>
            TrackType = 1002,

            /// <summary>
            ///  The info about track duration.
            /// </summary>
            TrackDurationMs = 1003,

            /// <summary>
            ///  The time to measure the max chunk duration.
            /// </summary>
            TrackMaxChunkDurationMs = 1004,

            /// <summary>
            ///  The info about encoded frames.
            /// </summary>
            TrackEncodedFrames = 1005,

            /// <summary>
            ///  The time to measure how well the audio and video track data interleaved.
            /// </summary>
            TrackInterChunkTimeUs = 1006,

            /// <summary>
            ///  The time to measure system response.
            /// </summary>
            TrackInitialDelayMs = 1007,

            /// <summary>
            ///   The time used to compensate for initial A/V sync.
            /// </summary>
            TrackStartOffsetMs = 1008,

            /// <summary>
            ///  Total number of bytes of the media data.
            /// </summary>
            TrackDataKBytes = 1009,
        };

        /// <summary>
        /// Video recorder error types
        /// </summary>
        public enum Error
        {
            Unknown = 1,
            ServerDied = 2,
            TrackGeneral = 100,
            VideoNoSyncFrame = 200,
        };

        /// <summary>
        /// Possible output formats
        /// </summary>
        public enum OutputFormat
        {
            Default = 0,

            /// <summary>
            ///  3gpp format.
            /// </summary>
            THREE_GPP,

            /// <summary>
            ///  MP4 format.
            /// </summary>
            MPEG_4,

            /// <summary>
            ///  AMR NB.
            /// </summary>
            AMR_NB,

            /// <summary>
            ///  AMR WB.
            /// </summary>
            AMR_WB,

            /// <summary>
            ///  AAC_ADIF.
            /// </summary>
            AAC_ADIF,

            /// <summary>
            ///  AAC_ADTS.
            /// </summary>
            AAC_ADTS,

            /// <summary>
            ///  Stream over a socket, limited to a single stream.
            /// </summary>
            RTP_AVP,

            /// <summary>
            ///  H.264/AAC data encapsulated in MPEG2/TS.
            /// </summary>
            MPEG2TS,

            /// <summary>
            ///  VP8/VORBIS data in a WEBM container.
            /// </summary>
            WEBM,

            /// <summary>
            ///  HEIC data in a HEIF container.
            /// </summary>
            HEIF,

            /// <summary>
            ///  Opus data in a OGG container.
            /// </summary>
            OGG,
        };

        /// <summary>
        /// Available video encoder formats
        /// </summary>
        public enum VideoEncoder
        {
            Default = 0,

            /// <summary>
            ///  H263. This format has support for software encoder.
            /// </summary>
            H263,

            /// <summary>
            ///  H264. This format has support for hardware encoder.
            /// </summary>
            H264,

            /// <summary>
            ///  MPEG4 SP. This format has support for software encoder.
            /// </summary>
            MPEG_4_SP,

            /// <summary>
            ///  VP8. This format has support for software encoder.
            /// </summary>
            VP8,

            /// <summary>
            ///  HEVC. This format has support for hardware encoder.
            /// </summary>
            HEVC,
        };

        /// <summary>
        /// Available audio encoder formats
        /// </summary>
        public enum AudioEncoder
        {
            Default = 0,

            /// <summary>
            ///  AMR NB.
            /// </summary>
            AMR_NB,

            /// <summary>
            ///  AMR WB.
            /// </summary>
            AMR_WB,

            /// <summary>
            ///  AAC.
            /// </summary>
            AAC,

            /// <summary>
            ///  HE AAC.
            /// </summary>
            HE_AAC,

            /// <summary>
            ///  AAC ELD.
            /// </summary>
            AAC_ELD,

            /// <summary>
            ///  Vorbis.
            /// </summary>
            VORBIS,

            /// <summary>
            ///  Opus.
            /// </summary>
            OPUS,
        };

        /// <summary>
        /// Info received when the media recorder runs into an error.
        /// </summary>
        public struct OnErrorData
        {
            /// <summary>
            /// The error of MLMediaRecorder.Error
            /// </summary>
            public Error Error;

            /// <summary>
            /// The extra info
            /// </summary>
            public int Extra;
        };
        
        /// <summary>
        /// Info received when the media recorder runs into a track error.
        /// </summary>
        public struct OnTrackErrorData
        {
            /// <summary>
            /// Track ID When the error or info type is track specific. 
            /// </summary>
            public uint TrackId;

            /// <summary>
            /// The error of MLMediaRecorder.Error
            /// </summary>
            public Error Error;

            /// <summary>
            /// The extra info
            /// </summary>
            public int Extra;
        };

        public struct OnInfoData
        {
            /// <summary>
            /// The info of MLMediaRecorder.Info
            /// </summary>
            public Info Info;

            /// <summary>
            /// The extra info
            /// </summary>
            public int Extra;
        };
        
        public struct OnTrackInfoData
        {
            /// <summary>
            /// Track ID When the error or info type is track specific. 
            /// </summary>
            public uint TrackId;

            /// <summary>
            /// The info of MLMediaRecorder.Info
            /// </summary>
            public Info Info;

            /// <summary>
            /// The extra info
            /// </summary>
            public int Extra;
        };

        public delegate void OnInfoDelegate(OnInfoData info);
        public delegate void OnTrackInfoDelegate(OnTrackInfoData info);

        public delegate void OnErrorDelegate(OnErrorData trackInfo);
        public delegate void OnTrackErrorDelegate(OnTrackErrorData trackInfo);

        /// <summary>
        /// MediaRecorder received a general info/warning message.
        /// </summary>
        public event OnInfoDelegate OnInfo = delegate { };

        /// <summary>
        /// MediaRecorder received a track-related info/warning message.
        /// </summary>
        public event OnTrackInfoDelegate OnTrackInfo = delegate { };

        /// <summary>
        /// MediaRecorder received a general error message.
        /// </summary>
        public event OnErrorDelegate OnError = delegate { };

        /// <summary>
        /// MediaRecorder received a track-related error message.
        /// </summary>
        public event OnTrackErrorDelegate OnTrackError = delegate { };

#if UNITY_MAGICLEAP || UNITY_ANDROID
        /// <summary>
        /// Handle to the underlying media recorder object.
        /// </summary>
        public ulong Handle { get; private set; } = MagicLeapNativeBindings.InvalidHandle;
#else
        public ulong Handle { get; private set; }
#endif

        /// <summary>
        /// Native surface object which should be used to get the
        /// native buffers to render the video frames onto for recorded.
        /// </summary>
        public MLNativeSurface InputSurface { get; private set; }

        /// <summary>
        /// Handle for the managed media recorder object to pass to and from unmanaged code.
        /// </summary>
        private GCHandle gcHandle;

        /// <summary>
        /// Create a media recorder object
        /// </summary>
        /// <returns>Media recorder object if construction was successful, null otherwise</returns>
        public static MLMediaRecorder Create()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLResult.Code resultCode = NativeBindings.MLMediaRecorderCreate(out ulong handle);
            return MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderCreate)) ? new MLMediaRecorder(handle) : null;
#else
            return null;
#endif
        }

        private MLMediaRecorder(ulong handle)
        {
            this.Handle = handle;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            NativeBindings.MLMediaRecorderEventCallbacks callbacks = NativeBindings.MLMediaRecorderEventCallbacks.Create();
            this.gcHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            IntPtr gcHandlePtr = GCHandle.ToIntPtr(this.gcHandle);
            var resultCode = NativeBindings.MLMediaRecorderSetEventCallbacks(handle, ref callbacks, gcHandlePtr);
            MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderSetEventCallbacks));
#endif
        }

        ~MLMediaRecorder()
        {
            Destroy();
            gcHandle.Free();
        }

        private void Destroy()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            if (!MagicLeapNativeBindings.MLHandleIsValid(Handle))
                return;

            ReleaseInputSurface();

            MLResult.Code resultCode = NativeBindings.MLMediaRecorderDestroy(Handle);
            if (MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderDestroy)))
                Handle = MagicLeapNativeBindings.InvalidHandle;
#endif
        }

        /// <summary>
        /// Pass in the unmanaged file descriptor of the file to be written.
        /// Call this after MLMediaRecorder.SetOutputFormat() but before
        /// MLMediaRecorder.Prepare().
        /// </summary>
        /// <param name="fd">Unmanaged file descriptor of the output file.</param>
        public MLResult SetOutputFileForFD(int fd)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetOutputFileForFD(Handle, fd));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the path of the output file to be produced.
        /// Call this after MLMediaRecorder.SetOutputFormat() but before
        /// MLMediaRecorder.Prepare().
        /// </summary>
        /// <param name="path">Path to output file. Folders should exist already.</param>
        public MLResult SetOutputFileForPath(string path)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetOutputFileForPath(Handle, path));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets MediaRecorder video source. Cannot be called twice without
        /// calling MLMediaRecorder.Reset() in between.
        /// </summary>
        public MLResult SetVideoSource(VideoSource inVideoSource)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetVideoSource(Handle, inVideoSource));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Set MediaRecorder audio source. Cannot be called twice without
        /// calling MLMediaRecorder.Reset() in between.
        /// </summary>
        public MLResult SetAudioSource(AudioSource inAudioSource)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetAudioSource(Handle, inAudioSource));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the format of the output file produced during recording.
        /// </summary>
        public MLResult SetOutputFormat(OutputFormat inFormat)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetOutputFormat(Handle, inFormat));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the video encoder to be used for recording.
        /// If this method is not called, the output file will not contain a
        /// video track. Call this after MLMediaRecorder.SetOutputFormat() and
        /// before MLMediaRecorder.Prepare(). The video source is
        /// always set to camera by default.
        /// </summary>
        public MLResult SetVideoEncoder(VideoEncoder inVideoEncoder)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetVideoEncoder(Handle, inVideoEncoder));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the audio encoder to be used for recording.
        /// If this method is not called, the output file will not contain an
        /// audio track. Call this after MLMediaRecorder.SetOutputFormat() and
        /// before MLMediaRecorder.Prepare().
        /// </summary>
        public MLResult SetAudioEncoder(AudioEncoder inAudioEncoder)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetAudioEncoder(Handle, inAudioEncoder));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the maximum duration (in ms) of the recording session. Call this
        /// after MLMediaRecorder.SetOutputFormat() and before
        /// MLMediaRecorder.Prepare(). After recording reaches the specified
        /// duration, a notification will be sent via the callback
        /// with a MLMediaRecorder.Info code of MLMediaRecorder.Info.MaxDurationReached
        /// and recording will be stopped. Stopping happens asynchronously, there
        /// is no guarantee that the recorder will have stopped by the time the listener is notified.
        /// </summary>
        public MLResult SetMaxDuration(int inMaxDurationMsec)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetMaxDuration(Handle, inMaxDurationMsec));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the maximum file size (in bytes) of the recording session.
        /// Call this after MLMediaRecorder.SetOutputFormat() and before MLMediaRecorder.Prepare().
        /// After recording reaches the specified filesize, a notification
        /// will be sent via the callback with a MLMediaRecorder.Info code of
        /// MLMediaRecorder.Info.MaxFileSizeReached and recording will be stopped.
        /// happens asynchronously, there is no guarantee that the recorder
        /// will have stopped by the time the listener is notified.
        /// </summary>
        public MLResult SetMaxFileSize(long inMaxFileSize)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetMaxFileSize(Handle, inMaxFileSize));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Sets the GEO location for recording.
        /// </summary>
        /// <param name="inLatitude10000">the 10000 multiplies latitude of location.</param>
        /// <param name="inLongitude10000">the 10000 multiplies longitude of location.</param>
        public MLResult SetGeoLocation(long inLatitude10000, long inLongitude10000)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderSetGeoLocation(Handle, inLatitude10000, inLongitude10000));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Prepares the recorder to begin capturing and encoding data for input mediaformat.
        /// Should be called after setting up the desired audio and video sources,
        /// encoders, but before MLMediaRecorder.Start().
        /// </summary>
        /// <param name="format">Media format object to configure the video & audio track.</param>
        public MLResult Prepare(MLMediaFormat format)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLResult.Code resultCode = NativeBindings.MLMediaRecorderPrepare(Handle, format.Handle);
            MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderPrepare));
            
            return MLResult.Create(resultCode);
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Begins capturing and encoding data to the specified file.
        /// Call this after MLMediaRecorder.Prepare(). The apps should
        /// not start another recording session during recording.
        /// </summary>
        public MLResult Start()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderStart(Handle));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Gets the input surface to record from when using SURFACE video source. May only be called after MLMediaRecorderPrepare and
        /// MLMediaRecorderStart. Frames rendered to the producer before MLMediaRecorderStart() is called will be discarded.  When using an input
        /// surface, there are no accessible input buffers, as buffers are automatically passed from the other modules to this surface.
        /// The returned input surface can also be passed as a destination surface to - a video/mixed reality video capture session
        /// when calling MLCameraPrepareCapture(). Captured raw video frames will be consumed directly as input to an encoder
        /// without copying.  Caller of this API should release the surface using #MLMediaRecorderReleaseInputSurface() on the Surface
        /// handle after usage.
        /// </summary>
        public MLResult GetInputSurface()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            var resultCode = NativeBindings.MLMediaRecorderGetInputSurface(Handle, out ulong inputSurface);
            if (MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderGetInputSurface)))
            {
                InputSurface = new MLNativeSurface(inputSurface);
            }
            
            return MLResult.Create(resultCode);
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Returns the maximum absolute amplitude that was sampled since the
        /// last call to this method. Call this only after the
        /// MLMediaRecorder.SetAudioSource().
        /// </summary>
        public MLResult GetMaxAmplitude(out int maxAmp)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderGetMaxAmplitude(Handle, out maxAmp));
#else
            maxAmp = 0;
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Stops recording. Call this after MLMediaRecorder.Start().
        /// Once recording is stopped, you will have to configure it
        /// again as if it has just been constructed.
        /// </summary>
        public MLResult Stop()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return MLResult.Create(NativeBindings.MLMediaRecorderStop(Handle));
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        /// <summary>
        /// Restarts the MediaRecorder to its idle state. After calling this method,
        /// you will have to configure it again as if it had just been constructed.
        /// </summary>
        public MLResult Reset()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLResult.Code resultCode = NativeBindings.MLMediaRecorderReset(Handle);
            MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderReset));
            ReleaseInputSurface();
            return MLResult.Create(resultCode);
#else
            return MLResult.Create(MLResult.Code.NotImplemented);
#endif
        }

        private MLResult.Code ReleaseInputSurface()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLResult.Code resultCode = MLResult.Code.Ok;
            if (InputSurface != null && MagicLeapNativeBindings.MLHandleIsValid(InputSurface.Handle))
            {
                resultCode = NativeBindings.MLMediaRecorderReleaseInputSurface(Handle, InputSurface.Handle);
                MLResult.DidNativeCallSucceed(resultCode, nameof(NativeBindings.MLMediaRecorderReleaseInputSurface));
                InputSurface = null;
            }

            return resultCode;
#else
            return MLResult.Code.NotImplemented;
#endif
        }
    }
}
