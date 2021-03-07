using System;
using System.IO;
using System.Reflection;
using System.Windows;
using TR2RandomizerCore;
using TR2RandomizerCore.Helpers;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Version { get; private set; }
        public string TaggedVersion { get; private set; }
        public string Copyright { get; private set; }

        public App()
        {
            WindowUtils.SetMenuAlignment();

            Assembly assembly = Assembly.GetExecutingAssembly();

            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                Title = ((AssemblyTitleAttribute)attributes[0]).Title;
            }
            else
            {
                Title = Path.GetFileNameWithoutExtension(assembly.CodeBase);
            }

            attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                Description = ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }

            attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length > 0)
            {
                Copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }

            Version v = assembly.GetName().Version;
            Version = string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);

            attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length > 0)
            {
                TaggedVersion = ((AssemblyProductAttribute)attributes[0]).Product.Trim();
                if (TaggedVersion.Contains(" "))
                {
                    string[] tagArr = TaggedVersion.Split(' ');
                    TaggedVersion = tagArr[tagArr.Length - 1];
                }
            }
            else
            {
                TaggedVersion = "v" + Version;
            }

            TR2RandomizerCoord.Instance.Initialise("TR2Rando", Version, TaggedVersion, new ModificationStamp
            {
                English = "Modified by TR2Rando",
                French = "Modifi)e par TR2Rando",
                German = "Ge~andert von TR2Rando"
            });
        }
    }
}