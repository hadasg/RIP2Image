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

namespace RIP2Image
{
	/// <summary>
	/// Manage FileConverter instances shard resource as thread safe. 
	/// </summary>
	internal class InstancesManager
	{
		public enum ConversionType { PDF2JPG, PDF2EPS, PDF2PNG, PDF2PNGSingle, EPS2PDF, PDF2LowResPDF, EPS2LowResPDF };

		/// <summary>
		/// private constructor
		/// </summary>
		private InstancesManager() { }

		/// <summary>
		/// Converter instances shard resource - thread safe.
		/// </summary>
		static private ConcurrentBag<FileConverter> m_PDF2JPGConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_PDF2EPSConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_PDF2PNGConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_PDF2PNGSingleConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_EPS2PDFConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_EPS2LowResPDFConverterInstances = new ConcurrentBag<FileConverter>();
		static private ConcurrentBag<FileConverter> m_PDF2LowResPDFConverterInstances = new ConcurrentBag<FileConverter>();

		/// <summary>
		/// Get converter instances by conversion type.
		/// </summary>
		/// <param name="inConversionType"></param>
		/// <returns></returns>
		static private ConcurrentBag<FileConverter> GetConverterInstancesByType(ConversionType inConversionType)
		{
			switch (inConversionType)
			{
				case ConversionType.PDF2PNG:
					return m_PDF2PNGConverterInstances;
				case ConversionType.PDF2PNGSingle:
					return m_PDF2PNGSingleConverterInstances;
				case ConversionType.PDF2JPG:
					return m_PDF2JPGConverterInstances;
				case ConversionType.PDF2EPS:
					return m_PDF2EPSConverterInstances;
				case ConversionType.EPS2PDF:
					return m_EPS2PDFConverterInstances;
				case ConversionType.EPS2LowResPDF:
					return m_EPS2LowResPDFConverterInstances;
				case ConversionType.PDF2LowResPDF:
					return m_PDF2LowResPDFConverterInstances;
				default:
					return null;
			}
		}

		/// <summary>
		/// Returns available FileConvertor from ConcurrentBag collection by conversion type (if there is any).
		/// Otherwise create one and return it.
		/// </summary>
		/// <param name="inConversionType"></param>
		/// <returns></returns>
		static public FileConverter GetObject(ConversionType inConversionType)
		{
			FileConverter fileConverter;
			if (!GetConverterInstancesByType(inConversionType).TryTake(out fileConverter))
				fileConverter = new FileConverter(inConversionType);
			return fileConverter;
		}

		/// <summary>
		/// Return used FileConvertor to ConcurrentBag collection by conversion type.
		/// </summary>
		/// <param name="inConversionType"></param>
		/// <param name="inFileConvertor"></param>
		static public void PutObject(ConversionType inConversionType, FileConverter inFileConvertor)
		{
			GetConverterInstancesByType(inConversionType).Add(inFileConvertor);
		}

		/// <summary>
		/// Delete dynamic loading dlls and their directory.
		/// </summary>
		static public void DeleteDynamicLoadingDLL()
		{
			// Cleanup converter instances, which also delete their dll file member. 
			FileConverter fileConverter;
			foreach (ConversionType type in Enum.GetValues(typeof(ConversionType)))
				while (GetConverterInstancesByType(type).TryTake(out fileConverter))
					fileConverter.Cleanup();

			// Delete dlls's directory
			GhostscriptWrapper.DeleteDllDirectory();
		}
	}
}
