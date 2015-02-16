﻿/*******************************************************************************
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
using System.Linq;
using System.Text;
using System.IO;

namespace RIP2Jmage
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
		/// Constructor - init FileConverter by convertion type.
		/// </summary>
		public FileConverter(InstancesManager.ConversionType inConvertionType)
		{
			switch (inConvertionType)
			{
                case InstancesManager.ConversionType.PDF2PNG:
                    InitPDF2PNGConversion();
                    break;
				case InstancesManager.ConversionType.PDF2JPG:
					InitPDF2JPGConversion();
					break;
				case InstancesManager.ConversionType.PDF2EPS:
					InitPDF2EPSConversion();			
					break;
				default:
					break;
			}	  
		}

        /// <summary>
        /// Initialize GhostscriptWrapper with relevant parameters for PDF2PNG conversion.
        /// </summary>
        public void InitPDF2PNGConversion()
        {
            Cleanup();

            // Parameters creation.
            string[] parameters = new string[4];
            parameters[0] = "this is gs command .exe name";		// Ghostscript exe command.
            parameters[1] = "-dNOPAUSE";						// Do not prompt and pause for each page
            parameters[2] = "-sDEVICE=pngalpha";				// what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
            parameters[3] = "-dDOINTERPOLATE";

            // Create the Ghostscript wrapper.
            m_GhostscriptWrapper = new GhostscriptWrapper(parameters);
        }

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		public void InitPDF2JPGConversion()
		{
			Cleanup();

			// Parameters creation.
			string[] parameters = new string[4];
			parameters[0] = "this is gs command .exe name";		// Ghostscript exe command.
			parameters[1] = "-dNOPAUSE";						// Do not prompt and pause for each page
			parameters[2] = "-sDEVICE=jpeg";					// what kind of export format i should provide.
			parameters[3] = "-dDOINTERPOLATE";

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper(parameters);
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2EPS conversion.
		/// </summary>
		public void InitPDF2EPSConversion()
		{
			// Need to implement in the future.
			// GS has a bug that causes the created EPS file to be lock until quit command called.
			// In addition, this bug prevents writing for more than one EPS file in a single run.
			// Therefor, it's impossible to reuse a GS instance many time.
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
        /// Convert PDF to JPG.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFolderPath"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
        /// <returns>True if conversion succeeded</returns>
		public bool ConvertPDF2JPG(string inPathFileToConvert, string inOutputFolderPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
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
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.jpg";
			ConvertPDF2JPGCommand.Append("<< /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			ConvertPDF2JPGCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			return m_GhostscriptWrapper.RunCommand(ConvertPDF2JPGCommand.ToString()); 
		}

        /// <summary>
        /// Convert PDF to PNG.
        /// </summary>
        /// <param name="inPathFileToConvert"></param>
        /// <param name="inOutputFolderPath"></param>
        /// <param name="inResolutionX"></param>
        /// <param name="inResolutionY"></param>
        /// <param name="inGraphicsAlphaBitsValue"></param>
        /// <param name="inTextAlphaBitsValue"></param>
        /// <returns>True if conversion succeeded</returns>
        public bool ConvertPDF2PNG(string inPathFileToConvert, string inOutputFolderPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue)
        {
            StringBuilder ConvertPDF2PNGCommand = new StringBuilder();

             // Determine rasterisation graphic quality - values are 1, 2 or 4.
             ConvertPDF2PNGCommand.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");
 
             // Determine rasterisation text quality - values are 1, 2 or 4.
             ConvertPDF2PNGCommand.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

            // Determine file resolution.
            ConvertPDF2PNGCommand.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

            // Determine new file name.
            string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.png";
            ConvertPDF2PNGCommand.Append("<< /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

            // Convert file type.
            ConvertPDF2PNGCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

            return m_GhostscriptWrapper.RunCommand(ConvertPDF2PNGCommand.ToString());
        }

		/// <summary>
		/// Convert PDF to EPS.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <param name="inFirstPageToConvert"></param>
		/// <param name="inLastPageToConvert"></param>
		/// <returns></returns>
		public bool ConvertPDF2EPS(string inPathFileToConvert, string inOutputFileFullPath, double inFirstPageToConvert, double inLastPageToConvert)
		{
			// Parameters creation.
			string[] parameters = new string[10];
			parameters[0] = "this is gs command .exe name";								// Ghostscript exe command.
			parameters[1] = "-dNOPAUSE";												// Do not prompt and pause for each page
			parameters[2] = "-dBATCH";													// Terminate when accomplish.
			parameters[3] = "-dGraphicsAlphaBits=2";									
			parameters[4] = "-dTextAlphaBits=4";
			parameters[5] = "-dFirstPage=" + Convert.ToInt32(inFirstPageToConvert);		// First page to convert in the PDF.
			parameters[6] = "-dLastPage=" + Convert.ToInt32(inLastPageToConvert);		// Last page to convert in the PDF.
			parameters[7] = "-sDEVICE=eps2write";										// Device name.
			parameters[8] = "-sOutputFile=" + inOutputFileFullPath;						// Where to write the output.
			parameters[9] = inPathFileToConvert;										// File to convert.

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper(parameters);

			Cleanup();

			return true;
		}
		

	#endregion

	}
}
