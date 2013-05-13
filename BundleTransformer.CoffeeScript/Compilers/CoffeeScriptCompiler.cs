﻿namespace BundleTransformer.CoffeeScript.Compilers
{
	using System;
	using System.Text;

	using MsieJavaScriptEngine;
	using MsieJavaScriptEngine.ActiveScript;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	using Core;
	using Core.SourceCodeHelpers;
	using CoreStrings = Core.Resources.Strings;

	/// <summary>
	/// CoffeeScript-compiler
	/// </summary>
	internal sealed class CoffeeScriptCompiler : IDisposable
	{
		/// <summary>
		/// Name of resource, which contains a CoffeeScript-library
		/// </summary>
		const string COFFEESCRIPT_LIBRARY_RESOURCE_NAME 
			= "BundleTransformer.CoffeeScript.Resources.coffeescript-combined.min.js";

		/// <summary>
		/// Name of resource, which contains a CoffeeScript-compiler helper
		/// </summary>
		const string CSC_HELPER_RESOURCE_NAME = "BundleTransformer.CoffeeScript.Resources.cscHelper.min.js";

		/// <summary>
		/// Template of function call, which is responsible for compilation
		/// </summary>
		const string COMPILATION_FUNCTION_CALL_TEMPLATE = @"coffeeScriptHelper.compile({0}, {1});";

		/// <summary>
		/// MSIE JS engine
		/// </summary>
		private MsieJsEngine _jsEngine;

		/// <summary>
		/// Synchronizer of compilation
		/// </summary>
		private readonly object _compilationSynchronizer = new object();

		/// <summary>
		/// Flag that compiler is initialized
		/// </summary>
		private bool _initialized;

		/// <summary>
		/// Flag that object is destroyed
		/// </summary>
		private bool _disposed;


		/// <summary>
		/// Destructs instance of CoffeeScript-compiler
		/// </summary>
		~CoffeeScriptCompiler()
		{
			Dispose(false /* disposing */);
		}


		/// <summary>
		/// Initializes compiler
		/// </summary>
		private void Initialize()
		{
			if (!_initialized)
			{
				Type type = GetType();

				_jsEngine = new MsieJsEngine(true, true);
				_jsEngine.ExecuteResource(COFFEESCRIPT_LIBRARY_RESOURCE_NAME, type);
				_jsEngine.ExecuteResource(CSC_HELPER_RESOURCE_NAME, type);

				_initialized = true;
			}
		}

		/// <summary>
		/// "Compiles" CoffeeScript-code to JS-code
		/// </summary>
		/// <param name="content">Text content written on CoffeeScript</param>
		/// <param name="isLiterate">Flag for whether to enable "literate" mode</param>
		/// <returns>Translated CoffeeScript-code</returns>
		public string Compile(string content, bool isLiterate = false)
		{
			string newContent;
			var options = new
			{
				bare = true,
				literate = isLiterate
			};

			lock (_compilationSynchronizer)
			{
				Initialize();

				try
				{
					var result = _jsEngine.Evaluate<string>(
						string.Format(COMPILATION_FUNCTION_CALL_TEMPLATE,
							JsonConvert.SerializeObject(content),
							JsonConvert.SerializeObject(options)));
					var json = JObject.Parse(result);

					var errors = json["errors"] != null ? json["errors"] as JArray : null;
					if (errors != null && errors.Count > 0)
					{
						throw new CoffeeScriptCompilingException(FormatErrorDetails(errors[0], content));
					}

					newContent = json.Value<string>("compiledCode");
				}
				catch (ActiveScriptException e)
				{
					throw new CoffeeScriptCompilingException(
						ActiveScriptErrorFormatter.Format(e));
				}
			}

			return newContent;
		}

		/// <summary>
		/// Generates a detailed error message
		/// </summary>
		/// <param name="errorDetails">Error details</param>
		/// <param name="sourceCode">Source code</param>
		/// <returns>Detailed error message</returns>
		private static string FormatErrorDetails(JToken errorDetails, string sourceCode)
		{
			var message = errorDetails.Value<string>("message");
			var lineNumber = errorDetails.Value<int>("lineNumber");
			var columnNumber = errorDetails.Value<int>("columnNumber");
			string sourceFragment = SourceCodeNavigator.GetSourceFragment(sourceCode, 
				new SourceCodeNodeCoordinates(lineNumber, columnNumber));

			var errorMessage = new StringBuilder();
			errorMessage.AppendFormatLine("{0}: {1}", CoreStrings.ErrorDetails_Message, message);
			if (lineNumber > 0)
			{
				errorMessage.AppendFormatLine("{0}: {1}", CoreStrings.ErrorDetails_LineNumber,
					lineNumber.ToString());
			}
			if (columnNumber > 0)
			{
				errorMessage.AppendFormatLine("{0}: {1}", CoreStrings.ErrorDetails_ColumnNumber,
					columnNumber.ToString());
			}
			if (!string.IsNullOrWhiteSpace(sourceFragment))
			{
				errorMessage.AppendFormatLine("{1}:{0}{0}{2}", Environment.NewLine,
					CoreStrings.ErrorDetails_SourceError, sourceFragment);
			}

			return errorMessage.ToString();
		}
	
		/// <summary>
		/// Destroys object
		/// </summary>
		public void Dispose()
		{
			Dispose(true /* disposing */);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Destroys object
		/// </summary>
		/// <param name="disposing">Flag, allowing destruction of 
		/// managed objects contained in fields of class</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_jsEngine != null)
				{
					_jsEngine.Dispose();
				}
			}
		}
	}
}
