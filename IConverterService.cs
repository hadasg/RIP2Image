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
using System.ServiceModel;

namespace RIP2Image
{
	[ServiceContract(Namespace = "RIP2Image")]
	public interface IConverterService
	{

		/// <summary>
		/// Convert PDF to JPG. 
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetFolderPath">Folder path where the converted file will generate.</param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDF2JPG")]
		bool ConvertPDF2JPG(string inConvertFilePath,
							string inNewFileTargetFolderPath,
							double inResolutionX,
							double inResolutionY,
							double inGraphicsAlphaBitsValue,
							double inTextAlphaBitsValue,
							double inQuality);

		/// <summary>
		/// Convert PDF to PNG. 
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetFolderPath">Folder path where the converted file will generate.</param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDF2PNG")]
		bool ConvertPDF2PNG(string inConvertFilePath,
							string inNewFileTargetFolderPath,
							double inResolutionX,
							double inResolutionY,
							double inGraphicsAlphaBitsValue,
							double inTextAlphaBitsValue);

		/// <summary>
		/// Convert the first page of the PDF to PNG.  
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetPath">Full path of the converted file will generate.</param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inPageNumToConvert"></param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDF2PNGSingle")]
		bool ConvertPDF2PNGSingle(string inConvertFilePath,
							string inNewFileTargetPath,
							double inResolutionX,
							double inResolutionY,
							double inGraphicsAlphaBitsValue,
							double inTextAlphaBitsValue);

		/// <summary>
		///  Convert PDF to EPS.
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetPath">Full path where the converted file will generate.</param>
		/// <param name="inFirstPageToConvert"> First page to convert in the PDF </param>
		/// <param name="inLastPageToConvert"> Last page to convert in the PDF </param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDF2EPS")]
		bool ConvertPDF2EPS(string inConvertFilePath, string inNewFileTargetPath, double inFirstPageToConvert, double inLastPageToConvert);

		/// <summary>
		/// Convert EPS to PDF
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetPath">Full path where the converted file will generate.</param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertEPS2PDF")]
		bool ConvertEPS2PDF(string inConvertFilePath, string inNewFileTargetPath);

		/// <summary>
		/// Convert EPS to PDF
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetPath">Full path where the converted file will generate.</param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertEPS2LowResPDF")]
		bool ConvertEPS2LowResPDF(string inConvertFilePath, string inNewFileTargetPath);

		/// <summary>
		/// Convert PDF to low res PDF
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert.</param>
		/// <param name="inNewFileTargetPath">Full path where the converted file will generate.</param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDF2LowResPDF")]
		bool ConvertPDF2LowResPDF(string inConvertFilePath, string inNewFileTargetPath);

		/// <summary>
		/// Convert image to low res image
		/// </summary>
		/// <param name="inConvertFilePath">Full path of the file we going to convert can be BMP, GIF, JPEG, PNG, TIFF.</param>
		/// <param name="inNewFileTargetPath">Full path where the converted file will generate.</param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertImage2LowResImage")]
		bool ConvertImage2LowResImage(string inConvertFilePath, string inNewFileTargetPath);

		/// <summary>
		/// Convert all files type under inConvertFolderPath to JPG.
		/// </summary>
		/// <param name="inConvertFolderPath"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"></param>
		/// <param name="inSearchSubFolders"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
		/// <returns></returns>
		[OperationContract(Action = "RIP2Image/ConvertPDFFolder2JPG")]
		bool ConvertPDFFolder2JPG(string inConvertFolderPath,
								  string inTargetFolderPath,
								  string inConvertFileWildCard,
								  bool inDeleteSourcePDF,
								  bool inSearchSubFolders,
								  double inResolutionX,
								  double inResolutionY,
								  double inGraphicsAlphaBitsValue,
								  double inTextAlphaBitsValue,
								  double inQuality);

		/// <summary>
		/// Convert all files type under inConvertFolderPath to EPS.
		/// </summary>
		/// <param name="inConvertFolderPath"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"></param>
		/// <param name="inSearchSubFolders"></param>
		/// <param name="inFirstPageToConvert"> First page to convert in all the PDF in the given folder  </param>
		/// <param name="inLastPageToConvert"> Last page to convert in all the PDF in the given folder  </param>
		/// <returns></returns>
		bool ConvertPDFFolder2EPS(string inConvertFolderPath,
								  string inTargetFolderPath,
								  string inConvertFileWildCard,
								  bool inDeleteSourcePDF,
								  bool inSearchSubFolders,
								  double inFirstPageToConvert,
								  double inLastPageToConvert);
	}
}
