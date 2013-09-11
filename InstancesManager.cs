/*******************************************************************************
	RIP2Image is a program that efficiently converts formats such as PDF or Postscript to image formats such as Jpeg or PNG.
    Copyright (C) 2013 XMPie Ltd.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

	This license covers only the RIP2Image files and not any file that RIP2Image links against or otherwise uses.
 
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace RIP2Jmage
{
	/// <summary>
	/// Manage FileConverter instances shard resource as thread safe. 
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
		static private ConcurrentBag<FileConverter> m_ConverterInstances = new ConcurrentBag<FileConverter>();

		/// <summary>
		/// Returns available FileConvertor from ConcurrentBag collection if there is any.
		/// Otherwise create one and return it.
		/// </summary>
		/// <returns></returns>
        static public FileConverter GetObject()
        {
            FileConverter fileConverter;
            if (!m_ConverterInstances.TryTake(out fileConverter))
				fileConverter = new FileConverter();
			return fileConverter;
        }
		
		/// <summary>
		/// Return used FileConvertor to ConcurrentBag collection.
		/// </summary>
		/// <param name="inFileConvertor"></param>
		static public void PutObject(FileConverter inFileConvertor)
        {
            m_ConverterInstances.Add(inFileConvertor);
        }

		/// <summary>
		/// Delete dynamic loading dlls and their directory.
		/// </summary>
		static public void DeleteDynamicLoadingDLL()
		{
			// Cleanup converter instances, which also delete their dll file member. 
			FileConverter fileConverter;
			while (m_ConverterInstances.TryTake(out fileConverter))
				fileConverter.Cleanup();

			// Delete dlls's directory
			GhostscriptWrapper.DeleteDllDirectory();
		}
	}
}
