/*******************************************************************************
Description:
	Ghostscript Service contract.

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
using System.ServiceModel;

namespace GhostscriptService
{
	[ServiceContract(Namespace = "GhostscriptService")]
    public interface IConverterService
    { 

		/// <summary>
		/// Convert file type. 
		/// </summary>
		/// <param name="inConvertFilePath"></param>
		/// <param name="inTargetFilePath"></param>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		[OperationContract(Action = "GhostscriptService/ConvertPDF2JPG")]
		bool ConvertPDF2JPG(string inConvertFilePath, string inTargetFilePath, double inQuality, double inResolutionX, double inResolutionY);

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
		[OperationContract(Action = "GhostscriptService/ConvertPDFFolder2JPG")]
		bool ConvertPDFFolder2JPG(string inConvertFolderPath, string inTargetFolderPath, string inConvertFileWildCard, bool inDeleteSourcePDF, bool inSearchSubFoldersstring, double inQuality, double inResolutionX, double inResolutionY);
    }
}
