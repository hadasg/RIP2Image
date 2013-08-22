/*******************************************************************************
	RIP2Image is a program that efficiently converts formats such as PDF or Postscript to image formats such as Jpeg or PNG.
    Copyright (C) 2013 XMPie Ltd.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace RIP2Jmage
{
	[ServiceContract(Namespace = "RIP2Jmage")]
    public interface IConverterService
		{ 

		/// <summary>
		/// Convert file type. 
		/// </summary>
		/// <param name="inConvertFilePath"></param>
		/// <param name="inTargetFilePath"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		[OperationContract(Action = "RIP2Jmage/ConvertPDF2JPG")]
		bool ConvertPDF2JPG(string inConvertFilePath, string inTargetFilePath, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality);

		/// <summary>
		/// Convert all files type under inConvertFolderPath to JPG.
		/// </summary>
		/// <param name="inConvertFolderPath"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"></param>
		/// <param name="inSearchSubFoldersstring"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		[OperationContract(Action = "RIP2Jmage/ConvertPDFFolder2JPG")]
		bool ConvertPDFFolder2JPG(string inConvertFolderPath, string inTargetFolderPath, string inConvertFileWildCard, bool inDeleteSourcePDF, bool inSearchSubFoldersstring, double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality);
    }
}
