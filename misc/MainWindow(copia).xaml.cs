using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Security;
using System.Net;
using System.IO;
using System.Windows.Markup;
/*
using org.xquark.bridge;
using org.xquark.extractor;
using org.xquark.extractor.runtime;
using org.xquark.extractor.common;
using org.xquark.jdbc.datasource;
using org.xquark.xml.xdbc;
*/
namespace _3231_Proyecto
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        private static string JDBC_URI = "jdbc:mysql://localhost/spuria";
        private static string JDBC_LOGIN = "netzuela";
        private static string JDBC_PASSWORD = "1234";
        */
        public MainWindow()
        {
            /*
             * OBLIGATORIO PARA QUE EL CODIGO EN JAVA DE XQUARE BRIDGE FUNCIONE EN .NET 
             * ========================================================================
             * 
             * Si compilaste los .jar a .dll empleando: ikvmc -classloader:ikvm.runtime.AppDomainAssemblyClassLoader,
             * entonces debeis cargar los dll que no son referencia directa de nadie (pero son necesarios) empleando 
             * alguna de las instrucciones siguientes:
             */           
            //ikvm.runtime.Startup.addBootClassPathAssemby(Assembly.Load("mysql-connector-java-5.1.0-bin"));
            //AppDomain.CurrentDomain.Load("mysql-connector-java-5.1.0-bin");
            /*
             * Si compilaste los .jar empleando: ikvmc -classloader:ikvm.runtime.ClassPathAssemblyClassLoader, debeis 
             * agregar los .dll al CLASSPATH para que sus clases puedan ser accesibles al momento de buscarlas. Actualmente
             * no he podido hacer que xquare bridge funcione correctamente de este modo.
             */
            //java.lang.System.setProperty("java.class.path", "D:\\Netzuela\\3230_Valeria\\3233_Bibliotecas\\xquark-bridge-1.2.0_(.NET-ikvmc)\\");

            InitializeComponent();
            /*
            try
            {
                XQBridge bridge = new XQBridge(JDBC_URI, JDBC_LOGIN, JDBC_PASSWORD);
                XMLConnection xc = bridge.getXMLConnection();
                XMLStatement xs = xc.createStatement();
               
                XMLResultSet xrs = xs.executeQuery (
                    "<result> {" +
                    "   for $i in collection(\"usuario\")/usuario" +
                    "   return" +
                    "       <item>" +
                    "           { $i/UsuarioID }" +
                    "           { $i/Parroquia }" +
                    "       </item>" +
                    "} </result> "
                );
                
                while (xrs.hasNext()) 
                {
                    MessageBox.Show(xrs.nextAsString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.TargetSite + "\n" + ex.Data);
                return;
            }*/
        }        
    }
}
