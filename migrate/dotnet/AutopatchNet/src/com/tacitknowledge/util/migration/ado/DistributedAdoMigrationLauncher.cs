/* 
 * Copyright 2007 Tacit Knowledge LLC
 * 
 * Licensed under the Tacit Knowledge Open License, Version 1.0 (the "License");
 * you may not use this file except in compliance with the License. You may
 * obtain a copy of the License at http://www.tacitknowledge.com/licenses-1.0.
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#region Imports
using System;
using com.tacitknowledge.util.migration;
#endregion

namespace com.tacitknowledge.util.migration.ado
{
	
	/// <summary>
    /// Core starting point for a distributed database migration run. This class obtains a connection
	/// to the orchestration database, checks its patch level, delegates the actual execution of the
    /// migration tasks to a <code>MigrationProcess</code> instance, and then commits and cleans
    /// everything up at the end.
	/// </summary>
	/// <author>Mike Hardy (mike@tacitknowledge.com)</author>
    /// <author>Vladislav Gangan (vgangan@tacitknowledge.com)</author>
    /// <version>$Id$</version>
    public class DistributedAdoMigrationLauncher : AdoMigrationLauncher
    {
        #region Constructors
        /// <seealso cref="AdoMigrationLauncher.AdoMigrationLauncher()"/>
        public DistributedAdoMigrationLauncher()
            : base()
        {
        }

        /// <seealso cref="AdoMigrationLauncher.AdoMigrationLauncher(IAdoMigrationContext)"/>
        public DistributedAdoMigrationLauncher(IAdoMigrationContext context)
            : base(context)
        {
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Overrides the base class' property so we get a <code>DistributedMigrationProcess</code>
        /// instead of the normal one.
		/// </summary>
        public override MigrationProcess NewMigrationProcess
		{
			get { return new DistributedMigrationProcess(); }
        }
        #endregion

        #region Public methods
        /// <seealso cref="AdoMigrationLauncher.DoMigrations()"/>
		public override int DoMigrations()
		{
            if (Contexts.Count == 0)
            {
                throw new MigrationException("You must configure a migration context");
            }

            return base.DoMigrations();
        }
        #endregion
    }
}
