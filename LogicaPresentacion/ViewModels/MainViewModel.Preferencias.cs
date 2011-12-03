using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;                                             // ConfigurationManager
using System.Windows;                                                   // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;                              // ParametrosDeConexion
using Zuliaworks.Netzuela.Valeria.Logica;                               // TablaMapeada
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Preferencias;   // Configuracion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public partial class MainViewModel
    {        
        #region Funciones

        private void GuardarConfiguracion()
        {
            // Con codigo de 
            // http://msdn.microsoft.com/es-es/library/system.configuration.configurationmanager%28v=VS.100%29.aspx

            Configuration AppConfig;
            ConexionesSection ConexionesGuardadas;
            AutentificacionSection Credenciales;

            MapeoDeColumnasElement Columnas;
            TablaMapeadaElement Tabla;
            TablasMapeadasSection Tablas;

            ColeccionElementosGenerica<TablaMapeadaElement> ColeccionTablas =
                    new ColeccionElementosGenerica<TablaMapeadaElement>();

            try
            {
                AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                ConexionesGuardadas = new ConexionesSection();
                ConexionesGuardadas.ParametrosConexionLocal = new ParametrosDeConexionElement(ConexionLocal.Parametros);
                ConexionesGuardadas.ParametrosConexionRemota = new ParametrosDeConexionElement(ConexionRemota.Parametros);

                Credenciales = new AutentificacionSection();
                Credenciales.UsuarioLocal = ConexionLocal.UsuarioNetzuela.Encriptar();
                Credenciales.ContrasenaLocal = ConexionLocal.ContrasenaNetzuela.Encriptar();

                // Esto esta aqui por joda... cuando tenga el servidor de Netzuela listo, aqui va 
                // a haber una vaina seria.
                Credenciales.UsuarioRemoto = "maricoerconio".ConvertirASecureString().Encriptar();
                Credenciales.ContrasenaRemota = "1234".ConvertirASecureString().Encriptar();

                Columnas = new MapeoDeColumnasElement();
                Tablas = new TablasMapeadasSection();

                foreach (TablaMapeada T in LocalARemota.Tablas)
                {
                    ColeccionElementosGenerica<MapeoDeColumnasElement> ColeccionColumnas =
                        new ColeccionElementosGenerica<MapeoDeColumnasElement>();

                    foreach (MapeoDeColumnas MP in T.MapasColumnas)
                    {
                        Columnas = new MapeoDeColumnasElement();
                        Columnas.NodoDestino = MP.ColumnaDestino.BuscarEnRepositorio().RutaCompleta();
                        if (MP.ColumnaOrigen != null)
                            Columnas.NodoOrigen = MP.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta();

                        ColeccionColumnas.Add(Columnas);
                    }

                    Tabla = new TablaMapeadaElement();
                    Tabla.TablaMapeada = ColeccionColumnas;

                    ColeccionTablas.Add(Tabla);
                }

                Tablas.Tablas = ColeccionTablas;

                AppConfig.Sections.Add("conexionesGuardadas", ConexionesGuardadas);
                AppConfig.Sections.Add("credenciales", Credenciales);
                AppConfig.Sections.Add("mapas", Tablas);

                AppConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("conexionesGuardadas");
                ConfigurationManager.RefreshSection("credenciales");
                ConfigurationManager.RefreshSection("mapas");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }
        }

        private bool CargarParametrosDeConexion()
        {
            bool Resultado = false;

            try
            {
                ConexionesSection ConexionesGuardadas = ConfigurationManager.GetSection("conexionesGuardadas")
                    as ConexionesSection;

                if (ConexionesGuardadas != null)
                {
                    _ConfiguracionLocal.ParametrosConexionLocal = ConexionesGuardadas.ParametrosConexionLocal.ConvertirAParametrosDeConexion();
                    _ConfiguracionLocal.ParametrosConexionRemota = ConexionesGuardadas.ParametrosConexionRemota.ConvertirAParametrosDeConexion();

                    Resultado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }

            return Resultado;
        }

        private bool CargarCredenciales()
        {
            bool Resultado = false;

            try
            {
                AutentificacionSection Credenciales = ConfigurationManager.GetSection("credenciales")
                    as AutentificacionSection;

                if (Credenciales != null)
                {
                    _ConfiguracionLocal.UsuarioLocal = Credenciales.UsuarioLocal.Desencriptar();
                    _ConfiguracionLocal.ContrasenaLocal = Credenciales.ContrasenaLocal.Desencriptar();
                    _ConfiguracionLocal.UsuarioRemoto = Credenciales.UsuarioRemoto.Desencriptar();
                    _ConfiguracionLocal.ContrasenaRemota = Credenciales.ContrasenaRemota.Desencriptar();

                    Resultado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }

            return Resultado;
        }

        private bool CargarTablas()
        {
            bool Resultado = false;

            try
            {
                TablasMapeadasSection Tablas = ConfigurationManager.GetSection("mapas") as TablasMapeadasSection;

                if (Tablas != null)
                {
                    foreach (TablaMapeadaElement Tabla in Tablas.Tablas)
                    {
                        foreach (MapeoDeColumnasElement Columnas in Tabla.TablaMapeada)
                        {
                            string[] Nodos = new string[] 
                            {
                                Columnas.NodoOrigen,
                                Columnas.NodoDestino                        
                            };

                            _ConfiguracionLocal.Mapas.Add(Nodos);
                        }
                    }

                    Resultado = true;
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }

            return Resultado;
        }

        #endregion
    }
}
