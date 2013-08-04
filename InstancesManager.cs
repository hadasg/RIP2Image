/*******************************************************************************
Description:
	Ghostscript Service instances manager.

COPYRIGHT (C) 2013 Hadas Groisman & Amit Cohen.
  
 	This file is part of GhostscriptService.

    GhostscriptService is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    GhostscriptService is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with GhostscriptService.  If not, see <http://www.gnu.org/licenses/>.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace GhostscriptService
{
	/// <summary>
	/// Manage FileConvertor instances shard resource as thread safe. 
	/// </summary>
	internal class InstancesManager
	{
		/// <summary>
		/// private constructor
		/// </summary>
		private InstancesManager(){}

		/// <summary>
		/// Instances shard resource - thread safe.
		/// </summary>
		static private ConcurrentBag<FileConverter> m_ConvertorInstances = new ConcurrentBag<FileConverter>();

		/// <summary>
		/// 
		/// </summary>
		static private System.Threading.Semaphore m_Semaphore = new System.Threading.Semaphore(1, 1);

		/// <summary>
		/// Returns available FileConvertor from ConcurrentBag collection if there is any.
		/// Otherwise create one and return it.
		/// </summary>
		/// <returns></returns>
        static public FileConverter GetObject()
        {
			m_Semaphore.WaitOne();
            FileConverter fileConvertor;
            if (!m_ConvertorInstances.TryTake(out fileConvertor))
				fileConvertor = new FileConverter();
			return fileConvertor;
        }
		
		/// <summary>
		/// Return used FileConvertor to ConcurrentBag collection.
		/// </summary>
		/// <param name="item"></param>
		static public void PutObject(FileConverter item)
        {
            m_ConvertorInstances.Add(item);
			m_Semaphore.Release();
        }
	}
}
