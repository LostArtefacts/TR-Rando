using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using TRRandomizerCore;
using TRRandomizerCore.Helpers;
using TRRandomizerView.Utilities;

namespace TRRandomizerView;

public partial class App : Application
{
    static App()
    {
        FrameworkElement.LanguageProperty.OverrideMetadata
        (
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag))
        );
    }

    public string Title { get; private set; }
    public string Version { get; private set; }
    public string TaggedVersion { get; private set; }
    public string Copyright { get; private set; }

    public App()
    {
        WindowUtils.SetMenuAlignment();

        Assembly assembly = Assembly.GetExecutingAssembly();

        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        if (attributes.Length > 0)
        {
            Title = ((AssemblyProductAttribute)attributes[0]).Product;
        }
        else
        {
            Title = "Tomb Raider Randomizer";
        }

        attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        if (attributes.Length > 0)
        {
            Copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        Version v = assembly.GetName().Version;
        Version = string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
        TaggedVersion = "V" + Version;

        TRRandomizerCoord.Instance.Initialise("TR2Rando", Version, TaggedVersion, new ModificationStamp
        {
            English = "TRRando",
            French = "TRRando",
            German = "TRRando"
        });
    }
}
