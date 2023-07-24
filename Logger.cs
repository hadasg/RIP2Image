using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RIP2Image
{
	internal class Logger
	{
		public static void LogError(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogErrorMethod, FormatMessage(inMessage, args));
		}

		public static void LogWarning(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogWarningMethod, FormatMessage(inMessage, args));
		}

		public static void LogMessage(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogMessageMethod, FormatMessage(inMessage, args));
		}

		public static void LogImportantMessage(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogImportantMessageMethod, FormatMessage(inMessage, args));
		}

		public static void LogPerformance(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogPerformanceMethod, FormatMessage(inMessage, args));
		}

		public static void LogExtendedMessage(string inMessage, params object[] args)
		{
			CallLogMethod(instance.m_LogExtendedMessageMethod, FormatMessage(inMessage, args));
		}

		public static string GetMostInnerMessage(Exception exception)
		{
			if (exception == null)
				return "";
			Exception mostInnerException = exception;
			while (mostInnerException.InnerException != null)
				mostInnerException = mostInnerException.InnerException;
			if (mostInnerException is IndexOutOfRangeException)
				return mostInnerException.Message + " out of range";
			return mostInnerException.Message;
		}

		private static string FormatMessage(string inMessage, params object[] args)
		{
			if (inMessage == null)
				return "null message";
			if (args == null || args.Length <= 0)
				return inMessage;
			return string.Format(inMessage, args);
		}

		/// <summary>
		/// Returns a handle to the dll in question.
		/// </summary>
		/// <param name="dllToLoad"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string dllToLoad);

		/// <summary>
		/// Obtain the address of an exported function within the previously loaded dll.
		/// </summary>
		/// <param name="hModule"></param>
		/// <param name="procedureName"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		/// <summary>
		/// Releases the DLL loaded by the LoadLibrary function.
		/// </summary>
		/// <param name="hModule"></param>
		/// <returns></returns>
		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);

		/// <summary>
		/// Logger function
		/// </summary>
		/// <param name="message"></param>
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate void LoggerMethod(IntPtr message);

		private LoggerMethod m_LogErrorMethod;
		private LoggerMethod m_LogWarningMethod;
		private LoggerMethod m_LogMessageMethod;
		private LoggerMethod m_LogImportantMessageMethod;
		private LoggerMethod m_LogPerformanceMethod;
		private LoggerMethod m_LogExtendedMessageMethod;

		static private Logger instance = new Logger();

		private Logger()
		{
			IntPtr LibraryPointerDll = LoadLibrary("XMPTrace_R.dll");
			if (LibraryPointerDll == IntPtr.Zero)
				return;

			IntPtr AddressOfFunctionToCall = IntPtr.Zero;

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTraceErrorStr");
			if(AddressOfFunctionToCall != IntPtr.Zero)
				m_LogErrorMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTraceWarningStr");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_LogWarningMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTraceMessageStr");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_LogMessageMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTraceImportantMessageStr");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_LogImportantMessageMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTracePerformanceStr");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_LogPerformanceMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);

			AddressOfFunctionToCall = GetProcAddress(LibraryPointerDll, "XMPTraceExtendedMessageStr");
			if (AddressOfFunctionToCall != IntPtr.Zero)
				m_LogExtendedMessageMethod = Marshal.GetDelegateForFunctionPointer<LoggerMethod>(AddressOfFunctionToCall);
		}

		private static void CallLogMethod(LoggerMethod loggerMethod, string inMessage)
		{
			if (loggerMethod == null)
				return;

			GCHandle messageGCHandle = GCHandle.Alloc(Encoding.Unicode.GetBytes(inMessage), GCHandleType.Pinned);
			loggerMethod(messageGCHandle.AddrOfPinnedObject());
			messageGCHandle.Free();
		}
	}
}
