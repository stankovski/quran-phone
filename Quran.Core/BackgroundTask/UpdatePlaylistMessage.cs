//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core
{
    [DataContract]
    public class UpdatePlaylistMessage
    {
        public UpdatePlaylistMessage()
        { }

        public UpdatePlaylistMessage(List<AudioTrackModel> tracks) : this(tracks, null)
        { }

        public UpdatePlaylistMessage(List<AudioTrackModel> tracks, AudioTrackModel currentTrack)
        {
            this.Tracks = tracks;
            this.CurrentTrack = currentTrack;
        }

        [DataMember]
        public List<AudioTrackModel> Tracks { get; set; }

        [DataMember]
        public AudioTrackModel CurrentTrack { get; set; }
    }
}
