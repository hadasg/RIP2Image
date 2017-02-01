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
using System.Linq;
using System.Text;
using System.IO;

namespace RIP2Image
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
		GhostscriptWrapper m_GhostscriptWrapper;

		/// <summary>
		/// the number of concurrent conversions
		/// </summary>
		int m_NumConcurrentConversions;

		/// <summary>
		/// the conversion type
		/// </summary>
		InstancesManager.ConversionType m_ConversionType;

		/// <summary>
		/// boolean indicating if the last conversion succeded
		/// </summary>
		bool m_LastRunSuccedded;

		#endregion

		#region Methods
		/// <summary>
		/// Constructor
		/// </summary>
		public FileConverter(InstancesManager.ConversionType inConvertionType)
		{
			m_GhostscriptWrapper = null;
			m_ConversionType = inConvertionType;
			m_NumConcurrentConversions = 0;
			m_LastRunSuccedded = true;
		}

		/// <summary>
		/// init if needed
		/// </summary>
		public void InitIfNeeded()
		{
			if (m_LastRunSuccedded && m_NumConcurrentConversions < 256 && m_GhostscriptWrapper != null)
			{
				++m_NumConcurrentConversions;
				return;
			}

			switch (m_ConversionType)
			{
				case InstancesManager.ConversionType.PDF2PNG:
					InitPDF2PNGConversion();
					break;
				case InstancesManager.ConversionType.PDF2PNGSingle:
					InitPDF2PNGSingleConversion();
					break;
				case InstancesManager.ConversionType.PDF2JPG:
					InitPDF2JPGConversion();
					break;
				case InstancesManager.ConversionType.PDF2EPS:
					InitPDF2EPSConversion();
					break;
				case InstancesManager.ConversionType.PDF2LowResPDF:
					InitPDF2LowResPDFConversion();
					break;
				case InstancesManager.ConversionType.EPS2PDF:
					InitEPS2PDFConversion();
					break;
				case InstancesManager.ConversionType.EPS2LowResPDF:
					InitEPS2LowResPDFConversion();
					break;
				default:
					break;
			}
			m_LastRunSuccedded = true;
			++m_NumConcurrentConversions;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2PNG conversion.
		/// </summary>
		public void InitPDF2PNGConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                        // Do not prompt and pause for each page.
			parameters.Add("-sDEVICE=pngalpha");                // what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
			parameters.Add("-dDOINTERPOLATE");

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2PNGSingle conversion.
		/// This method convert only the first page of the PDF to PNG.
		/// </summary>
		public void InitPDF2PNGSingleConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                     // Do not prompt and pause for each page
			parameters.Add("-dFirstPage=1");                 // Convert only the first page of the PDF to PNG.
			parameters.Add("-dLastPage=1");                      // Convert only the first page of the PDF to PNG.
			parameters.Add("-sDEVICE=pngalpha");             // what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
			parameters.Add("-dDOINTERPOLATE");

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		public void InitPDF2JPGConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                     // Do not prompt and pause for each page
			parameters.Add("-sDEVICE=jpeg");                 // what kind of export format i should provide.
			parameters.Add("-dDOINTERPOLATE");

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		public void InitPDF2LowResPDFConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                                                // Do not prompt and pause for each page
			//parameters.Add("-dPDFSETTINGS=/screen");
			parameters.Add("-sDEVICE=pdfwrite");                                        // Device name.
			parameters.Add("-r72x72");
			parameters.Add("-dDownsampleColorImages=true");
			parameters.Add("-dDownsampleGrayImages=true");
			parameters.Add("-dDownsampleMonoImages=true");
			parameters.Add("-dColorImageResolution=72");
			parameters.Add("-dGrayImageResolution=72");
			parameters.Add("-dMonoImageResolution=72");
			parameters.Add("-dCompatibilityLevel=1.4");
			parameters.Add("-dDetectDuplicateImages=true");
			parameters.Add("-dAutoRotatePages=/None");
			parameters.Add("-sOutputFile=" + "rip2image_junk_helper");                  // we must set the output at init stage, so we put a junk file, just for the init to successed

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
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
			/*
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                                                // Do not prompt and pause for each page
			parameters.Add("-sDEVICE=eps2write");                                       // Device name.
			parameters.Add("-sOutputFile=" + "rip2image_junk_helper");                  // we must set the output at init stage, so we put a junk file, just for the init to successed

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
			*/
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for EPS2PDF conversion.
		/// </summary>
		public void InitEPS2PDFConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                                                // Do not prompt and pause for each page
			parameters.Add("-dPDFSETTINGS=/printer");
			parameters.Add("-sDEVICE=pdfwrite");                                        // Device name.
			parameters.Add("-dEPSFitPage");
			parameters.Add("-dEPSCrop");
			parameters.Add("-dCompatibilityLevel=1.4");
			parameters.Add("-dDetectDuplicateImages=true");
			parameters.Add("-dAutoRotatePages=/None");
			parameters.Add("-sOutputFile=" + "rip2image_junk_helper");                  // we must set the output at init stage, so we put a junk file, just for the init to successed

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for EPS2LowResPDF conversion.
		/// </summary>
		public void InitEPS2LowResPDFConversion()
		{
			Cleanup();

			// Parameters creation.
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                                                // Do not prompt and pause for each page
			//parameters.Add("-dPDFSETTINGS=/screen");
			parameters.Add("-sDEVICE=pdfwrite");                                        // Device name.
			parameters.Add("-dEPSFitPage");
			parameters.Add("-dEPSCrop");
			parameters.Add("-r72x72");
			parameters.Add("-dDownsampleColorImages=true");
			parameters.Add("-dDownsampleGrayImages=true");
			parameters.Add("-dDownsampleMonoImages=true");
			parameters.Add("-dColorImageResolution=72");
			parameters.Add("-dGrayImageResolution=72");
			parameters.Add("-dMonoImageResolution=72");
			parameters.Add("-dCompatibilityLevel=1.4");
			parameters.Add("-dDetectDuplicateImages=true");
			parameters.Add("-dAutoRotatePages=/None");
			parameters.Add("-sOutputFile=" + "rip2image_junk_helper");                  // we must set the output at init stage, so we put a junk file, just for the init to successed

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			m_GhostscriptWrapper.Init(parameters.ToArray());
		}

		/// <summary>
		/// Cleanup GhostscriptWrapper.
		/// </summary>
		public void Cleanup()
		{
			m_NumConcurrentConversions = 0;
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
			InitIfNeeded();

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

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertPDF2JPGCommand.ToString());

			return m_LastRunSuccedded;
		}


		/// <summary>
		/// Convert PDF to PNG - only the first page.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFilePath"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <returns>True if conversion succeeded</returns>
		public bool ConvertPDF2PNGSingle(string inPathFileToConvert, string inOutputFilePath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue)
		{
			InitIfNeeded();

			StringBuilder ConvertPDF2PNGSingleCommand = new StringBuilder();

			// Determine rasterisation graphic quality - values are 1, 2 or 4.
			ConvertPDF2PNGSingleCommand.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine rasterisation text quality - values are 1, 2 or 4.
			ConvertPDF2PNGSingleCommand.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine file resolution.
			ConvertPDF2PNGSingleCommand.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

			// Determine new file name.
			ConvertPDF2PNGSingleCommand.Append("<< /OutputFile (" + inOutputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			ConvertPDF2PNGSingleCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertPDF2PNGSingleCommand.ToString());

			return m_LastRunSuccedded;
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
			InitIfNeeded();

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

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertPDF2PNGCommand.ToString());

			return m_LastRunSuccedded;
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
			List<string> parameters = new List<string>();
			parameters.Add("this is gs command .exe name");                             // Ghostscript exe command.
			parameters.Add("-dNOPAUSE");                                              // Do not prompt and pause for each page
			parameters.Add("-dBATCH");                                                  // Terminate when accomplish.
			parameters.Add("-dFirstPage=" + Convert.ToInt32(inFirstPageToConvert));     // First page to convert in the PDF.
			parameters.Add("-dLastPage=" + Convert.ToInt32(inLastPageToConvert));       // Last page to convert in the PDF.
			parameters.Add("-sDEVICE=eps2write");                                       // Device name.
			parameters.Add("-sOutputFile=" + inOutputFileFullPath);                     // Where to write the output.
			parameters.Add(inPathFileToConvert);                                        // File to convert.

			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			bool convertResult = m_GhostscriptWrapper.Init(parameters.ToArray());

			Cleanup();

			return convertResult;

		}

		/// <summary>
		/// Convert EPS to PDF.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <returns></returns>
		public bool ConvertEPS2PDF(string inPathFileToConvert, string inOutputFileFullPath)
		{
			InitIfNeeded();

			StringBuilder ConvertEPS2PDFCommand = new StringBuilder();

			// Determine new file name.
			ConvertEPS2PDFCommand.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			ConvertEPS2PDFCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertEPS2PDFCommand.ToString());

			// we need to change back the output to the just file, so the output file will be finalized and unlocked
			if (m_LastRunSuccedded)
				m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand("<< /OutputFile (rip2image_junk_helper) >> setpagedevice ");

			return m_LastRunSuccedded;
		}

		/// <summary>
		/// Convert EPS to PDF.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <returns></returns>
		public bool ConvertEPS2LowResPDF(string inPathFileToConvert, string inOutputFileFullPath)
		{
			InitIfNeeded();

			StringBuilder ConvertEPS2LowResPDFCommand = new StringBuilder();

			// Determine new file name.
			ConvertEPS2LowResPDFCommand.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			ConvertEPS2LowResPDFCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertEPS2LowResPDFCommand.ToString());

			// we need to change back the output to the just file, so the output file will be finalized and unlocked
			if (m_LastRunSuccedded)
				m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand("<< /OutputFile (rip2image_junk_helper) >> setpagedevice ");

			return m_LastRunSuccedded;
		}

		/// <summary>
		/// Convert EPS to PDF.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <returns></returns>
		public bool ConvertPDF2LowResPDF(string inPathFileToConvert, string inOutputFileFullPath)
		{
			InitIfNeeded();

			StringBuilder ConvertPDF2JPGCommand = new StringBuilder();

			// Determine new file name.
			ConvertPDF2JPGCommand.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			ConvertPDF2JPGCommand.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand(ConvertPDF2JPGCommand.ToString());

			// we need to change back the output to the just file, so the output file will be finalized and unlocked
			if (m_LastRunSuccedded)
				m_LastRunSuccedded = m_GhostscriptWrapper.RunCommand("<< /OutputFile (rip2image_junk_helper) >> setpagedevice ");

			return m_LastRunSuccedded;
		}

		#endregion

	}
}
