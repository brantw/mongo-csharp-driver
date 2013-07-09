﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Thrown when a feature is not supported.
    /// </summary>
    [Serializable]
    public class FeatureNotRegisteredException : MongoDriverException
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureNotRegisteredException" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FeatureNotRegisteredException(string name) 
            : base(string.Format("The feature '{0}' has not been registered.", name))
        {
        }
    }
}