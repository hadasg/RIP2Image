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
using System.Web;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
// using log4net;
// using log4net.Config;

/*[assembly: log4net.Config.XmlConfigurator(Watch = true)]*/

namespace RIP2Image
{
	/// <summary>
	/// Uniting all convert utilities.
	/// </summary>
	class ConverterService : IConverterService
	{
		//private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constructor.
		/// </summary>
		public ConverterService()
		{
		}


		#region Convert methods
		public bool ConvertPDF2JPG(string inConvertFilePath,
								   string inNewFileTargetFolderPath,
								   double inResolutionX,
								   double inResolutionY,
								   double inGraphicsAlphaBitsValue,
								   double inTextAlphaBitsValue,
								   double inQuality)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetFolderPath = " + inNewFileTargetFolderPath +
			// 						", inResolutionX = " + inResolutionX + ", inResolutionY = " + inResolutionY + ", inGraphicsAlphaBitsValue = " + inGraphicsAlphaBitsValue +
			// 						", inTextAlphaBitsValue = " + inTextAlphaBitsValue + ", inQuality = " + inQuality);

			bool conversionSucceed;

			CheckJPGParamValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2JPG);
			conversionSucceed = fileConvertor.ConvertPDF2JPG(inConvertFilePath, inNewFileTargetFolderPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2JPG, fileConvertor);

			// Rename JPG names to the correct page counter.
			RenameImagesNames(inNewFileTargetFolderPath, inConvertFilePath, "jpg");


			return conversionSucceed;
		}

		public bool ConvertPDF2PNG(string inConvertFilePath,
								   string inNewFileTargetFolderPath,
								   double inResolutionX,
								   double inResolutionY,
								   double inGraphicsAlphaBitsValue,
								   double inTextAlphaBitsValue)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetFolderPath = " + inNewFileTargetFolderPath +
			// 						", inResolutionX = " + inResolutionX + ", inResolutionY = " + inResolutionY + ", inGraphicsAlphaBitsValue = " + inGraphicsAlphaBitsValue +
			// 						", inTextAlphaBitsValue = " + inTextAlphaBitsValue + ");

			bool conversionSucceed;

			CheckBaseParamsValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2PNG);
			conversionSucceed = fileConvertor.ConvertPDF2PNG(inConvertFilePath, inNewFileTargetFolderPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2PNG, fileConvertor);

			// Rename PNG names to the correct page counter.
			RenameImagesNames(inNewFileTargetFolderPath, inConvertFilePath, "png");


			return conversionSucceed;
		}

		public bool ConvertPDF2PNGSingle(string inConvertFilePath,
								   string inNewFileTargetPath,
								   double inResolutionX,
								   double inResolutionY,
								   double inGraphicsAlphaBitsValue,
								   double inTextAlphaBitsValue)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetFolderPath = " + inNewFileTargetFolderPath +
			// 						", inResolutionX = " + inResolutionX + ", inResolutionY = " + inResolutionY + ", inGraphicsAlphaBitsValue = " + inGraphicsAlphaBitsValue +
			// 						", inTextAlphaBitsValue = " + inTextAlphaBitsValue + ");

			bool conversionSucceed;

			CheckBaseParamsValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2PNGSingle);
			conversionSucceed = fileConvertor.ConvertPDF2PNGSingle(inConvertFilePath, inNewFileTargetPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2PNGSingle, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertPDF2EPS(string inConvertFilePath, string inNewFileTargetPath, double inFirstPageToConvert, double inLastPageToConvert)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetPath = " + inNewFileTargetPath +
			// 						", inFirstPageToConvert = " + inFirstPageToConvert + ", inLastPageToConvert = " + inLastPageToConvert);

			bool conversionSucceed;

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2EPS);
			conversionSucceed = fileConvertor.ConvertPDF2EPS(inConvertFilePath, inNewFileTargetPath, inFirstPageToConvert, inLastPageToConvert);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2EPS, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertEPS2PDF(string inConvertFilePath, string inNewFileTargetPath)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetPath = " + inNewFileTargetPath);

			bool conversionSucceed;

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.EPS2PDF);
			conversionSucceed = fileConvertor.ConvertEPS2PDF(inConvertFilePath, inNewFileTargetPath);
			InstancesManager.PutObject(InstancesManager.ConversionType.EPS2PDF, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertEPS2LowResPDF(string inConvertFilePath, string inNewFileTargetPath)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetPath = " + inNewFileTargetPath);

			bool conversionSucceed;

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.EPS2LowResPDF);
			conversionSucceed = fileConvertor.ConvertEPS2LowResPDF(inConvertFilePath, inNewFileTargetPath);
			InstancesManager.PutObject(InstancesManager.ConversionType.EPS2LowResPDF, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertPDF2LowResPDF(string inConvertFilePath, string inNewFileTargetPath)
		{
			// 			logger.Info("inConvertFilePath = " + inConvertFilePath + ", inNewFileTargetPath = " + inNewFileTargetPath);

			bool conversionSucceed;

			// Make the conversion.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2LowResPDF);
			conversionSucceed = fileConvertor.ConvertPDF2LowResPDF(inConvertFilePath, inNewFileTargetPath);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2LowResPDF, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertImage2LowResImage(string inConvertFilePath, string inNewFileTargetPath)
		{
			ImageFormat targetFormat = ImageFormat.Jpeg;
			string typeStr = Path.GetExtension(inConvertFilePath).Substring(1).ToUpperInvariant();
			switch (typeStr)
			{
				case "JPEG":
				case "JPG":
					targetFormat = ImageFormat.Jpeg;
					break;
				case "TIF":
				case "TIFF":
					targetFormat = ImageFormat.Tiff;
					break;
				case "BMP":
					targetFormat = ImageFormat.Bmp;
					break;
				case "GIF":
					targetFormat = ImageFormat.Gif;
					break;
				case "PNG":
					targetFormat = ImageFormat.Png;
					break;
			}
			try
			{
				using (Bitmap bitmap = new Bitmap(inConvertFilePath))
				{
					float targetResolution = 72;
					// 96 means that windows might have failed reading the original resolution so we don't change it
					if (bitmap.HorizontalResolution == 96 && bitmap.VerticalResolution == 96)
						return false;
					if (bitmap.HorizontalResolution <= 72 || bitmap.VerticalResolution <= 72)
						return false;
					using (Bitmap smallBitmap = new Bitmap(bitmap, new Size((int)(bitmap.Width / (bitmap.HorizontalResolution / targetResolution)), (int)(bitmap.Height / (bitmap.VerticalResolution / targetResolution)))))
					{
						smallBitmap.SetResolution(targetResolution, targetResolution);
						ImageCodecInfo encoder = GetEncoder(targetFormat);
						EncoderParameters encoderParams = new EncoderParameters(1);
						encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, Image.GetPixelFormatSize(bitmap.PixelFormat));
						smallBitmap.Save(inNewFileTargetPath, encoder, encoderParams);
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
					return codec;
			}
			return null;
		}
		#endregion

		#region Folder conversion.
		public bool ConvertPDFFolder2JPG(string inConvertFolderPath,
										 string inTargetFolderPath,
										 string inConvertFileWildCard,
										 bool inDeleteSourcePDF,
										 bool inSearchSubFolders,
										 double inResolutionX,
										 double inResolutionY,
										 double inGraphicsAlphaBitsValue,
										 double inTextAlphaBitsValue,
										 double inQuality)
		{
			// 			logger.Info("inConvertFolderPath = " + inConvertFolderPath + ", inTargetFolderPath = " + inTargetFolderPath +
			// 						", inConvertFileWildCard = " + inConvertFileWildCard + ", inDeleteSourcePDF = " + inDeleteSourcePDF + ", inSearchSubFolders = " + inSearchSubFolders +
			// 						", inResolutionX = " + inResolutionX + ", inResolutionY = " + inResolutionY + ", inGraphicsAlphaBitsValue = " + inGraphicsAlphaBitsValue +
			// 						 ", inTextAlphaBitsValue = " + inTextAlphaBitsValue + ", inQuality = " + inQuality);

			bool conversionSucceed;

			CheckJPGParamValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);

			System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(inConvertFolderPath);

			// Convert all files in folder.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2JPG);
			conversionSucceed = WalkDirectoryTreePDF2JPG(fileConvertor, root, inTargetFolderPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inConvertFolderPath.Equals(inTargetFolderPath), inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2JPG, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertPDFFolder2PNG(string inConvertFolderPath,
								 string inTargetFolderPath,
								 string inConvertFileWildCard,
								 bool inDeleteSourcePDF,
								 bool inSearchSubFolders,
								 double inResolutionX,
								 double inResolutionY,
								 double inGraphicsAlphaBitsValue,
								 double inTextAlphaBitsValue)
		{
			// 			logger.Info("inConvertFolderPath = " + inConvertFolderPath + ", inTargetFolderPath = " + inTargetFolderPath +
			// 						", inConvertFileWildCard = " + inConvertFileWildCard + ", inDeleteSourcePDF = " + inDeleteSourcePDF + ", inSearchSubFolders = " + inSearchSubFolders +
			// 						", inResolutionX = " + inResolutionX + ", inResolutionY = " + inResolutionY + ", inGraphicsAlphaBitsValue = " + inGraphicsAlphaBitsValue +
			// 						 ", inTextAlphaBitsValue = " + inTextAlphaBitsValue);

			bool conversionSucceed;

			CheckBaseParamsValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);

			System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(inConvertFolderPath);

			// Convert all files in folder.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2PNG);
			conversionSucceed = WalkDirectoryTreePDF2PNG(fileConvertor, root, inTargetFolderPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inConvertFolderPath.Equals(inTargetFolderPath), inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2PNG, fileConvertor);

			return conversionSucceed;
		}

		public bool ConvertPDFFolder2EPS(string inConvertFolderPath,
										 string inTargetFolderPath,
										 string inConvertFileWildCard,
										 bool inDeleteSourcePDF,
										 bool inSearchSubFolders,
										 double inFirstPageToConvert,
										 double inLastPageToConvert)
		{
			// 			logger.Info("inConvertFolderPath = " + inConvertFolderPath + ", inTargetFolderPath = " + inTargetFolderPath +
			// 						", inConvertFileWildCard = " + inConvertFileWildCard + ", inDeleteSourcePDF = " + inDeleteSourcePDF + ", inSearchSubFolders = " + inSearchSubFolders +
			// 						", inFirstPageToConvert = " + inFirstPageToConvert + ", inLastPageToConvert = " + inLastPageToConvert);

			bool conversionSucceed;

			System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(inConvertFolderPath);

			// Convert all files in folder.
			FileConverter fileConvertor = InstancesManager.GetObject(InstancesManager.ConversionType.PDF2EPS);
			conversionSucceed = WalkDirectoryTreePDF2EPS(fileConvertor, root, inTargetFolderPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders,
														inConvertFolderPath.Equals(inTargetFolderPath), inFirstPageToConvert, inLastPageToConvert);
			InstancesManager.PutObject(InstancesManager.ConversionType.PDF2EPS, fileConvertor);

			return conversionSucceed;
		}

		#endregion

		#region Help Method

		/// <summary>
		/// Check base parameters validation.
		/// </summary>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <returns></returns>
		private void CheckBaseParamsValidation(double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue)
		{
			if (inResolutionX <= 0 || inResolutionY <= 0)
				throw new ArgumentException("Resolution cannot be <= 0");
			else if (!(inGraphicsAlphaBitsValue == 1 || inGraphicsAlphaBitsValue == 2 || inGraphicsAlphaBitsValue == 4))
				throw new ArgumentException("GraphicsAlphaBits values are 1, 2 or 4");
			else if (!(inTextAlphaBitsValue == 1 || inTextAlphaBitsValue == 2 || inTextAlphaBitsValue == 4))
				throw new ArgumentException("TextAlphaBits values are 1, 2 or 4");
		}

		/// <summary>
		/// Check JPG parameters validation.
		/// </summary>
		/// <param name="inResolutionX"></param>
		/// <param name="inResolutionY"></param>
		/// <param name="inGraphicsAlphaBitsValue"></param>
		/// <param name="inTextAlphaBitsValue"></param>
		/// <param name="inQuality"></param>
		private void CheckJPGParamValidation(double inResolutionX, double inResolutionY, double inGraphicsAlphaBitsValue, double inTextAlphaBitsValue, double inQuality)
		{
			CheckBaseParamsValidation(inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);

			if (inQuality < 0 || inQuality > 100)
				throw new ArgumentException("File quality range is 0-100");
		}


		/// <summary>
		/// Walking traverse all folders under inRoot looking for PDF files need to convert to JPG.
		/// </summary>
		/// <param name="inSearchSubFolders"> If true traverse each sub-folders and convert them, except if one of the sub-folders is the target folder. </param>
		/// <param name="inSameTargetFolder"> If false create new sub folder under target folder path with the same name as the root sub-folder. </param>
		private bool WalkDirectoryTreePDF2JPG(FileConverter inFileConvertor,
											  System.IO.DirectoryInfo inRoot,
											  string inTargetFolderPath,
											  string inConvertFileWildCard,
											  bool inDeleteSourcePDF,
											  bool inSearchSubFolders,
											  bool inSameTargetFolder,
											  double inResolutionX,
											  double inResolutionY,
											  double inGraphicsAlphaBitsValue,
											  double inTextAlphaBitsValue,
											  double inQuality)
		{
			bool fileConversion;

			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			files = inRoot.GetFiles(inConvertFileWildCard);

			if (files != null)
			{
				foreach (System.IO.FileInfo file in files)
				{
					// Make file conversion.
					fileConversion = inFileConvertor.ConvertPDF2JPG(file.FullName, inTargetFolderPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
					if (!fileConversion)
						return false;

					//Delete old files.
					if (inDeleteSourcePDF)
						FileDelete(file.FullName);


					// Rename JPG names to the correct page counter.
					RenameImagesNames(inTargetFolderPath, file.FullName, "jpg");
				}

				if (inSearchSubFolders)
				{
					// Now find all the subdirectories under this directory.
					subDirs = inRoot.GetDirectories();
					foreach (System.IO.DirectoryInfo dirInfo in subDirs)
					{
						// In case the target folder is sub directory of the converted folder don't check it. 
						if (inTargetFolderPath.Contains(dirInfo.FullName))
							continue;
						if (!inSameTargetFolder)
						{
							//Create a new sub folder under target folder path
							string newPath = System.IO.Path.Combine(inTargetFolderPath, dirInfo.Name);
							//Create the sub folder
							System.IO.Directory.CreateDirectory(newPath);
							//Recursive call for each subdirectory.
							WalkDirectoryTreePDF2JPG(inFileConvertor, dirInfo, newPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}
						else
						{
							// Recursive call for each subdirectory.
							WalkDirectoryTreePDF2JPG(inFileConvertor, dirInfo, dirInfo.FullName, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}

					}
				}

			}

			return true;
		}

		/// <summary>
		/// Walking traverse all folders under inRoot looking for PDF files need to convert to PNG.
		/// </summary>
		/// <param name="inSearchSubFolders"> If true traverse each sub-folders and convert them, except if one of the sub-folders is the target folder. </param>
		/// <param name="inSameTargetFolder"> If false create new sub folder under target folder path with the same name as the root sub-folder. </param>
		private bool WalkDirectoryTreePDF2PNG(FileConverter inFileConvertor,
											  System.IO.DirectoryInfo inRoot,
											  string inTargetFolderPath,
											  string inConvertFileWildCard,
											  bool inDeleteSourcePDF,
											  bool inSearchSubFolders,
											  bool inSameTargetFolder,
											  double inResolutionX,
											  double inResolutionY,
											  double inGraphicsAlphaBitsValue,
											  double inTextAlphaBitsValue)
		{
			bool fileConversion;

			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			files = inRoot.GetFiles(inConvertFileWildCard);

			if (files != null)
			{
				foreach (System.IO.FileInfo file in files)
				{
					// Make file conversion.
					fileConversion = inFileConvertor.ConvertPDF2PNG(file.FullName, inTargetFolderPath, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue);
					if (!fileConversion)
						return false;

					//Delete old files.
					if (inDeleteSourcePDF)
						FileDelete(file.FullName);


					// Rename PNG names to the correct page counter.
					RenameImagesNames(inTargetFolderPath, file.FullName, "png");
				}

				if (inSearchSubFolders)
				{
					// Now find all the subdirectories under this directory.
					subDirs = inRoot.GetDirectories();
					foreach (System.IO.DirectoryInfo dirInfo in subDirs)
					{
						// In case the target folder is sub directory of the converted folder don't check it. 
						if (inTargetFolderPath.Contains(dirInfo.FullName))
							continue;
						if (!inSameTargetFolder)
						{
							//Create a new sub folder under target folder path
							string newPath = System.IO.Path.Combine(inTargetFolderPath, dirInfo.Name);
							//Create the sub folder
							System.IO.Directory.CreateDirectory(newPath);
							//Recursive call for each subdirectory.
							WalkDirectoryTreePDF2PNG(inFileConvertor, dirInfo, newPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}
						else
						{
							// Recursive call for each subdirectory.
							WalkDirectoryTreePDF2PNG(inFileConvertor, dirInfo, dirInfo.FullName, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inResolutionX, inResolutionY, inGraphicsAlphaBitsValue, inTextAlphaBitsValue, inQuality);
						}

					}
				}

			}

			return true;
		}

		/// <summary>
		/// Walking traverse all folders under inRoot looking for PDF files need to convert to EPS.
		/// </summary>
		/// <param name="inFileConvertor"></param>
		/// <param name="inRoot"></param>
		/// <param name="inTargetFolderPath"></param>
		/// <param name="inConvertFileWildCard"></param>
		/// <param name="inDeleteSourcePDF"></param>
		/// <param name="inSearchSubFolders"> If true traverse each sub-folders and convert them, except if one of the sub-folders is the target folder. </param>
		/// <param name="inSameTargetFolder"> If false create new sub folder under target folder path with the same name as the root sub-folder. </param>
		/// <param name="inFirstPageToConvert"></param>
		/// <param name="inLastPageToConvert"></param>
		/// <returns></returns>
		private bool WalkDirectoryTreePDF2EPS(FileConverter inFileConvertor, System.IO.DirectoryInfo inRoot, string inTargetFolderPath, string inConvertFileWildCard,
												bool inDeleteSourcePDF, bool inSearchSubFolders, bool inSameTargetFolder, double inFirstPageToConvert, double inLastPageToConvert)
		{
			bool fileConversion;

			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			files = inRoot.GetFiles(inConvertFileWildCard);

			if (files != null)
			{
				foreach (System.IO.FileInfo file in files)
				{
					// Create converted EPS path.
					string convertedEPSPath = inTargetFolderPath + "\\" + Path.GetFileNameWithoutExtension(file.FullName) + ".eps";

					// Make file conversion.
					fileConversion = inFileConvertor.ConvertPDF2EPS(file.FullName, convertedEPSPath, inFirstPageToConvert, inLastPageToConvert);
					if (!fileConversion)
						return false;

					//Delete old files.
					if (inDeleteSourcePDF)
						FileDelete(file.FullName);
				}

				if (inSearchSubFolders)
				{
					// Now find all the subdirectories under this directory.
					subDirs = inRoot.GetDirectories();
					foreach (System.IO.DirectoryInfo dirInfo in subDirs)
					{
						// In case the target folder is sub directory of the converted folder don't check it. 
						if (inTargetFolderPath.Contains(dirInfo.FullName))
							continue;

						if (!inSameTargetFolder)
						{
							//Create new sub folder under target folder path
							string newPath = System.IO.Path.Combine(inTargetFolderPath, dirInfo.Name);
							//Create the sub folder
							System.IO.Directory.CreateDirectory(newPath);
							//Recursive call for each subdirectory.
							WalkDirectoryTreePDF2EPS(inFileConvertor, dirInfo, newPath, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inFirstPageToConvert, inLastPageToConvert);
						}
						else
						{
							// Recursive call for each subdirectory.
							WalkDirectoryTreePDF2EPS(inFileConvertor, dirInfo, dirInfo.FullName, inConvertFileWildCard, inDeleteSourcePDF, inSearchSubFolders, inSameTargetFolder, inFirstPageToConvert, inLastPageToConvert);
						}

					}
				}

			}

			return true;
		}

		/// <summary>
		/// Rename images names to the correct name and page counter.
		/// </summary>
		/// <param name="inFileDir">Target folder path</param>
		/// <param name="inFileFullName">File full path name</param>
		/// <param name="inFileExtension">File extension</param>
		private void RenameImagesNames(string inFileDir, string inFileFullName, string inFileExtension)
		{
			string[] filesNameWithTheSamePrefix = Directory.GetFiles(inFileDir, Path.GetFileNameWithoutExtension(inFileFullName) + "-*." + inFileExtension);

			int filesCounter = 1;
			foreach (string fileName in filesNameWithTheSamePrefix)
			{
				string pageNumberOutputFormat = GeneratePageNumberOutputFormat(filesCounter);
				string fileNewName = inFileDir + "\\" + Path.GetFileNameWithoutExtension(inFileFullName) + pageNumberOutputFormat + filesCounter + "." + inFileExtension;
				// Rename file.
				FileMove(fileName, fileNewName);
				filesCounter++;
			}
		}

		/// <summary>
		/// Generate page number prefix format. 
		/// </summary>
		/// <param name="inFilesCounter"></param>
		/// <returns></returns>
		private string GeneratePageNumberOutputFormat(int inFilesCounter)
		{
			if (inFilesCounter >= 1 && inFilesCounter <= 9)
				return "_p00";
			else if (inFilesCounter >= 10 && inFilesCounter <= 99)
				return "_p0";
			else if (inFilesCounter >= 100 && inFilesCounter <= 999)
				return "_p";

			return null;
		}

		/// <summary>
		/// Tries to move (or rename) file several times in order to avoid unavailable/locked files issues
		/// </summary>		
		private void FileMove(string sourceFileName, string destFileName)
		{
			// try to move file
			int i = 0;
			while (true)
			{
				try
				{
					if (File.Exists(destFileName))
						File.Delete(destFileName);
					File.Move(sourceFileName, destFileName);
					break;
				}
				catch (Exception ex)
				{
					if (i < 3)
						Thread.Sleep(++i * 100);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Tries to delete the file several times in order to avoid unavailable/locked files issues
		/// </summary>		
		private void FileDelete(string filePath)
		{
			int i = 0;
			while (true)
			{
				try
				{
					File.Delete(filePath);
					break;
				}
				catch (Exception ex)
				{
					if (i < 3)
						Thread.Sleep(++i * 100);
					else
						throw ex;
				}
			}
		}
		#endregion

	}
}
