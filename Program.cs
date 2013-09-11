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

namespace RIP2Jmage
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
		static void Main()
        {
           //ConvertPDF2JPG("C:\\gs\\PDF\\Folder2\\2Text_graph_image_cmyk_rgb.pdf", "C:\\gs\\PDF\\Folder1");
          //ConvertPDF2JPG("C:\\gs\\PDF\\Folder1\\text_graph_image_cmyk_rgb.pdf", "C:\\gs\\JPG");

          //mulipuleFileNomTimes(10000, "1Record", "C:\\gs\\PS\\1Record.ps", "C:\\gs\\PS");
           
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //string convertFolderPath = "C:\\gs\\PDF";
            //string targetFolderPath = "C:\\gs\\JPG";

			/*
			FileConvertor fileConverter = new FileConvertor();
			*/

			ConverterService convert = new ConverterService();

			Worker workerObject = new Worker();
			Thread workerThread = new Thread(workerObject.DoWork1);
			//workerThread.Start();

			Worker workerObject2 = new Worker();
			Thread workerThread2 = new Thread(workerObject.DoWork2);
			workerThread2.Start();
			
			//convert.ConvertFileType("C:\\gs\\PDF\\Folder2\\2Text_graph_image_cmyk_rgb.pdf", "C:\\gs\\JPG\\Folder2", wild.Length, "jpg");
		
			/*
			convert.ConvertFileTypeNestedFolders(convertFolderPath, targetFolderPath, "*.pdf", "jpg");
            */
			sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.ElapsedMilliseconds/1000);

			//workerThread.Join();
			workerThread2.Join();
			InstancesManager.DeleteDynamicLoadingDLL();


        }

		public class Worker
		{
			// This method will be called when the thread is started.
			public void DoWork1()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();
				
				for (int i = 3; i < 101; i++)
				{
				convert.ConvertPDF2JPG("C:\\gs\\PDFThread\\Folder1\\text_graphic_image - Copy - Copy "+ i +".pdf", "C:\\gs\\JPG\\Folder1", 72, 72, 4, 4, 85);
				}
				
				sw.Stop();
				Console.WriteLine("Elapsed Thread 1={0}", sw.ElapsedMilliseconds / 1000);
			}

			public void DoWork2()
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();

				ConverterService convert = new ConverterService();
				convert.ConvertPDFFolder2JPG("C:\\gs\\XLIMTest\\PDF", "C:\\gs\\XLIMTest\\JPG", "*.pdf", false, true, 72, 72, 4, 4, 85);
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
			string OutputFileFullPath = targetFilePath + fileName;
			
			OutputFileFullPath = OutputFileFullPath.Replace("\\", "\\\\");

			convertFilePath = convertFilePath.Replace("\\", "\\\\");
			fileConverter.Convert(convertFilePath, OutputFileFullPath);
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
           
        }
		 */
    }
}
