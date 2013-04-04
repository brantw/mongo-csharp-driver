﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a binding to a cluster.
    /// </summary>
    public interface IClusterBinding : IMongoBinding
    {
        // methods
        /// <summary>
        /// Gets a binding to a connection (to a node in this cluster).
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A connection binding.</returns>
        ConnectionBinding GetConnectionBinding(INodeSelector selector);

        /// <summary>
        /// Gets a binding to a node in this cluster.
        /// </summary>
        /// <param name="selector">The node selector.</param>
        /// <returns>A node binding.</returns>
        INodeBinding GetNodeBinding(INodeSelector selector);
    }
}
