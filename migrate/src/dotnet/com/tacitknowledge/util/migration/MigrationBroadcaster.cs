/* Copyright 2006 Tacit Knowledge LLC
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
	
	/// <summary> Manages the <code>MessageListener</code> that are associated with a
	/// <code>Migration</code> instance. 
	/// 
	/// </summary>
	/// <author>   Scott Askew (scott@tacitknowledge.com)
	/// </author>
	class MigrationBroadcaster
	{
		/// <summary> Get the list of listeners
		/// 
		/// </summary>
		/// <returns> List of MigrationListener objects
		/// </returns>
		virtual public System.Collections.IList Listeners
		{
			get
			{
				return listeners;
			}
			
		}
		/// <summary> Used by <code>notifyListeners</code> to indicate that listeners should
		/// be informed that a task is about to start. 
		/// </summary>
		public const int TASK_START = 1;
		
		/// <summary> Used by <code>notifyListeners</code> to indicate that listeners should
		/// be informed that a task has successfully completed. 
		/// </summary>
		public const int TASK_SUCCESS = 2;
		
		/// <summary> Used by <code>notifyListeners</code> to indicate that listeners should
		/// be informed that a task failed. 
		/// </summary>
		public const int TASK_FAILED = 3;
		
		/// <summary> The listeners interested in being notified of migration task events.</summary>
		private System.Collections.IList listeners = new System.Collections.ArrayList();
		
		/// <summary> Notifies all registered listeners of a migration task event.
		/// 
		/// </summary>
		/// <param name="task">the task that is being or that has been executed
		/// </param>
		/// <param name="context">the context in which the task was executed
		/// </param>
		/// <param name="eventType">TASK_START, TASK_SUCCESS, or TASK_FAIL
		/// </param>
		/// <param name="e">the exception thrown by the task if the task failed
		/// </param>
		/// <throws>  MigrationException if one of the listeners threw an exception  </throws>
		public virtual void  notifyListeners(MigrationTask task, MigrationContext context, MigrationException e, int eventType)
		{
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratorhasNext'"
			for (System.Collections.IEnumerator i = listeners.GetEnumerator(); i.MoveNext(); )
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1073_javautilIteratornext'"
				MigrationListener listener = (MigrationListener) i.Current;
				switch (eventType)
				{
					
					case TASK_START: 
						listener.migrationStarted(task, context);
						break;
					
					
					case TASK_SUCCESS: 
						listener.migrationSuccessful(task, context);
						break;
					
					
					case TASK_FAILED: 
						listener.migrationFailed(task, context, e);
						break;
					
					
					default: 
						throw new System.ArgumentException("Unknown event type");
					
				}
			}
		}
		
		/// <summary> Notifies all registered listeners of a migration task event.
		/// 
		/// </summary>
		/// <param name="task">the task that is being or that has been executed
		/// </param>
		/// <param name="context">the context in which the task was executed
		/// </param>
		/// <param name="eventType">TASK_START, TASK_SUCCESS, or TASK_FAIL
		/// </param>
		/// <throws>  MigrationException if one of the listeners threw an exception  </throws>
		public virtual void  notifyListeners(MigrationTask task, MigrationContext context, int eventType)
		{
			notifyListeners(task, context, null, eventType);
		}
		
		/// <summary> Registers the given <code>MigrationListener</code> as being interested
		/// in migration task events.
		/// 
		/// </summary>
		/// <param name="listener">the listener to add; may not be <code>null</code>
		/// </param>
		public virtual void  addListener(MigrationListener listener)
		{
			if (listener == null)
			{
				throw new System.ArgumentException("listener cannot be null");
			}
			listeners.Add(listener);
		}
		
		/// <summary> Removes the given <code>MigrationListener</code> from the list of listeners
		/// associated with the <code>Migration</code> instance.
		/// 
		/// </summary>
		/// <param name="listener">the listener to add; may not be <code>null</code>
		/// </param>
		/// <returns> <code>true</code> if the listener was located and removed,
		/// otherwise <code>false</code>.
		/// </returns>
		public virtual bool removeListener(MigrationListener listener)
		{
			if (listener == null)
			{
				throw new System.ArgumentException("listener cannot be null");
			}
			System.Boolean tempBoolean;
			tempBoolean = listeners.Contains(listener);
			listeners.Remove(listener);
			return tempBoolean;
		}
	}
}