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
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Quran.Core.Common;

namespace Quran.Core
{
    [DataContract]
    public class TrackChangedMessage
    {
        public TrackChangedMessage()
        {
        }

        public TrackChangedMessage(KeyValuePair<int, int> ayah)
        {
            this.Ayah = ayah;
        }

        public TrackChangedMessage(string ayah)
        {
            if (string.IsNullOrEmpty(ayah))
            {
                throw new ArgumentNullException(nameof(ayah));
            }

            var splitString = ayah.Split(':');
            this.Ayah = new KeyValuePair<int, int>(
                int.Parse(splitString[0], CultureInfo.InvariantCulture), 
                int.Parse(splitString[1], CultureInfo.InvariantCulture));
        }

        [DataMember]
        public KeyValuePair<int, int> Ayah;
    }
}
