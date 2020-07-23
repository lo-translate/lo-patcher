﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LoPatcher.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LoPatcher.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] classdata {
            get {
                object obj = ResourceManager.GetObject("classdata", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to patch all files.
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalPatchFullFail {
            get {
                return ResourceManager.GetString("ErrorModalPatchFullFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to patch all files.
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalPatchPartialFail {
            get {
                return ResourceManager.GetString("ErrorModalPatchPartialFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No valid files selected.
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalSelectionFullFail {
            get {
                return ResourceManager.GetString("ErrorModalSelectionFullFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some invalid files selected.
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalSelectionPartialFail {
            get {
                return ResourceManager.GetString("ErrorModalSelectionPartialFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        internal static string ErrorModalTitle {
            get {
                return ResourceManager.GetString("ErrorModalTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to parse translations, patcher will not work until broken translations are fixed.
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalTranslationParse {
            get {
                return ResourceManager.GetString("ErrorModalTranslationParse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Translation update check failed
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalUpdateCheckFailed {
            get {
                return ResourceManager.GetString("ErrorModalUpdateCheckFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Translation update failed
        ///
        ///{reason}.
        /// </summary>
        internal static string ErrorModalUpdateFailed {
            get {
                return ResourceManager.GetString("ErrorModalUpdateFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Checking....
        /// </summary>
        internal static string LabelTextCheckingUpdate {
            get {
                return ResourceManager.GetString("LabelTextCheckingUpdate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LoTranslation.zip.
        /// </summary>
        internal static string LanguageLocalFile {
            get {
                return ResourceManager.GetString("LanguageLocalFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://api.github.com/repos/lo-translate/lo-translation/releases/latest.
        /// </summary>
        internal static string LanguageUpdateUrl {
            get {
                return ResourceManager.GetString("LanguageUpdateUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] LoTranslation {
            get {
                object obj = ResourceManager.GetObject("LoTranslation", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Patch complete.
        /// </summary>
        internal static string SuccessModalPatchComplete {
            get {
                return ResourceManager.GetString("SuccessModalPatchComplete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Success.
        /// </summary>
        internal static string SuccessModalTitle {
            get {
                return ResourceManager.GetString("SuccessModalTitle", resourceCulture);
            }
        }
    }
}
