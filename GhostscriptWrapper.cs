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
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace RIP2Jmage
{
	/// <summary>
	/// Ghostscript wrapper for 64/32 bit.
	/// </summary>
	internal class GhostscriptWrapper
	{

		#region Native methods import Dll

		/// <summary>
		/// Returns a handle to the dll in question.
		/// </summary>
		/// <param name="dllToLoad"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string dllToLoad);

		/// <summary>
		/// Obtain the address of an exported function within the previously loaded dll.
		/// </summary>
		/// <param name="hModule"></param>
		/// <param name="procedureName"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		/// <summary>
		/// Releases the DLL loaded by the LoadLibrary function.
		/// </summary>
		/// <param name="hModule"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		public static extern bool FreeLibrary(IntPtr hModule);

		#endregion


		#region Ghostscript import Dll

		/// <summary>
		/// Generate new instance of Ghostscript.
		/// </summary>
		/// <param name="pinstance"></param>
		/// <param name="caller_handle"></param>
		/// <returns></returns>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int CreateNewGhostscriptInstance(out IntPtr pinstance, IntPtr caller_handle);


		/// <summary>Inisilaize Ghostscript arguments.</summary>
		/// <param name="instance"></param><param name="argc"></param><param name="argv"></param>
		/// <returns>0 if is ok</returns>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int InitInstanceWithArgs(IntPtr instance, int argc, IntPtr argv);


		/// <summary>ExitGSInstance the interpreter.</summary>
		/// <param name="instance"></param><returns></returns>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int Exit(IntPtr instance);


		/// <summary>Destroy an instance of Ghostscript.</summary>
		/// <param name="instance"></param>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate void DeleteInstance(IntPtr instance);

		/// <summary>
		/// Run Ghostscript command.
		/// </summary>
		/// <param name="gsInstance"></param>
		/// <param name="commandString"></param>
		/// <param name="user_errors"></param>
		/// <param name="pexit_code"></param>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate void RunCommandStringOnInstance(IntPtr gsInstance, IntPtr commandString, int user_errors, out IntPtr pexit_code);

		#endregion



		#region Class Members

		/// <summary>
		/// Pointer to Ghostscript instance.
		/// </summary>
		private IntPtr m_Instance = IntPtr.Zero;

		/// <summary>
		/// Ghostscript dll path.
		/// </summary>
		private string m_GSDllPath;

		/// <summary>
		/// Pointer to dynamic loading DLL.
		/// </summary>
		private IntPtr m_LibraryPointerDll;

		#endregion


		#region Ghostscript Wrapper Functions

		/// <summary>
		///  Create new Ghostscript instance using Ghostscript import Dll.
		/// </summary>
		private void CreateNewGSInstance()
		{
			IntPtr pAddressOfFunctionToCall = GetProcAddress(m_LibraryPointerDll, "gsapi_new_instance");
			CreateNewGhostscriptInstance createNewGhostscriptInstance = (CreateNewGhostscriptInstance)Marshal.GetDelegateForFunctionPointer(
																					pAddressOfFunctionToCall,
																					typeof(CreateNewGhostscriptInstance));

			createNewGhostscriptInstance(out m_Instance, IntPtr.Zero);
		}

		/// <summary>
		/// Inisilaize Ghostscript instance with arguments using Ghostscript import Dll.
		/// </summary>
		/// <param name="inParametersLength"></param>
		/// <param name="inArgvPointer"></param>
		/// <returns></returns>
		private int InitGSInstanceWithArgs(int inParametersLength, IntPtr inArgvPointer)
		{
			IntPtr pAddressOfFunctionToCall = GetProcAddress(m_LibraryPointerDll, "gsapi_init_with_args");
			InitInstanceWithArgs initInstanceWithArgs = (InitInstanceWithArgs)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(InitInstanceWithArgs));
			return initInstanceWithArgs(m_Instance, inParametersLength, inArgvPointer);
		}

		/// <summary>
		/// Exit Ghostscript Instance using Ghostscript import Dll.
		/// </summary>
		private void ExitGSInstance()
		{
			IntPtr pAddressOfFunctionToCall = GetProcAddress(m_LibraryPointerDll, "gsapi_exit");
			Exit exit = (Exit)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Exit));
			exit(m_Instance);
		}

		/// <summary>
		/// Delete Ghostscript Instance using Ghostscript import Dll.
		/// </summary>
		private void DeleteGSInstance()
		{
			IntPtr pAddressOfFunctionToCall = GetProcAddress(m_LibraryPointerDll, "gsapi_delete_instance");
			DeleteInstance deleteInstance = (DeleteInstance)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(DeleteInstance));
			deleteInstance(m_Instance);
		}

		/// <summary>
		/// Run Ghostscript command using Ghostscript import Dll.
		/// </summary>
		/// <param name="inCommandPointer"></param>
		/// <returns></returns>
		private bool RunGSCommandStringOnInstance(IntPtr inCommandPointer)
		{
			IntPtr exitcode;

			IntPtr pAddressOfFunctionToCall = GetProcAddress(m_LibraryPointerDll, "gsapi_run_string");
			RunCommandStringOnInstance runCommandStringOnInstance = (RunCommandStringOnInstance)Marshal.GetDelegateForFunctionPointer(
																					pAddressOfFunctionToCall,
																					typeof(RunCommandStringOnInstance));
			runCommandStringOnInstance(m_Instance, inCommandPointer, 0, out exitcode);

			return exitcode.Equals(IntPtr.Zero);
		}

		#endregion


		#region Methods

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public GhostscriptWrapper()
		{
			m_Instance = IntPtr.Zero;
		}

		/// <summary>
		/// Init Ghostscript - making the instance reusable.
		/// </summary>
		/// <param name="inParameters"></param>
		/// <returns></returns>
		public bool Init(string[] inParameters)
		{
			Cleanup();

			// Generate and load unique Dll 
			m_GSDllPath = GenerateUniqueDll();
			m_LibraryPointerDll = LoadLibrary(m_GSDllPath);
			// Error report - couldn't load library
			if (m_LibraryPointerDll == IntPtr.Zero)
				return false;

			// Create new Ghostscript instance.
			CreateNewGSInstance();

			// create the parameters as pinned allocated.
			GCHandle[] parametersGCHandle = new GCHandle[inParameters.Length];
			GCHandle argsGCHandle = Parameters2IntPtr(inParameters, parametersGCHandle);
			IntPtr argvPointer = argsGCHandle.AddrOfPinnedObject();

			// initialize the instance.
			int initReturnValue = InitGSInstanceWithArgs(inParameters.Length, argvPointer);

			// clear memory.
			for (int intCounter = 0; intCounter < parametersGCHandle.Length; intCounter++)
				parametersGCHandle[intCounter].Free();
			argsGCHandle.Free();

			return !(initReturnValue == -1);
		}


		/// <summary>
		/// clean up Ghostscript instance memory.
		/// </summary>
		public void Cleanup()
		{
			if (m_Instance != IntPtr.Zero)
			{
				ExitGSInstance();

				DeleteGSInstance();

				// Free loaded library and delete Dll file. 
				FreeLibrary(m_LibraryPointerDll);
				File.Delete(m_GSDllPath);
			}
			m_Instance = IntPtr.Zero;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~GhostscriptWrapper()
		{
			Cleanup();
		}

		/// <summary>
		/// Run Ghostscript command.
		/// </summary>
		/// <param name="command"></param>
		public bool RunCommand(string inCommand)
		{
			// convert parameters to byte
			if (inCommand == null) inCommand = String.Empty;
			object commandUTF8 = Encoding.UTF8.GetBytes(inCommand);

			// create parameters as pinned allocated.
			GCHandle commandGCHandle = GCHandle.Alloc(commandUTF8, GCHandleType.Pinned);
			IntPtr commandPointer = commandGCHandle.AddrOfPinnedObject();

			//Run Ghostscript command
			bool runGSCommandSucceed = RunGSCommandStringOnInstance(commandPointer);

			// Clear Parameter
			commandGCHandle.Free();

			return runGSCommandSucceed;
		}

		#endregion

		#region Help Method

		/// <summary>
		///	Generate unique Dll and return its path. 
		/// </summary>
		public string GenerateUniqueDll()
		{
			string fullExeNameAndPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string exeDirectory = System.IO.Path.GetDirectoryName(fullExeNameAndPath);

			// The original dll.
			string sourceFile = System.IO.Path.Combine(exeDirectory, "gsdll.dll");

			// Create directory for the copy dll if there isn't.
			string dllTargetPath = System.IO.Path.Combine(exeDirectory, "Dynamic Loading DLL");
			if (!Directory.Exists(dllTargetPath))
				System.IO.Directory.CreateDirectory(dllTargetPath);

			// Generate unique name for the copied Dll.
			string copyDllName = "gsdll" + Guid.NewGuid() + ".dll";
			string destFile = System.IO.Path.Combine(dllTargetPath, copyDllName);

			// Copy. 
			System.IO.File.Copy(sourceFile, destFile, true);

			return destFile;
		}

		/// <summary>
		/// Convert GS parameters to pointers in order to send them to GS functions.
		/// </summary>
		/// <param name="inParameters"></param>
		/// <param name="inParametersGCHandle"></param>
		/// <returns></returns>
		private GCHandle Parameters2IntPtr(string[] inParameters, GCHandle[] inParametersGCHandle)
		{
			int intElementCount = inParameters.Length;
			object[] _UTF8Args = new object[intElementCount];
			IntPtr[] argsPointers = new IntPtr[intElementCount];

			// Convert parameters
			for (int intCounter = 0; intCounter < intElementCount; intCounter++)
			{
				// convert parameters to byte
				if (inParameters[intCounter] == null) inParameters[intCounter] = String.Empty;

				_UTF8Args[intCounter] = Encoding.UTF8.GetBytes(inParameters[intCounter]);

				// create parameters as pinned allocated.
				inParametersGCHandle[intCounter] = GCHandle.Alloc(_UTF8Args[intCounter], GCHandleType.Pinned);
				argsPointers[intCounter] = inParametersGCHandle[intCounter].AddrOfPinnedObject();
			}

			GCHandle argsGChandle = GCHandle.Alloc(argsPointers, GCHandleType.Pinned);

			return argsGChandle;
		}

		#endregion

		#region Static Method

		/// <summary>
		/// Delete dlls's directory
		/// </summary>
		public static void DeleteDllDirectory()
		{
			string fullExeNameAndPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string exeDirectory = System.IO.Path.GetDirectoryName(fullExeNameAndPath);
			string targetPath = System.IO.Path.Combine(exeDirectory, "Dynamic Loading DLL");
			if (Directory.Exists(targetPath))
			{
				System.IO.Directory.Delete(targetPath, true);
			}
		}

		#endregion
	}
}
