using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;                                             // ConfigurationManager
using System.Windows;                                                   // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;                              // ParametrosDeConexion
using Zuliaworks.Netzuela.Valeria.Logica;                               // TablaMapeada
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Preferencias;      // Configuracion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public partial class MainViewModel
    {        
        #region Funciones

        private void GuardarPreferencias()
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

                // Parametros de las conexiones
                ConexionesGuardadas = new ConexionesSection();
                ColeccionElementosGenerica<ParametrosDeConexionElement> ColeccionParametros =
                    new ColeccionElementosGenerica<ParametrosDeConexionElement>();

                ParametrosDeConexionElement ParametrosLocales = new ParametrosDeConexionElement(ConexionLocal.Parametros);
                ParametrosDeConexionElement ParametrosRemotos = new ParametrosDeConexionElement(ConexionRemota.Parametros);
                ParametrosLocales.ID = "Local";
                ParametrosRemotos.ID = "Remoto";

                ColeccionParametros.Add(ParametrosLocales);
                ColeccionParametros.Add(ParametrosRemotos);
                ConexionesGuardadas.ParametrosDeConexion = ColeccionParametros;
                
                // Credenciales
                Credenciales = new AutentificacionSection();
                ColeccionElementosGenerica<UsuarioContrasenaElement> ColeccionDeLlaves =
                    new ColeccionElementosGenerica<UsuarioContrasenaElement>();

                UsuarioContrasenaElement LlaveLocal = new UsuarioContrasenaElement();
                UsuarioContrasenaElement LlaveRemota = new UsuarioContrasenaElement();

                LlaveLocal.ID = "Local";
                LlaveLocal.Usuario = ConexionLocal.UsuarioNetzuela.Encriptar();
                LlaveLocal.Contrasena = ConexionLocal.ContrasenaNetzuela.Encriptar();

                // Esto esta aqui por joda... cuando tenga el servidor de Netzuela listo, aqui va 
                // a haber una vaina seria.
                LlaveRemota.ID = "Remoto";
                LlaveRemota.Usuario = "maricoerconio".ConvertirASecureString().Encriptar();
                LlaveRemota.Contrasena = "1234".ConvertirASecureString().Encriptar();

                ColeccionDeLlaves.Add(LlaveLocal);
                ColeccionDeLlaves.Add(LlaveRemota);
                Credenciales.LlavesDeAcceso = ColeccionDeLlaves;

                // Mapas de tablas
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
                    Tabla.ID = T.NodoTabla.Nombre;
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
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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
                    foreach (ParametrosDeConexionElement Param in ConexionesGuardadas.ParametrosDeConexion)
                    {
                        switch(Param.ID)
                        {
                            case "Local":
                                _ConfiguracionLocal.ParametrosConexionLocal = Param.ConvertirAParametrosDeConexion();
                                break;
                            case "Remoto":
                                _ConfiguracionLocal.ParametrosConexionRemota = Param.ConvertirAParametrosDeConexion();
                                break;
                            default:
                                break;
                        }
                    }

                    /*
                    _ConfiguracionLocal.ParametrosConexionLocal = ConexionesGuardadas.ParametrosConexionLocal.ConvertirAParametrosDeConexion();
                    _ConfiguracionLocal.ParametrosConexionRemota = ConexionesGuardadas.ParametrosConexionRemota.ConvertirAParametrosDeConexion();
                    */
                    Resultado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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
                    foreach (UsuarioContrasenaElement UsuCon in Credenciales.LlavesDeAcceso)
                    {
                        switch (UsuCon.ID)
                        {
                            case "Local":
                                _ConfiguracionLocal.UsuarioLocal = UsuCon.Usuario.Desencriptar();
                                _ConfiguracionLocal.ContrasenaLocal = UsuCon.Contrasena.Desencriptar();
                                break;
                            case "Remoto":
                                _ConfiguracionLocal.UsuarioRemoto = UsuCon.Usuario.Desencriptar();
                                _ConfiguracionLocal.ContrasenaRemota = UsuCon.Contrasena.Desencriptar();
                                break;
                            default:
                                break;
                        }
                    }

                    /*
                    _ConfiguracionLocal.UsuarioLocal = Credenciales.UsuarioLocal.Desencriptar();
                    _ConfiguracionLocal.ContrasenaLocal = Credenciales.ContrasenaLocal.Desencriptar();
                    _ConfiguracionLocal.UsuarioRemoto = Credenciales.UsuarioRemoto.Desencriptar();
                    _ConfiguracionLocal.ContrasenaRemota = Credenciales.ContrasenaRemota.Desencriptar();
                    */

                    Resultado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }

            return Resultado;
        }

        #endregion
    }
}
