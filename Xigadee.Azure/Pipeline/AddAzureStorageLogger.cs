﻿#region Copyright
// Copyright Hitachi Consulting
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Xigadee
{
    public static partial class AzureExtensionMethods
    {
        [Obsolete("This will be removed. Used the Azure Storage DataCollector instead")]
        public static P AddAzureStorageLogger<P>(this P pipeline
            , string serviceName = null
            , string containerName = "log"
            , ResourceProfile resourceProfile = null
            , Action<AzureStorageLogger> onCreate = null)
            where P:IPipeline
        {
            return pipeline.AddAzureStorageLogger(pipeline.Configuration.LogStorageCredentials(), serviceName ?? pipeline.Service?.Id.Name, containerName, resourceProfile, pipeline.Configuration.AesEncryptionWithCompression(), onCreate);
        }

        [Obsolete("This will be removed. Used the Azure Storage DataCollector instead")]
        public static P AddAzureStorageLogger<P>(this P pipeline
            , StorageCredentials creds
            , string serviceName = null
            , string containerName = "log"
            , ResourceProfile resourceProfile = null
            , IEncryptionHandler encryption = null
            , Action<AzureStorageLogger> onCreate = null)
            where P : IPipeline
        {
            var logger = new AzureStorageLogger(creds, serviceName ?? pipeline.Service?.Id.Name, containerName, resourceProfile, encryption);
            onCreate?.Invoke(logger);

            return pipeline.AddLogger(logger);
        }
    }
}
