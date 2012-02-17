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

using org.xquark.xml.xdbc;
using org.xquark.bridge;

namespace Valeria
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       // private string Usuario;
        private string RutaDelArchivo;
        private string Dominio = "http://127.0.0.1/Valeria.php";
        private static String JDBC_URI = "jdbc:mysql://localhost:3306:spuria";
        private static String JDBC_LOGIN = "netzuela";
        private static String JDBC_PASSWORD = "1234";
    //    private SecureString Contrasena;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                XQBridge bridge = new XQBridge(JDBC_URI, JDBC_LOGIN, JDBC_PASSWORD);
                XMLConnection xc = bridge.getXMLConnection();
                XMLStatement xs = xc.createStatement();
                xs.setBaseURI("D://Netzuela/3230_Valeria/");

                bool result = xs.execute (
                    " import module namespace isn = " +
                    "\"http://www.xquark.org/Bridge/API/Tutorial\" " +
                    "at \"view.mod\";" +
                    "<result> {" +
                    "             for $i in isn:itemSummary()" +
                    "             where contains($i/DESCRIPTION, \"Bicycle\")" + 
                    "             return $i" +
                    "} </result> "
                );
                
                if (result) 
                {
                    XMLResultSet xrs = xs.getResultSet();
                    while (xrs.hasNext()) 
                    {
                        MessageBox.Show(xrs.nextAsString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void botn_Enviar_Click(object sender, RoutedEventArgs e)
        {
            /* Pedimos al servidor HTTP acceder al sistema con los credenciales aportados por el usuario*/
            HttpWebRequest Peticion = (HttpWebRequest)WebRequest.Create(Dominio);
            
            /* Obtenemos la respuesta del servidor */
            HttpWebResponse Respuesta = (HttpWebResponse)Peticion.GetResponse();

            /* Abrimos el canal de flujo asociado con la respuesta */
            Stream FlujoDeEntrada = Respuesta.GetResponseStream();
            StreamReader LectorDeFlujo = new StreamReader(FlujoDeEntrada, Encoding.UTF8);
            
            /* Simplemente mostramos que ocurre */
         //   text_Actividad.Items.Add(LectorDeFlujo.ReadToEnd());
            
            Respuesta.Close();
            LectorDeFlujo.Close();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            RutaDelArchivo = text_RutaDelArchivo.Text;
        }

        private void botn_AbrirArchivo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog Dialogo = new Microsoft.Win32.OpenFileDialog();
            Dialogo.InitialDirectory = "C:\\";
            Dialogo.CheckFileExists = true;
            Dialogo.Filter = "Archivos XML (*.xml)|*.txt|Todos los archivos(*.*)|*.*";
            Dialogo.Multiselect = false;
 
            if (Dialogo.ShowDialog() == true)
            {
                try
                {
                    Stream Flujo;

                    if ((Flujo = Dialogo.OpenFile()) != null)
                    {
                        StreamReader LectorDeFlujo = new StreamReader(Flujo);
                        text_Visor.Text = LectorDeFlujo.ReadToEnd();
                        LectorDeFlujo.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problema con el archivo. Mirá: " + ex.Message);
                    return;
                }

                text_RutaDelArchivo.Text = Dialogo.FileName;
            }
        }

        private void botn_Borrar_Click(object sender, RoutedEventArgs e)
        {
            text_RutaDelArchivo.Text = string.Empty;
            text_Visor.Text = string.Empty;
        }

        private void VisorDeDocumento_PageViewsChanged(object sender, EventArgs e)
        {
        }
    }    
}