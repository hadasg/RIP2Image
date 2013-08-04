/*******************************************************************************
Description:
	Ghostscript wrapper.

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
using System.Runtime.InteropServices;
using System.Text;

namespace GhostscriptService
{
	/// <summary>
	/// Ghostscript wrapper for 64/32 bit.
	/// </summary>
	internal class GhostscriptWrapper
	{

		#region Ghostscript 64 bit import Dll

		/// <summary>
		/// Ghostscript64 - Generate new instance of Ghostscript.
		/// </summary>
		/// <param name="pinstance"></param>
		/// <param name="caller_handle"></param>
		/// <returns></returns>
		[DllImport("gsdll64.dll", EntryPoint = "gsapi_new_instance")]
		private static extern int CreateNewGhostscriptInstance64(out IntPtr pinstance, IntPtr caller_handle);

		/// <summary>Ghostscript64 - Inisilaize Ghostscript arguments</summary>
		/// <param name="instance"></param><param name="argc"></param><param name="argv"></param>
		/// <returns>0 if is ok</returns>
		[DllImport("gsdll64.dll", EntryPoint = "gsapi_init_with_args")]
		private static extern int InitInstanceWithArgs64(IntPtr instance, int argc, IntPtr argv);

		/// <summary>Ghostscript64 - Exit the interpreter</summary>
		/// <param name="instance"></param><returns></returns>
		[DllImport("gsdll64.dll", EntryPoint = "gsapi_exit")]
		private static extern int Exit64(IntPtr instance);

		/// <summary>Ghostscript64 - Destroy an instance of Ghostscript.</summary>
		/// <param name="instance"></param>
		[DllImport("gsdll64.dll", EntryPoint = "gsapi_delete_instance")]
		private static extern void DeleteInstance64(IntPtr instance);

		/// <summary>
		/// Ghostscript64 - Run Ghostscript command.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="inString"></param>
		/// <param name="user_errors"></param>
		/// <param name="pexit_code"></param>
		[DllImport("gsdll64.dll", EntryPoint = "gsapi_run_string")]
		private static extern void RunCommandStringOnInstance64(IntPtr gsInstance, IntPtr commandString, int user_errors, out IntPtr pexit_code);

		#endregion

		#region Ghostscript 32 bit import Dll

		/// <summary>
		/// Ghostscript32 - Generate new instance of Ghostscript.
		/// </summary>
		/// <param name="pinstance"></param>
		/// <param name="caller_handle"></param>
		/// <returns></returns>
		[DllImport("gsdll32.dll", EntryPoint = "gsapi_new_instance")]
		private static extern int CreateNewGhostscriptInstance32(out IntPtr pinstance, IntPtr caller_handle);

		/// <summary>Ghostscript32 - Inisilaize Ghostscript arguments</summary>
		/// <param name="instance"></param><param name="argc"></param><param name="argv"></param>
		/// <returns>0 if is ok</returns>
		[DllImport("gsdll32.dll", EntryPoint = "gsapi_init_with_args")]
		private static extern int InitInstanceWithArgs32(IntPtr instance, int argc, IntPtr argv);

		/// <summary>Ghostscript32 - Exit the interpreter</summary>
		/// <param name="instance"></param><returns></returns>
		[DllImport("gsdll32.dll", EntryPoint = "gsapi_exit")]
		private static extern int Exit32(IntPtr instance);

		/// <summary>Ghostscript32 - Destroy an instance of Ghostscript.</summary>
		/// <param name="instance"></param>
		[DllImport("gsdll32.dll", EntryPoint = "gsapi_delete_instance")]
		private static extern void DeleteInstance32(IntPtr instance);

		/// <summary>
		/// Ghostscript32 - Run Ghostscript command.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="inString"></param>
		/// <param name="user_errors"></param>
		/// <param name="pexit_code"></param>
		[DllImport("gsdll32.dll", EntryPoint = "gsapi_run_string")]
		private static extern void RunCommandStringOnInstance32(IntPtr gsInstance, IntPtr commandString, int user_errors, out IntPtr pexit_code);

		#endregion

		#region Class Members

		/// <summary>
		/// Pointer to Ghostscript instance.
		/// </summary>
		private IntPtr m_Instance = IntPtr.Zero;

		/// <summary>
		/// Is 64 Bit Process?
		/// </summary>
		bool m_is64BitProcess = Environment.Is64BitProcess;

		#endregion

		#region Methods

		/// <summary>
		/// Constructor - getting Ghostscript parameters as input. 
		/// </summary>
		/// <param name="parametersList"></param>
		public GhostscriptWrapper(string[] inParameters)
		{
			m_Instance = IntPtr.Zero;
			Init(inParameters);
		}

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

			// create Ghostscript instance.
			if (m_is64BitProcess)
				CreateNewGhostscriptInstance64(out m_Instance, IntPtr.Zero);
			else
				CreateNewGhostscriptInstance32(out m_Instance, IntPtr.Zero);

			// create the parameters as pinned allocated.
			GCHandle[] parametersGCHandle = new GCHandle[inParameters.Length];
			GCHandle argsGCHandle = Parameters2IntPtr(inParameters, parametersGCHandle);
			IntPtr argvPointer = argsGCHandle.AddrOfPinnedObject();

			// initialize the instance.
			int initReturnValue;
			if (m_is64BitProcess)
				initReturnValue = InitInstanceWithArgs64(m_Instance, inParameters.Length, argvPointer);
			else
				initReturnValue = InitInstanceWithArgs32(m_Instance, inParameters.Length, argvPointer);
			
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
				if (m_is64BitProcess)
				{
					Exit64(m_Instance);
					DeleteInstance64(m_Instance);
				}
				else
				{
					Exit32(m_Instance);
					DeleteInstance32(m_Instance);
				}
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
			IntPtr exitcode;

			// convert parameters to byte
			if (inCommand == null) inCommand = String.Empty;
			object commandANSI = Encoding.Default.GetBytes(inCommand);

			// create parameters as pinned allocated.
			GCHandle commandGCHandle = GCHandle.Alloc(commandANSI, GCHandleType.Pinned);
			IntPtr commandPointer = commandGCHandle.AddrOfPinnedObject();

			//Run Ghostscript command
			if (m_is64BitProcess)
				RunCommandStringOnInstance64(m_Instance, commandPointer, 0, out exitcode);
			else
				RunCommandStringOnInstance32(m_Instance, commandPointer, 0, out exitcode);
		
			// Clear Parameter
			commandGCHandle.Free();

			return exitcode.Equals(IntPtr.Zero);
		}

		#endregion

		#region Help Method

		/// <summary>
		/// Convert GS parameters to pointers in order to send them to GS functions.
		/// </summary>
		/// <param name="inParameters"></param>
		/// <param name="inParametersGCHandle"></param>
		/// <returns></returns>
		private GCHandle Parameters2IntPtr(string[] inParameters, GCHandle[] inParametersGCHandle)
		{
			int intElementCount = inParameters.Length;
			object[] _ANSIArgs = new object[intElementCount];
			IntPtr[] argsPointers = new IntPtr[intElementCount];

			// Convert parameters
			for (int intCounter = 0; intCounter < intElementCount; intCounter++)
			{
				// convert parameters to byte
				if (inParameters[intCounter] == null) inParameters[intCounter] = String.Empty;
				_ANSIArgs[intCounter] = Encoding.Default.GetBytes(inParameters[intCounter]);

				// create parameters as pinned allocated.
				inParametersGCHandle[intCounter] = GCHandle.Alloc(_ANSIArgs[intCounter], GCHandleType.Pinned);
				argsPointers[intCounter] = inParametersGCHandle[intCounter].AddrOfPinnedObject();
			}

			GCHandle argsGChandle = GCHandle.Alloc(argsPointers, GCHandleType.Pinned);

			return argsGChandle;
		}

		#endregion

	}
}
