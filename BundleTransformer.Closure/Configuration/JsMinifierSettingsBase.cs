﻿namespace BundleTransformer.Closure.Configuration
{
	using System.Configuration;

	/// <summary>
	/// Configuration settings of JS-minifier
	/// </summary>
	public abstract class JsMinifierSettingsBase : ConfigurationElement
	{
		/// <summary>
		/// Gets or sets a degree of compression and optimization to apply to your JavaScript.
		/// There are 3 possible compilation levels:
		/// WhitespaceOnly - just removes whitespace and comments from your JavaScript;
		/// Simple - performs compression and optimization that does not interfere with 
		/// the interaction between the compiled JavaScript and other JavaScript. 
		/// This level renames only local variables;
		/// Advanced - achieves the highest level of compression by renaming symbols in your 
		/// JavaScript. When using this type of compilation you must perform extra steps 
		/// to preserve references to external symbols.
		/// </summary>
		[ConfigurationProperty("compilationLevel", DefaultValue = CompilationLevel.Simple)]
		public CompilationLevel CompilationLevel
		{
			get { return (CompilationLevel)this["compilationLevel"]; }
			set { this["compilationLevel"] = value; }
		}

		/// <summary>
		/// Gets or sets a flag for whether to add line breaks and indentation to its 
		/// output code to make the code easier for humans to read
		/// </summary>
		[ConfigurationProperty("prettyPrint", DefaultValue = false)]
		public bool PrettyPrint
		{
			get { return (bool)this["prettyPrint"]; }
			set { this["prettyPrint"] = value; }
		}
		
		/// <summary>
		/// Gets or sets a severity level of errors:
		///		0 - only syntax error messages;
		///		1 - only syntax error messages and warnings generated by the optimization;
		///		2 - in addition to syntax errors and warnings generated by optimization 
		///		passes, outputs warnings generated by selected code-checking passes;
		///		3 - in addition to syntax errors and warnings generated by optimization 
		///		passes, outputs warnings generated by all code-checking passes.
		/// </summary>
		[ConfigurationProperty("severity", DefaultValue = 0)]
		[IntegerValidator(MinValue = 0, MaxValue = 3, ExcludeRange = false)]
		public int Severity
		{
			get { return (int)this["severity"]; }
			set { this["severity"] = value; }
		}
	}
}