/*******************************************************************************
Description:
	Ghostscript Service file converter.

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
using System.Linq;
using System.Text;
using System.IO;

namespace GhostscriptService
{
	/// <summary>
	/// Convert file type using Ghostscript.
	/// </summary>
	internal class FileConverter
	{

	#region Class Members

		/// <summary>
		/// Ghostscript wrapper for use Ghostscript services. 
		/// </summary>
		GhostscriptWrapper m_GhostscriptWrapper = null;

	#endregion

	#region Methods
		/// <summary>
		/// Constructor.
		/// </summary>
		public FileConverter()
		{
			Init();		  
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters.
		/// </summary>
		public void Init()
		{
			Cleanup();

			// Parameters creation.
			string[] parameters = new string[4];
			parameters[0] = "this is gs command .exe name";		// Ghostscript exe command.
			parameters[1] = "-dNOPAUSE";						// Do not prompt and pause for each page
			parameters[2] = "-sDEVICE=jpeg";					// what kind of export format i should provide. For PNG use "png16m"

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper(parameters);
		}

		/// <summary>
		/// Cleanup GhostscriptWrapper.
		/// </summary>
		public void Cleanup()
		{
			if (m_GhostscriptWrapper != null)
			{
				m_GhostscriptWrapper.Cleanup();
				m_GhostscriptWrapper = null;
			}
			
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~FileConverter()
		{
			Cleanup();
		}

		/// <summary>
		/// Convert file type.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <returns>True if conversion succeeded</returns>
		public bool Convert(string inPathFileToConvert, string inOutputFileFullPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			StringBuilder ConvertPDF2JPGCommand = new StringBuilder();

			// Determine rasterisation graphic quality - values are 1, 2 or 4.
			ConvertPDF2JPGCommand.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine rasterisation text quality - values are 1, 2 or 4.
			ConvertPDF2JPGCommand.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine file quality the range is 0-100.
			ConvertPDF2JPGCommand.Append("mark /JPEGQ " + inQuality + " currentdevice putdeviceprops ");

			// Determine file resolution.
			ConvertPDF2JPGCommand.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

			// Determine new file name.
			ConvertPDF2JPGCommand.Append("<< /OutputFile (" + inOutputFileFullPath + ") >> setpagedevice ");

			// Convert file type.
			ConvertPDF2JPGCommand.Append("(" + inPathFileToConvert + ") run ");

			return m_GhostscriptWrapper.RunCommand(ConvertPDF2JPGCommand.ToString()); 
		}

		

	#endregion

	}
}
