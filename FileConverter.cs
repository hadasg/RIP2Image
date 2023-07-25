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
using System.IO;
using System.Text;
using static RIP2Image.GhostscriptWrapper;

namespace RIP2Image
{
	/// <summary>
	/// Convert file type using Ghostscript.
	/// </summary>
	internal class FileConverter : IDisposable
	{
		/// <summary>
		/// Ghostscript wrapper for use Ghostscript services. 
		/// </summary>
		private GhostscriptWrapper m_GhostscriptWrapper = null;

		/// <summary>
		/// the number of concurrent conversions
		/// </summary>
		private int m_NumSequentialConversions = 0;

		/// <summary>
		/// the conversion type
		/// </summary>
		private InstancesManager.ConversionType m_ConversionType;

		/// <summary>
		/// boolean indicating if the last conversion succeeded
		/// </summary>
		private bool m_LastRunSuccedded = true;

		#region Methods
		/// <summary>
		/// Constructor
		/// </summary>
		public FileConverter(InstancesManager.ConversionType inConvertionType)
		{
			m_ConversionType = inConvertionType;
		}

		/// <summary>
		/// dispose implementation
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			Cleanup();
		}

		/// <summary>
		/// dispose implementation
		/// </summary>
		public void Dispose()
		{
			// Dispose of unmanaged resources.
			Dispose(true);
			// Suppress finalization.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// init if needed
		/// </summary>
		private bool InitIfNeeded()
		{
			++m_NumSequentialConversions;
			
			if (m_LastRunSuccedded && m_NumSequentialConversions < 256 && m_GhostscriptWrapper != null)
				return true;

			Cleanup();

			switch (m_ConversionType)
			{
				case InstancesManager.ConversionType.PDF2PNG:
					return InitPDF2PNGConversion();
				case InstancesManager.ConversionType.PDF2PNGSingle:
					return InitPDF2PNGSingleConversion();
				case InstancesManager.ConversionType.PDF2JPG:
					return InitPDF2JPGConversion();
				case InstancesManager.ConversionType.PDF2EPS:
					return InitPDF2EPSConversion();
				case InstancesManager.ConversionType.PDF2LowResPDF:
					return InitPDF2LowResPDFConversion();
				case InstancesManager.ConversionType.EPS2PDF:
					return InitEPS2PDFConversion();
				case InstancesManager.ConversionType.EPS2LowResPDF:
					return InitEPS2LowResPDFConversion();
				case InstancesManager.ConversionType.PDF2GrayscalePDF:
					return InitPDF2GrayscalePDFConversion();
			}

			return false;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2PNG conversion.
		/// </summary>
		private bool InitPDF2PNGConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",						    // Ghostscript exe command.
				"-dNOPAUSE",                            // Do not prompt and pause for each page.
				"-dNOSAFER",                            // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-sDEVICE=pngalpha",                    // what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
				"-dDOINTERPOLATE"
				);

			if(code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2PNGConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2PNGSingle conversion.
		/// This method convert only the first page of the PDF to PNG.
		/// </summary>
		private bool InitPDF2PNGSingleConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",								// Ghostscript exe command.
				"-dNOPAUSE",								// Do not prompt and pause for each page.
				"-dNOSAFER",								// This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-dFirstPage=1",                            // Convert only the first page of the PDF to PNG.
				"-dLastPage=1",                             // Convert only the first page of the PDF to PNG.
				"-sDEVICE=pngalpha",						// what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
				"-dDOINTERPOLATE",
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2PNGSingleConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		private bool InitPDF2JPGConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-sDEVICE=jpeg",                            // what kind of export format i should provide.
				"-dDOINTERPOLATE"
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2JPGConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		private bool InitPDF2LowResPDFConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				//"-dPDFSETTINGS=/screen",
				"\"-r72x72\"",
				"-dDownsampleColorImages=true",
				"-dDownsampleGrayImages=true",
				"-dDownsampleMonoImages=true",
				"-dColorImageResolution=72",
				"-dGrayImageResolution=72",
				"-dMonoImageResolution=72",
				"-dCompatibilityLevel=1.4",
				"-dDetectDuplicateImages=true",
				"-dAutoRotatePages=/None",
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2LowResPDFConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2JPG conversion.
		/// </summary>
		private bool InitPDF2GrayscalePDFConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				//"-dPDFSETTINGS=/screen",
				"-sDEVICE=pdfwrite",                        // Device name.
				"-sProcessColorModel=DeviceGray",
				"-sColorConversionStrategy=Gray",
				"-dOverrideICC",
				"-dCompatibilityLevel=1.4",
				"-dDetectDuplicateImages=true",
				"-dAutoRotatePages=/None",
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2GrayscalePDFConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for PDF2EPS conversion.
		/// </summary>
		private bool InitPDF2EPSConversion()
		{
			// Need to implement in the future.
			// GS has a bug that causes the created EPS file to be lock until quit command called.
			// In addition, this bug prevents writing for more than one EPS file in a single run.
			// Therefor, it's impossible to reuse a GS instance many time.

			/*
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-sDEVICE=eps2write",                       // Device name.
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitPDF2EPSConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}
			*/

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for EPS2PDF conversion.
		/// </summary>
		private bool InitEPS2PDFConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-dPDFSETTINGS=/printer",
				"-sDEVICE=pdfwrite",                        // Device name.
				"-dEPSFitPage",
				"-dEPSCrop",
				"-dCompatibilityLevel=1.4",
				"-dDetectDuplicateImages=true",
				"-dAutoRotatePages=/None",
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitEPS2PDFConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Initialize GhostscriptWrapper with relevant parameters for EPS2LowResPDF conversion.
		/// </summary>
		private bool InitEPS2LowResPDFConversion()
		{
			// Create the Ghostscript wrapper.
			m_GhostscriptWrapper = new GhostscriptWrapper();
			gs_error_type code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                              // Ghostscript exe command.
				"-dNOPAUSE",                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				//"-dPDFSETTINGS=/screen",
				"-sDEVICE=pdfwrite",                        // Device name.
				"-dEPSFitPage",
				"-dEPSCrop",
				"-r72x72",
				"-dDownsampleColorImages=true",
				"-dDownsampleGrayImages=true",
				"-dDownsampleMonoImages=true",
				"-dColorImageResolution=72",
				"-dGrayImageResolution=72",
				"-dMonoImageResolution=72",
				"-dCompatibilityLevel=1.4",
				"-dDetectDuplicateImages=true",
				"-dAutoRotatePages=/None",
				"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
				);

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.InitEPS2LowResPDFConversion - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Cleanup GhostscriptWrapper.
		/// </summary>
		private void Cleanup()
		{
			m_NumSequentialConversions = 0;
			m_LastRunSuccedded = true;
			if (m_GhostscriptWrapper != null)
			{
				m_GhostscriptWrapper.Dispose();
				m_GhostscriptWrapper = null;
			}
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
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine file quality the range is 0-100.
			command.Append("mark /JPEGQ " + inQuality + " currentdevice putdeviceprops ");

			// Determine file resolution.
			command.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.jpg";
			command.Append("<< /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if(code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded =  false;
				Logger.LogError("FileConverter.ConvertPDF2JPG - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}
			

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
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine file resolution.
			command.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

			// Determine new file name.
			command.Append("<< /OutputFile (" + inOutputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			// we need to change back the output to the just file, so the output file will be finalized and unlocked.
			command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertPDF2PNGSingle - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

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
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append("mark /GraphicsAlphaBits " + inGraphicsAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append("mark /TextAlphaBits " + inTextAlphaBitsValue + " currentdevice putdeviceprops ");

			// Determine file resolution.
			command.Append("<< /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "] >> setpagedevice ");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.png";
			command.Append("<< /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertPDF2PNG - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

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
			using(GhostscriptWrapper ghostscriptWrapper = new GhostscriptWrapper())
			{
				gs_error_type code = ghostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",												// Ghostscript exe command.
				"-dNOPAUSE",												// Do not prompt and pause for each page.
				"-dNOSAFER",												// This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-dBATCH",													// Terminate when accomplish.
				"-dFirstPage=" + Convert.ToInt32(inFirstPageToConvert),     // First page to convert in the PDF.
				"-dLastPage=" + Convert.ToInt32(inLastPageToConvert),       // Last page to convert in the PDF.
				"-sDEVICE=eps2write",                                       // Device name.
				"-sOutputFile=" + inOutputFileFullPath,						// Where to write the output.
				inPathFileToConvert											// File to convert.
				);

				if (code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2EPS - gsapi_init_with_args return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Convert EPS to PDF.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <returns></returns>
		public bool ConvertEPS2PDF(string inPathFileToConvert, string inOutputFileFullPath)
		{
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine new file name.
			command.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			// we need to change back the output to the just file, so the output file will be finalized and unlocked.
			command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertEPS2PDF - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

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
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine new file name.
			command.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			// we need to change back the output to the just file, so the output file will be finalized and unlocked.
			command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertEPS2LowResPDF - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

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
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Make sure proofing to PDF doesn't use independent color spaces (ICCBased) and always renders to RGB
			command.Append("<< -dColorConversionStrategy=/RGB");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			// we need to change back the output to the just file, so the output file will be finalized and unlocked.
			command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertPDF2LowResPDF - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

			return m_LastRunSuccedded;
		}

		/// <summary>
		/// Convert PDF to Grayscale PDF.
		/// </summary>
		/// <param name="inPathFileToConvert"></param>
		/// <param name="inOutputFileFullPath"></param>
		/// <returns></returns>
		public bool ConvertPDF2GrayscalePDF(string inPathFileToConvert, string inOutputFileFullPath)
		{
			m_LastRunSuccedded = InitIfNeeded();
			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// Determine new file name.
			command.Append("<< /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			// we need to change back the output to the just file, so the output file will be finalized and unlocked.
			command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				m_LastRunSuccedded = false;
				Logger.LogError("FileConverter.ConvertPDF2GrayscalePDF - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
			}
			else
			{
				m_LastRunSuccedded = true;
			}

			return m_LastRunSuccedded;
		}

		#endregion

	}
}
