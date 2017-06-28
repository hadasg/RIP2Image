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
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace RIP2Image
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new GhostscriptService()
			};
			ServiceBase.Run(ServicesToRun);

			InstancesManager.DeleteDynamicLoadingDLL();
		}
	}

	static class Tester
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
		private static extern int AllocConsole();

		static void Main()
		{
			AllocConsole();

			string testFolder = "..\\..\\Tests\\";
			string outputFolder = testFolder + "Output\\";
			string inputFolder = testFolder + "Input\\PNG folder";
			int numRepeats = 1;

			if (!System.IO.Directory.Exists(testFolder))
				System.IO.Directory.CreateDirectory(testFolder);
			if (!System.IO.Directory.Exists(outputFolder))
				System.IO.Directory.CreateDirectory(outputFolder);

			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(outputFolder);
			foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
				dir.Delete(true);
			foreach (System.IO.FileInfo file in di.GetFiles())
				file.Delete();

			Console.WriteLine("Start Test");
			Stopwatch stopWatch = new Stopwatch();
			Stopwatch iterationWatch = new Stopwatch();
			TimeSpan ts;
			stopWatch.Start();

			ConverterService converter = new ConverterService();
			for (int i = 1; i <= numRepeats; ++i)
			{
				iterationWatch.Reset();
				iterationWatch.Start();

				//converter.ConvertPDF2JPG(inputFolder + "Sample.pdf", outputFolder, 72, 72, 1, 1, 72);
				//converter.ConvertPDF2EPS(inputFolder + "Sample.pdf", outputFolder + i.ToString() + ".ps",1,1);
				//converter.ConvertEPS2PDF(outputFolder + i.ToString() + ".ps", outputFolder + i.ToString() + ".pdf");
				//converter.ConvertEPS2LowResPDF(outputFolder + i.ToString() + ".ps", outputFolder + i.ToString() + "_lowres.pdf");
				//converter.ConvertImage2LowResImage(inputFolder + "ImageTest.jpg", inputFolder + i.ToString() + ".jpg");
				//converter.ConvertImage2LowResImage(inputFolder + "Sample.TIF", outputFolder + i.ToString() + ".tif");
				//converter.ConvertImage2LowResImage(inputFolder + "Sample.jpg", outputFolder + i.ToString() + ".jpg");
				//converter.ConvertImage2LowResImage(inputFolder + "Sample.png", outputFolder + i.ToString() + ".png");
				//converter.ConvertImage2LowResImage(inputFolder + "Sample.gif", outputFolder + i.ToString() + ".gif");
				//converter.ConvertPDF2PNGSingle(inputFolder + "Sample.pdf", outputFolder + "Sample.png", 72, 72, 2, 4);
				converter.ConvertPDFFolder2PNG(inputFolder, outputFolder, "*.pdf", true, true, 72, 72, 2, 4);

				iterationWatch.Stop();
				ts = iterationWatch.Elapsed;
				Console.WriteLine("RunTime iteration {0} {1:00}:{2:00}:{3:00}.{4:000}", i, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
			}

			stopWatch.Stop();
			ts = stopWatch.Elapsed;
			Console.WriteLine("");
			Console.WriteLine("Total Run Time: {0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
			ts = TimeSpan.FromMilliseconds(ts.TotalMilliseconds / numRepeats);
			Console.WriteLine("Avrag Run Time: {0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);

			InstancesManager.DeleteDynamicLoadingDLL();
			Console.ReadLine();

			/*
			Worker workerObject = new Worker();
			Thread workerThread = new Thread(workerObject.DoWork1);
			//workerThread.Start();

			Worker workerObject2 = new Worker();
			Thread workerThread2 = new Thread(workerObject.DoWork2);
			//workerThread2.Start();

			Worker workerObject3 = new Worker();
			Thread workerThread3 = new Thread(workerObject.DoWork3);
			//workerThread3.Start();

			Worker workerObject4 = new Worker();
			Thread workerThread4 = new Thread(workerObject.DoWork4);
			//workerThread4.Start();
			
			//convert.ConvertFileType("C:\\gs\\PDF\\Folder2\\2Text_graph_image_cmyk_rgb.pdf", "C:\\gs\\JPG\\Folder2", wild.Length, "jpg");
		
			convert.ConvertFileTypeNestedFolders(convertFolderPath, targetFolderPath, "*.pdf", "jpg");
            
			sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.ElapsedMilliseconds/1000);

			workerThread.Join();
			workerThread2.Join();
			*/
		}

		public class Worker
		{
			// This method will be called when the thread is started.
			public void DoWork1()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();

				convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\1000RPDF\\1000R-1", "C:\\gs\\XLIMTest\\1000RJPG\\1000R-1", "*.pdf", true, true, 72, 72, 2, 4, 100);

				// 				for (int i = 3; i < 101; i++)
				// 				{
				// 				convert.ConvertPDF2JPG("C:\\gs\\PDFThread\\Folder1\\text_graphic_image - Copy - Copy "+ i +".pdf", "C:\\gs\\JPG\\Folder1", 72, 72, 4, 4, 85);
				// 				}
				// 				
				sw.Stop();
				Console.WriteLine("Elapsed Thread 1={0}", sw.ElapsedMilliseconds / 1000);
			}

			public void DoWork2()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();

				convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\1000RPDF\\1000R-2", "C:\\gs\\XLIMTest\\1000RJPG\\1000R-2", "*.pdf", true, true, 72, 72, 2, 4, 100);
				//convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\PDF", "C:\\gs\\XLIMTest\\JPG", "*.pdf", false, true, 72, 72, 4, 4, 85);
				/*
				for (int i = 3; i < 101; i++)
 				{
					convert.ConvertPDF2JPG("C:\\gs\\PDFThread\\Folder1\\text_graphic_image - Copy - Copy " + i + ".pdf", "C:\\gs\\JPG\\Folder2", 1, 1, 85, 72, 72);
 				}
				*/
				sw.Stop();
				Console.WriteLine("Elapsed Thread 2={0}", sw.ElapsedMilliseconds / 1000);
			}

			public void DoWork3()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();

				convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\1000RPDF\\1000R-3", "C:\\gs\\XLIMTest\\1000RJPG\\1000R-3", "*.pdf", true, true, 72, 72, 2, 4, 100);
				//convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\PDF", "C:\\gs\\XLIMTest\\JPG", "*.pdf", false, true, 72, 72, 4, 4, 85);
				/*
				for (int i = 3; i < 101; i++)
 				{
					convert.ConvertPDF2JPG("C:\\gs\\PDFThread\\Folder1\\text_graphic_image - Copy - Copy " + i + ".pdf", "C:\\gs\\JPG\\Folder2", 1, 1, 85, 72, 72);
 				}
				*/
				sw.Stop();
				Console.WriteLine("Elapsed Thread 2={0}", sw.ElapsedMilliseconds / 1000);
			}

			public void DoWork4()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();

				convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\1000RPDF\\1000R-4", "C:\\gs\\XLIMTest\\1000RJPG\\1000R-4", "*.pdf", true, true, 72, 72, 2, 4, 100);
				//convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\PDF", "C:\\gs\\XLIMTest\\JPG", "*.pdf", false, true, 72, 72, 4, 4, 85);
				/*
				for (int i = 3; i < 101; i++)
 				{
					convert.ConvertPDF2JPG("C:\\gs\\PDFThread\\Folder1\\text_graphic_image - Copy - Copy " + i + ".pdf", "C:\\gs\\JPG\\Folder2", 1, 1, 85, 72, 72);
 				}
				*/
				sw.Stop();
				Console.WriteLine("Elapsed Thread 2={0}", sw.ElapsedMilliseconds / 1000);
			}

		}


		/*
		/// <summary>
		/// Extracting file name from convertFilePath to be JPG name.
		/// </summary>
		/// <param name="convertFilePath"></param>
		/// <param name="wildCardLength"></param>
		/// <returns></returns>
		private static string GetJPGName(string convertFilePath, int wildCardLength)
        {
            int lastDoubleSlashIndex = convertFilePath.LastIndexOf("\\");
			
			int fileNameLastIndex = convertFilePath.Length - wildCardLength - 1;

            int fileNameLength = fileNameLastIndex - lastDoubleSlashIndex;

            string jpgName = convertFilePath.Substring(lastDoubleSlashIndex + 1, fileNameLength+1);
            return jpgName;
        }

		/// <summary>
		/// Convert file type to JPG.
		/// </summary>
		/// <param name="fileConverter"></param>
		/// <param name="convertFilePath"></param>
		/// <param name="targetFilePath">target JPG file path</param>
		/// <param name="wildCardLength"></param>
		public static void Convert2JPG(FileConvertor fileConverter, string convertFilePath, string targetFilePath, int wildCardLength, string newFileType)
        {
			string fileName = GetJPGName(convertFilePath, wildCardLength);
			//fileName += "-%d.jpg";
			fileName += "-%d." + newFileType;
			
			targetFilePath += "\\";
			string outputFileFullPath = targetFilePath + fileName;
			
			outputFileFullPath = outputFileFullPath.Replace("\\", "\\\\");

			convertFilePath = convertFilePath.Replace("\\", "\\\\");
			fileConverter.Convert(convertFilePath, outputFileFullPath);
        }

		/// <summary>
		/// Convert all files type under convertFolderPath to JPG.
		/// </summary>
		/// <param name="fileConverter"></param>
		/// <param name="convertFolderPath"></param>
		/// <param name="targetFolderPath"></param>
		/// <param name="convertFileWildCard"></param>
		public static void Convert2JPGNestedFolders(FileConvertor fileConverter, string convertFolderPath, string targetFolderPath, string convertFileWildCard)
        {
            System.IO.DirectoryInfo root = new System.IO.DirectoryInfo(convertFolderPath);
			WalkDirectoryTree(fileConverter, root, targetFolderPath, convertFolderPath.Equals(targetFolderPath), convertFileWildCard);
        }

		/// <summary>
		/// Walking traverse all folders under root looking for appropriate files which need to be convert to JPG. 
		/// While find one convert it and put it under the same folder if sameTrgetFolder==true, otherwise creating a folder under targetFolderPath with the
		/// same name as the original file located on.
		/// </summary>
		/// <param name="fileConverter"></param>
		/// <param name="root"></param>
		/// <param name="targetFolderPath"></param>
		/// <param name="sameTrgetFolder"></param>
		/// <param name="convertFileWildCard"></param>
        public static void WalkDirectoryTree(FileConvertor fileConverter, System.IO.DirectoryInfo root, string targetFolderPath, bool sameTrgetFolder, string convertFileWildCard)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles(convertFileWildCard);
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
				Console.WriteLine(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo file in files)
                {
					Convert2JPG(fileConverter, file.FullName, targetFolderPath, convertFileWildCard.Length);
                    if (sameTrgetFolder)
                    {
						file.Delete();
                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    if (!sameTrgetFolder)
                    {
						//Create a new sub folder under target folder path
                        string newPath = System.IO.Path.Combine(targetFolderPath, dirInfo.Name);
                        //Create the sub folder
                        System.IO.Directory.CreateDirectory(newPath);
                        //Resursive call for each subdirectory.
						WalkDirectoryTree(fileConverter, dirInfo, newPath, sameTrgetFolder, convertFileWildCard);
                    }
                    else
                    {
                        // Resursive call for each subdirectory.
						WalkDirectoryTree(fileConverter, dirInfo, dirInfo.FullName, sameTrgetFolder, convertFileWildCard);
                    }
                    
                }

            }
        }

        public static void mulipuleFileNomTimes(int num, string originalFileName, string sourceFile, string dllTargetPath)
        {
            
            for (int i = 0; i < num; i++ )
            {
                string copyDllName = originalFileName + i + "copy.ps";
                string destFile = System.IO.Path.Combine(dllTargetPath, copyDllName);   
                System.IO.File.Copy(sourceFile, destFile, true);
            }
		 
		 * 
		 * 
		 * for (int i = 2; i <= 1000; i++)
			{
				string copyDllName = i + ".jpg";
				string destFile = System.IO.Path.Combine("C:\\Users\\hadasg\\Desktop\\Features\\vPDFIIRForJPG\\Performance Issue\\Slim JPGs\\Assets", copyDllName);
				System.IO.File.Copy("C:\\Users\\hadasg\\Desktop\\Features\\vPDFIIRForJPG\\Performance Issue\\Slim JPGs\\Assets\\1.jpg", destFile, true);
			}
           
        }
		 */
	}
}
