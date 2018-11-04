namespace Mila {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.8.0.0")]
    internal sealed partial class AutoRespond : global::System.Configuration.ApplicationSettingsBase {
        
        private static AutoRespond defaultInstance = ((AutoRespond)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new AutoRespond())));
        
        public static AutoRespond Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool Sewers {
            get {
                return ((bool)(this["Sewers"]));
            }
            set {
                this["Sewers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool FastLOD {
            get {
                return ((bool)(this["FastLOD"]));
            }
            set {
                this["FastLOD"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool Cemetary {
            get {
                return ((bool)(this["Cemetary"]));
            }
            set {
                this["Cemetary"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool OceanTrench {
            get {
                return ((bool)(this["OceanTrench"]));
            }
            set {
                this["OceanTrench"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool Craig {
            get {
                return ((bool)(this["Craig"]));
            }
            set {
                this["Craig"] = value;
            }
        }
    }
}
