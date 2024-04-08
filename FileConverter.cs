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
using System.Configuration;
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

		private bool DetectDuplicateImages
		{
			get
			{
				return ConfigurationManager.AppSettings["DetectDuplicateImages"] == "true";
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public FileConverter(InstancesManager.ConversionType inConvertionType)
		{
			m_ConversionType = inConvertionType;
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
		/// establish need for initialization
		/// </summary>
		private bool IsNeedInitialization()
		{
			++m_NumSequentialConversions;

			if (m_LastRunSuccedded && m_NumSequentialConversions < 256 && m_GhostscriptWrapper != null)
				return false;

			Cleanup();

			return true;
		}

		/// <summary>
		/// Run a command on dummy file, so output file is changed and valid
		/// </summary>
		/// <param name="inputFile">the inpur file</param>
		/// <param name="outputFile">the output file, null to use the default</param>
		/// <returns>true if successfully</returns>
		private bool DummyFileOutput(bool inSetOutput)
		{
			try
			{
				if (inSetOutput && File.Exists(m_GhostscriptWrapper.GSDummyOutputFile))
					File.Delete(m_GhostscriptWrapper.GSDummyOutputFile);
			}
			catch (System.Exception ex)
			{
				Logger.LogError("FileConverter.DummyFileOutput - {0} for Instance {1}", Logger.GetMostInnerMessage(ex), m_GhostscriptWrapper.InstanceId);
			}

			StringBuilder command = new StringBuilder();

			if (inSetOutput)
				command.Append("<< /OutputFile (" + m_GhostscriptWrapper.GSDummyOutputFile.Replace("\\", "\\\\") + ") >> setpagedevice ");

			// Convert file type.
			command.Append("(" + m_GhostscriptWrapper.GSDummyInputFile.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.DummyFileOutput - gsapi_run_string_with_length return error {0} for Instance {1}", code.ToString(), m_GhostscriptWrapper.InstanceId);
				return false;
			}

			return true;
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-sDEVICE=jpeg",                            // what kind of export format i should provide.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2JPG - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file quality the range is 0-100.
			command.Append(" /JPEGQ " + inQuality);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.jpg";
			command.Append(" /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if(code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2JPG - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded =  false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
			return m_LastRunSuccedded;
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
		public bool ConvertPDF2GrayscaleJPG(string inPathFileToConvert, string inOutputFolderPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-sDEVICE=jpeggray",                        // what kind of export format i should provide.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2GrayscaleJPG - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file quality the range is 0-100.
			command.Append(" /JPEGQ " + inQuality);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.jpg";
			command.Append(" /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2GrayscaleJPG - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-dFirstPage=1",                            // Convert only the first page of the PDF to PNG.
					"-dLastPage=1",                             // Convert only the first page of the PDF to PNG.
					"-sDEVICE=pngalpha",                        // what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2PNGSingle - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// setting the output device
			command.Append(" /OutputDevice /pngalpha");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2PNGSingle - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
		public bool ConvertPDF2GrayscalePNGSingle(string inPathFileToConvert, string inOutputFilePath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue)
		{
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-dFirstPage=1",                            // Convert only the first page of the PDF to PNG.
					"-dLastPage=1",                             // Convert only the first page of the PDF to PNG.
					"-sDEVICE=pnggray",                         // what kind of export format i should provide.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2GrayscalePNGSingle - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2GrayscalePNGSingle - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                          // Ghostscript exe command.
					"-dNOPAUSE",                            // Do not prompt and pause for each page.
					"-dNOSAFER",                            // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-sDEVICE=pngalpha",                    // what kind of export format i should provide, in this case "pngalpha" for transparent PNG.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2PNG - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.png";
			command.Append(" /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2PNG - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
		public bool ConvertPDF2GrayscalePNG(string inPathFileToConvert, string inOutputFolderPath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue)
		{
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                          // Ghostscript exe command.
					"-dNOPAUSE",                            // Do not prompt and pause for each page.
					"-dNOSAFER",                            // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-sDEVICE=pnggray",                    // what kind of export format i should provide.
					"-dDOINTERPOLATE"
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2GrayscalePNG - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine rasterization graphic quality - values are 1, 2 or 4.
			command.Append(" /GraphicsAlphaBits " + inGraphicsAlphaBitsValue);

			// Determine rasterization text quality - values are 1, 2 or 4.
			command.Append(" /TextAlphaBits " + inTextAlphaBitsValue);

			// Determine file resolution.
			command.Append(" /HWResolution [" + inResolutionX.ToString() + " " + inResolutionY.ToString() + "]");

			// Determine new file name.
			string outputFilePath = inOutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(inPathFileToConvert) + "-%d.png";
			command.Append(" /OutputFile (" + outputFilePath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2GrayscalePNG - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			// Need to implement in the future.
			// GS has a bug that causes the created EPS file to be lock until exit command called.
			// So we employ deferent strategy

			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
			}

			gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
				"gswin64.exe",                                              // Ghostscript exe command.
				"-dNOPAUSE",                                                // Do not prompt and pause for each page.
				"-dNOSAFER",                                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
				"-dBATCH",                                                  // Terminate when accomplish.
				"-dFirstPage=" + Convert.ToInt32(inFirstPageToConvert),     // First page to convert in the PDF.
				"-dLastPage=" + Convert.ToInt32(inLastPageToConvert),       // Last page to convert in the PDF.
				"-sDEVICE=eps2write",                                       // Device name.
				"-sOutputFile=" + inOutputFileFullPath,                     // Where to write the output.
				inPathFileToConvert                                         // File to convert.
				);

			if (init_code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2EPS - gsapi_init_with_args return error {0} for Instance {1} for file {2}", init_code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			gs_error_type exit_code = m_GhostscriptWrapper.gsapi_exit();

			if (exit_code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2EPS - gsapi_init_with_args return error {0} for Instance {1} for file {2}", exit_code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.EPS);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
					"-dPDFSETTINGS=/printer",
					"-sDEVICE=pdfwrite",                        // Device name.
					"-dEPSFitPage",
					"-dEPSCrop",
					"-dCompatibilityLevel=1.5",
					DetectDuplicateImages ? "-dDetectDuplicateImages=true" : "-dDetectDuplicateImages=false",
					"-dAutoRotatePages=/None",
					"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertEPS2PDF - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
				else
				{
					m_LastRunSuccedded = DummyFileOutput(false);
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertEPS2PDF - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.EPS);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
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
					"-dCompatibilityLevel=1.5",
					DetectDuplicateImages ? "-dDetectDuplicateImages=true" : "-dDetectDuplicateImages=false",
					"-dAutoRotatePages=/None",
					"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertEPS2LowResPDF - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
				else
				{
					m_LastRunSuccedded = DummyFileOutput(false);
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertEPS2LowResPDF - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
																//"-dPDFSETTINGS=/screen",
					"-sDEVICE=pdfwrite",                        // Device name.
					"-r72x72",
					"-dDownsampleColorImages=true",
					"-dDownsampleGrayImages=true",
					"-dDownsampleMonoImages=true",
					"-dColorConversionStrategy=/RGB",
					"-dColorImageResolution=72",
					"-dGrayImageResolution=72",
					"-dMonoImageResolution=72",
					"-dCompatibilityLevel=1.5",
					DetectDuplicateImages ? "-dDetectDuplicateImages=true" : "-dDetectDuplicateImages=false",
					"-dAutoRotatePages=/None",
					"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2LowResPDF - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
				else
				{
					m_LastRunSuccedded = DummyFileOutput(false);
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2LowResPDF - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
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
			if (IsNeedInitialization())
			{
				m_LastRunSuccedded = true;
				m_GhostscriptWrapper = new GhostscriptWrapper(GSDummyInputType.PDF);
				gs_error_type init_code = m_GhostscriptWrapper.gsapi_init_with_args(
					"gswin64.exe",                              // Ghostscript exe command.
					"-dNOPAUSE",                                // Do not prompt and pause for each page.
					"-dNOSAFER",                                // This flag disables SAFER mode until the .setsafe procedure is run. This is intended for clients or scripts that cannot operate in SAFER mode. If Ghostscript is started with -dNOSAFER or -dDELAYSAFER, PostScript programs are allowed to read, write, rename or delete any files in the system that are not protected by operating system permissions.
																//"-dPDFSETTINGS=/screen",
					"-sDEVICE=pdfwrite",                        // Device name.
					"-sProcessColorModel=DeviceGray",
					"-sColorConversionStrategy=Gray",
					"-dOverrideICC",
					"-dCompatibilityLevel=1.5",
					DetectDuplicateImages ? "-dDetectDuplicateImages=true" : "-dDetectDuplicateImages=false",
					"-dAutoRotatePages=/None",
					"-sOutputFile=" + m_GhostscriptWrapper.GSDummyOutputFile   // we must set the output at init stage, so we put a junk file, just for the init to successes
					);

				if (init_code != gs_error_type.gs_error_ok)
				{
					Logger.LogError("FileConverter.ConvertPDF2GrayscalePDF - gsapi_init_with_args return error {0} for Instance {1}", init_code.ToString(), m_GhostscriptWrapper.InstanceId);
					m_LastRunSuccedded = false;
				}
				else
				{
					m_LastRunSuccedded = DummyFileOutput(false);
				}
			}

			if (!m_LastRunSuccedded)
				return false;

			StringBuilder command = new StringBuilder();

			// start the page device properties setup dictionary
			command.Append("<<");

			// Determine new file name.
			command.Append(" /OutputFile (" + inOutputFileFullPath.Replace("\\", "\\\\") + ")");

			// end the page device properties setup dictionary
			command.Append(" >> setpagedevice ");

			// Convert file type.
			command.Append("(" + inPathFileToConvert.Replace("\\", "\\\\") + ") run ");

			gs_error_type code = m_GhostscriptWrapper.gsapi_run_string_with_length(command.ToString());

			if (code != gs_error_type.gs_error_ok)
			{
				Logger.LogError("FileConverter.ConvertPDF2GrayscalePDF - gsapi_run_string_with_length return error {0} for Instance {1} for file {2}", code.ToString(), m_GhostscriptWrapper.InstanceId, inPathFileToConvert);
				m_LastRunSuccedded = false;
				return false;
			}

			m_LastRunSuccedded = DummyFileOutput(true);
			return m_LastRunSuccedded;
		}
	}
}
