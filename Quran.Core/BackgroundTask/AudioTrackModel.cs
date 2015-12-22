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
using System.Runtime.Serialization;

namespace Quran.Core
{
    /// <summary>
    /// Simple representation for track in a playlist that can be used both for
    /// data model (across processes) and view model (foreground UI)
    /// </summary>
    [DataContract]
    public class AudioTrackModel
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Details { get; set; }

        [DataMember]
        public KeyValuePair<int, int> Ayah { get; set; }
    }
}
