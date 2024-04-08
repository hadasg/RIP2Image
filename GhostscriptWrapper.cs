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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RIP2Image
{
	/// <summary>
	/// Ghostscript wrapper for 64/32 bit.
	/// </summary>
	internal class GhostscriptWrapper : IDisposable
	{
		/// <summary>
		/// The Ghostscript dll name
		/// </summary>
		static private readonly string s_GsDllName = "gsdll64";

		/// <summary>
		/// The Ghostscript last used instance id.
		/// </summary>
		static private int s_InstanceId = 0;

		/// <summary>
		/// Pointer to Ghostscript instance.
		/// </summary>
		private IntPtr m_Instance = IntPtr.Zero;

		/// <summary>
		/// The Ghostscript instance id.
		/// </summary>
		private int m_InstanceId = 0;

		/// <summary>
		/// The Ghostscript instance id.
		/// </summary>
		public int InstanceId
		{
			get
			{
				return m_InstanceId;
			}
		}

		/// <summary>
		/// The Ghostscript encoding.
		/// </summary>
		private gsEncoding m_ArgEncoding = gsEncoding.GS_ARG_ENCODING_LOCAL;

		/// <summary>
		/// The Ghostscript encoding.
		/// </summary>
		public gsEncoding ArgEncoding
		{
			get
			{
				return m_ArgEncoding;
			}
		}

		/// <summary>
		/// Ghostscript dll path.
		/// </summary>
		private string m_GSDllFile;

		/// <summary>
		/// Ghostscript dll path.
		/// </summary>
		public string GSDllFile
		{
			get
			{
				return m_GSDllFile;
			}
		}

		/// <summary>
		/// Ghostscript dummy output path.
		/// </summary>
		private string m_GSDummyOutputFile;

		/// <summary>
		/// Ghostscript dummy output path.
		/// </summary>
		public string GSDummyOutputFile
		{
			get
			{
				return m_GSDummyOutputFile;
			}
		}

		public enum GSDummyInputType
		{
			None,
			EPS,
			PDF
		}

		/// <summary>
		/// Ghostscript dummy input path.
		/// </summary>
		private string m_GSDummyInputFile;

		/// <summary>
		/// Ghostscript dummy input path.
		/// </summary>
		public string GSDummyInputFile
		{
			get
			{
				return m_GSDummyInputFile;
			}
		}

		/// <summary>
		/// Pointer to dynamic loading DLL.
		/// </summary>
		private IntPtr m_GSLoadedDll;

		/// <summary>
		/// A partial log message
		/// </summary>
		private string m_LogMessagePart;

		/// <summary>
		/// A partial log error message
		/// </summary>
		private string m_LogErrorMessagePart;

		/// <summary>
		/// Indication that init was called and exit was not
		/// </summary>
		private bool m_NeedExit = false;

		/// <summary>
		/// constructor.
		/// </summary>
		public GhostscriptWrapper(GSDummyInputType inGSDummyInputType)
		{
			m_InstanceId = Interlocked.Increment(ref s_InstanceId);

			string assemblyFolder = System.Reflection.Assembly.GetExecutingAssembly().Location;
			assemblyFolder = Path.GetDirectoryName(assemblyFolder);

			string sourceFile = Path.Combine(assemblyFolder, s_GsDllName + ".dll");
			m_GSDllFile = Path.Combine(assemblyFolder, "RIP2ImageGSDlls", s_GsDllName + "-" + m_InstanceId.ToString() + ".dll");
			m_GSDummyOutputFile = Path.Combine(assemblyFolder, "RIP2ImageGSDlls", s_GsDllName + "-" + m_InstanceId.ToString() + "-dummyoutput");

			switch(inGSDummyInputType)
			{
				case GSDummyInputType.None:
					m_GSDummyInputFile = Path.Combine(assemblyFolder, "Empty.none");
					break;
				case GSDummyInputType.EPS:
					m_GSDummyInputFile = Path.Combine(assemblyFolder, "Empty.eps");
					break;
				case GSDummyInputType.PDF:
					m_GSDummyInputFile = Path.Combine(assemblyFolder, "Empty.pdf");
					break;
			}

			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(m_GSDllFile)))
					Directory.CreateDirectory(Path.GetDirectoryName(m_GSDllFile));
			}
			catch { }

			File.Copy(sourceFile, m_GSDllFile, true);

			m_GSLoadedDll = LoadLibrary(m_GSDllFile);
			if (m_GSLoadedDll == IntPtr.Zero)
			{
				Logger.LogError("GhostscriptWrapper.GhostscriptWrapper - failed to load dll {0} for Instance {1}", m_GSDllFile, m_InstanceId);
				return;
			}

			IntPtr AddressOfFunctionToCall = IntPtr.Zero;

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_new_instance");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_new_instance = Marshal.GetDelegateForFunctionPointer<gsapi_new_instance_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_set_arg_encoding");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_set_arg_encoding = Marshal.GetDelegateForFunctionPointer<gsapi_set_arg_encoding_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_init_with_args");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_init_with_args = Marshal.GetDelegateForFunctionPointer<gsapi_init_with_args_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_exit");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_exit = Marshal.GetDelegateForFunctionPointer<gsapi_exit_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_delete_instance");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_delete_instance = Marshal.GetDelegateForFunctionPointer<gsapi_delete_instance_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_run_string_begin");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_run_string_begin = Marshal.GetDelegateForFunctionPointer<gsapi_run_string_begin_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_run_string_continue");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_run_string_continue = Marshal.GetDelegateForFunctionPointer<gsapi_run_string_continue_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_run_string_end");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_run_string_end = Marshal.GetDelegateForFunctionPointer<gsapi_run_string_end_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_run_string_with_length");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_run_string_with_length = Marshal.GetDelegateForFunctionPointer<gsapi_run_string_with_length_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_run_string");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_run_string = Marshal.GetDelegateForFunctionPointer<gsapi_run_string_delegate>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(m_GSLoadedDll, "gsapi_set_stdio");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_gsapi_set_stdio = Marshal.GetDelegateForFunctionPointer<gsapi_set_stdio_delegate>(AddressOfFunctionToCall);

			gs_error_type code = gsapi_new_instance();
			if (code != gs_error_type.gs_error_ok)
				Logger.LogError("GhostscriptWrapper.constructor - gsapi_new_instance return error {0} for Instance {1}", code.ToString(), m_InstanceId);

			code = gsapi_set_arg_encoding(gsEncoding.GS_ARG_ENCODING_UTF8);
			if (code != gs_error_type.gs_error_ok)
				Logger.LogError("GhostscriptWrapper.constructor - gsapi_set_arg_encoding return error {0} for Instance {1}", code.ToString(), m_InstanceId);

			if (ConfigurationManager.AppSettings["ReDirectStdio"] == "true")
			{
				code = gsapi_set_stdio();
				if (code != gs_error_type.gs_error_ok)
					Logger.LogError("GhostscriptWrapper.constructor - gsapi_set_stdio return error {0} for Instance {1}", code.ToString(), m_InstanceId);
			}
		}

		/// <summary>
		/// dispose implementation
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (m_Instance != IntPtr.Zero)
			{
				if (m_NeedExit)
				{
					gs_error_type code = gsapi_exit();
					if (code != gs_error_type.gs_error_ok)
						Logger.LogError("GhostscriptWrapper.Dispose - gsapi_exit return error {0} for Instance {1}", code.ToString(), m_InstanceId);
				}

				gsapi_delete_instance();
			}
			
			if(m_GSLoadedDll != IntPtr.Zero)
			{
				if (!FreeLibrary(m_GSLoadedDll))
					Logger.LogError("GhostscriptWrapper.Dispose - failed to unload {0} for Instance {1}", m_GSDllFile, m_InstanceId);
				m_GSLoadedDll = IntPtr.Zero;

				try
				{
					File.Delete(m_GSDllFile);
				}
				catch (System.Exception ex)
				{
					Logger.LogError("GhostscriptWrapper.Dispose - {0} for Instance {1}", Logger.GetMostInnerMessage(ex), m_InstanceId);
				}

				try
				{
					if(File.Exists(m_GSDummyOutputFile))
						File.Delete(m_GSDummyOutputFile);
				}
				catch (System.Exception ex)
				{
					Logger.LogWarning("GhostscriptWrapper.Dispose - {0} for Instance {1}", Logger.GetMostInnerMessage(ex), m_InstanceId);
				}
			}
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

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);

		public enum gsEncoding : int
		{
			GS_ARG_ENCODING_LOCAL = 0,
			GS_ARG_ENCODING_UTF8 = 1,
			GS_ARG_ENCODING_UTF16LE = 2
		};

		public enum gs_error_type : int
		{
			gs_error_ok = 0,
			gs_error_unknownerror = -1, /* unknown error */
			gs_error_dictfull = -2,
			gs_error_dictstackoverflow = -3,
			gs_error_dictstackunderflow = -4,
			gs_error_execstackoverflow = -5,
			gs_error_interrupt = -6,
			gs_error_invalidaccess = -7,
			gs_error_invalidexit = -8,
			gs_error_invalidfileaccess = -9,
			gs_error_invalidfont = -10,
			gs_error_invalidrestore = -11,
			gs_error_ioerror = -12,
			gs_error_limitcheck = -13,
			gs_error_nocurrentpoint = -14,
			gs_error_rangecheck = -15,
			gs_error_stackoverflow = -16,
			gs_error_stackunderflow = -17,
			gs_error_syntaxerror = -18,
			gs_error_timeout = -19,
			gs_error_typecheck = -20,
			gs_error_undefined = -21,
			gs_error_undefinedfilename = -22,
			gs_error_undefinedresult = -23,
			gs_error_unmatchedmark = -24,
			gs_error_VMerror = -25,     /* must be the last Level 1 error */

			/* ------ Additional Level 2 errors (also in DPS, ------ */

			gs_error_configurationerror = -26,
			gs_error_undefinedresource = -27,

			gs_error_unregistered = -28,
			gs_error_invalidcontext = -29,
			/* invalidid is for the NeXT DPS extension. */
			gs_error_invalidid = -30,

			/* We need a specific stackoverflow error for the PDF interpreter to avoid dropping into
			 * the Postscript interpreter's stack extending code, when the PDF interpreter is called from
			 * Postscript
			 */
			gs_error_pdf_stackoverflow = -31,

			/* Internal error for the C-based PDF interpreter, to indicate a circular PDF reference */
			gs_error_circular_reference = -32,

			/* ------ Pseudo-errors used internally ------ */

			gs_error_hit_detected = -99,

			gs_error_Fatal = -100,
			/*
			 * Internal code for the .quit operator.
			 * The real quit code is an integer on the operand stack.
			 * gs_interpret returns this only for a .quit with a zero exit code.
			 */
			gs_error_Quit = -101,

			/*
			 * Internal code for a normal exit from the interpreter.
			 * Do not use outside of interp.c.
			 */
			gs_error_InterpreterExit = -102,

			/* Need the remap color error for high level pattern support */
			gs_error_Remap_Color = -103,

			/*
			 * Internal code to indicate we have underflowed the top block
			 * of the e-stack.
			 */
			gs_error_ExecStackUnderflow = -104,

			/*
			 * Internal code for the vmreclaim operator with a positive operand.
			 * We need to handle this as an error because otherwise the interpreter
			 * won't reload enough of its state when the operator returns.
			 */
			gs_error_VMreclaim = -105,

			/*
			 * Internal code for requesting more input from run_string.
			 */
			gs_error_NeedInput = -106,

			/*
			 * Internal code to all run_string to request that the data is rerun
			 * using run_file.
			 */
			gs_error_NeedFile = -107,

			/*
			 * Internal code for a normal exit when usage info is displayed.
			 * This allows Window versions of Ghostscript to pause until
			 * the message can be read.
			 */
			gs_error_Info = -110,

			/* A special 'error', like reamp color above. This is used by a subclassing
			 * device to indicate that it has fully processed a device method, and parent
			 * subclasses should not perform any further action. Currently this is limited
			 * to compositor creation.
			 */
			gs_error_handled = -111
		}

		private byte[] GetEncodedBytes(string str)
		{
			if (str == null)
				return new byte[0];

			switch(m_ArgEncoding)
			{
				case gsEncoding.GS_ARG_ENCODING_LOCAL:
					return Encoding.ASCII.GetBytes(str);
				case gsEncoding.GS_ARG_ENCODING_UTF8:
					return Encoding.UTF8.GetBytes(str);
				case gsEncoding.GS_ARG_ENCODING_UTF16LE:
					return Encoding.Unicode.GetBytes(str);
			}

			return Encoding.ASCII.GetBytes(str);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_new_instance_delegate(out IntPtr pinstance, IntPtr caller_handle);

		private gsapi_new_instance_delegate m_gsapi_new_instance;

		private gs_error_type gsapi_new_instance()
		{
			return (gs_error_type)m_gsapi_new_instance(out m_Instance, IntPtr.Zero);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_set_arg_encoding_delegate(IntPtr instance, int encoding);

		private gsapi_set_arg_encoding_delegate m_gsapi_set_arg_encoding;

		private gs_error_type gsapi_set_arg_encoding(gsEncoding encoding)
		{
			m_ArgEncoding = encoding;
			return (gs_error_type)m_gsapi_set_arg_encoding(m_Instance, (int)encoding);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_init_with_args_delegate(IntPtr instance, int argc, IntPtr argv);

		private gsapi_init_with_args_delegate m_gsapi_init_with_args;

		public gs_error_type gsapi_init_with_args(params string[] args)
		{
			m_NeedExit = true;

			GCHandle[] argsGCHandles = new GCHandle[args.Length];
			IntPtr[] argsPtrs = new IntPtr[args.Length];
			for (int i = 0; i < args.Length; ++i)
			{
				argsGCHandles[i] = GCHandle.Alloc(GetEncodedBytes(args[i]), GCHandleType.Pinned);
				argsPtrs[i] = argsGCHandles[i].AddrOfPinnedObject();
			}
			GCHandle argsGCHandle = GCHandle.Alloc(argsPtrs, GCHandleType.Pinned);

			gs_error_type retVal = (gs_error_type)m_gsapi_init_with_args(m_Instance, args.Length, argsGCHandle.AddrOfPinnedObject());

			for (int i = 0; i < argsGCHandles.Length; ++i)
				argsGCHandles[i].Free();
			argsGCHandle.Free();

			return retVal;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_exit_delegate(IntPtr instance);

		private gsapi_exit_delegate m_gsapi_exit;

		public gs_error_type gsapi_exit()
		{
			m_NeedExit = false;
			return (gs_error_type)m_gsapi_exit(m_Instance);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate void gsapi_delete_instance_delegate(IntPtr instance);

		private gsapi_delete_instance_delegate m_gsapi_delete_instance;

		private void gsapi_delete_instance()
		{
			m_gsapi_delete_instance(m_Instance);
			m_Instance = IntPtr.Zero;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_run_string_begin_delegate(IntPtr instance, int usererr, ref int exitcode);

		private gsapi_run_string_begin_delegate m_gsapi_run_string_begin;

		public gs_error_type gsapi_run_string_begin()
		{
			int exitcode = 0;
			return (gs_error_type)m_gsapi_run_string_begin(m_Instance, 0, ref exitcode);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_run_string_continue_delegate(IntPtr instance, IntPtr command, int count, int usererr, ref int exitcode);

		private gsapi_run_string_continue_delegate m_gsapi_run_string_continue;

		public gs_error_type gsapi_run_string_continue(string command)
		{
			byte[] commandBytes = GetEncodedBytes(command);
			GCHandle messageGCHandle = GCHandle.Alloc(commandBytes, GCHandleType.Pinned);
			int exitcode = 0;
			gs_error_type retVal = (gs_error_type)m_gsapi_run_string_continue(m_Instance, messageGCHandle.AddrOfPinnedObject(), commandBytes.Length, 0, ref exitcode);
			messageGCHandle.Free();
			return retVal;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_run_string_end_delegate(IntPtr instance, int usererr, ref int exitcode);

		private gsapi_run_string_end_delegate m_gsapi_run_string_end;

		public gs_error_type gsapi_run_string_end()
		{
			int exitcode = 0;
			return (gs_error_type)m_gsapi_run_string_end(m_Instance, 0, ref exitcode);
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_run_string_with_length_delegate(IntPtr instance, IntPtr command, uint length, int usererr, ref int exitcode);

		private gsapi_run_string_with_length_delegate m_gsapi_run_string_with_length;

		public gs_error_type gsapi_run_string_with_length(string command)
		{
			byte[] commandBytes = GetEncodedBytes(command);
			GCHandle messageGCHandle = GCHandle.Alloc(commandBytes, GCHandleType.Pinned);
			int exitcode = 0;
			gs_error_type retVal = (gs_error_type)m_gsapi_run_string_with_length(m_Instance, messageGCHandle.AddrOfPinnedObject(), (uint)commandBytes.Length, 0, ref exitcode);
			messageGCHandle.Free();
			return retVal;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_run_string_delegate(IntPtr instance, IntPtr command, int usererr, ref int exitcode);

		private gsapi_run_string_delegate m_gsapi_run_string;

		public gs_error_type gsapi_run_string(string command)
		{
			byte[] commandBytes = GetEncodedBytes(command);
			GCHandle messageGCHandle = GCHandle.Alloc(commandBytes, GCHandleType.Pinned);
			int exitcode = 0;
			gs_error_type retVal = (gs_error_type)m_gsapi_run_string(m_Instance, messageGCHandle.AddrOfPinnedObject(), 0, ref exitcode);
			messageGCHandle.Free();
			return retVal;
		}

		public delegate int gs_stdio_handler(IntPtr caller_handle, IntPtr buffer, int len);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate int gsapi_set_stdio_delegate(IntPtr instance, gs_stdio_handler stdin, gs_stdio_handler stdout, gs_stdio_handler stderr);

		private gsapi_set_stdio_delegate m_gsapi_set_stdio;

		private gs_error_type gsapi_set_stdio()
		{
			return (gs_error_type)m_gsapi_set_stdio(m_Instance, stdin_callback, stdout_callback, stderr_callback);
		}

		private int stdin_callback(IntPtr handle, IntPtr pointer, int count)
		{
			Marshal.PtrToStringAnsi(pointer, count);
			return count;
		}

		char[] s_LineBreak = new char[] { '\n' };

		private int stdout_callback(IntPtr handle, IntPtr pointer, int count)
		{
			string message = Marshal.PtrToStringAnsi(pointer, count);
			if (string.IsNullOrEmpty(message))
				return count;

			if (!string.IsNullOrEmpty(m_LogMessagePart))
				message = m_LogMessagePart + message;
			m_LogMessagePart = null;

			string[] lines = message.Split(s_LineBreak, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < lines.Length; ++i)
			{
				if(i == (lines.Length-1) && message.Last() != s_LineBreak[0])
				{
					m_LogMessagePart = lines[i];
					continue;
				}
				Logger.LogExtendedMessage("GS message for instance {0}: {1}", m_InstanceId, lines[i]);
			}

			return count;
		}

		private int stderr_callback(IntPtr handle, IntPtr pointer, int count)
		{
			string message = Marshal.PtrToStringAnsi(pointer, count);
			if (string.IsNullOrEmpty(message))
				return count;

			if (!string.IsNullOrEmpty(m_LogErrorMessagePart))
				message = m_LogErrorMessagePart + message;
			m_LogErrorMessagePart = null;

			string[] lines = message.Split(s_LineBreak, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < lines.Length; ++i)
			{
				if (i == (lines.Length - 1) && message.Last() != s_LineBreak[0])
				{
					m_LogErrorMessagePart = lines[i];
					continue;
				}
				Logger.LogError("GS error message for instance {0}: {1}", m_InstanceId, lines[i]);
			}

			return count;
		}

		/// <summary>
		/// Delete dlls's directory
		/// </summary>
		public static void DeleteDllDirectory()
		{
			try
			{
				string fullExeNameAndPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
				string exeDirectory = System.IO.Path.GetDirectoryName(fullExeNameAndPath);
				string targetPath = System.IO.Path.Combine(exeDirectory, "RIP2ImageGSDlls");
				if (Directory.Exists(targetPath))
					System.IO.Directory.Delete(targetPath, true);
			}
			catch (System.Exception ex)
			{
				Logger.LogError("GhostscriptWrapper.DeleteDllDirectory - {0}", Logger.GetMostInnerMessage(ex));
			}
		}
	}
}
