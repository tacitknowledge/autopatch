/* Copyright 2004 Tacit Knowledge LLC
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
#endregion
namespace com.tacitknowledge.util.migration
{
	
	/// <summary> 
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	/// <version>  $Id$
	/// </version>
	[Serializable]
	public class MigrationException:System.Exception
	{
		/// <seealso cref="Exception.Exception(String)">
		/// </seealso>
		public MigrationException(System.String message):base(message)
		{
		}
		
		
		public MigrationException(System.String message, System.Exception cause):base(message, cause)
		{
		}
	}
}