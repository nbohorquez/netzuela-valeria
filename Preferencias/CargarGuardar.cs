using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;                         // ConfigurationManager
using System.Security;                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    public static class CargarGuardar
    {
        #region Variables

        private static Configuration _AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        #endregion

        #region Funciones

        public static void GuardarParametrosDeConexion(ColeccionElementosGenerica<ParametrosDeConexionElement> ColeccionParametros)
        {
            try
            {
                ConexionesSection ConexionesGuardadas = new ConexionesSection();
                ConexionesGuardadas.ParametrosDeConexion = ColeccionParametros;

                _AppConfig.Sections.Remove("conexionesGuardadas");
                _AppConfig.Sections.Add("conexionesGuardadas", ConexionesGuardadas);
                _AppConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("conexionesGuardadas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los ParametrosDeConexion en el archivo de configuración", ex);
            }
        }
        
        public static ParametrosDeConexion CargarParametrosDeConexion(string ID)
        {
            ParametrosDeConexion Resultado = null;

            try
            {
                ConexionesSection ConexionesGuardadas = ConfigurationManager.GetSection("conexionesGuardadas")
                    as ConexionesSection;

                if (ConexionesGuardadas != null)
                {
                    foreach (ParametrosDeConexionElement Param in ConexionesGuardadas.ParametrosDeConexion)
                    {
                        if (Param.ID == ID)
                        {
                            Resultado = Param.ConvertirAParametrosDeConexion();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los ParametrosDeConexion desde el archivo de configuración", ex);
            }

            return Resultado;
        }

        public static void GuardarCredenciales(ColeccionElementosGenerica<UsuarioContrasenaElement> ColeccionDeLlaves)
        {
            AutentificacionSection Credenciales = new AutentificacionSection();
            Credenciales.LlavesDeAcceso = ColeccionDeLlaves;

            _AppConfig.Sections.Remove("credenciales");
            _AppConfig.Sections.Add("credenciales", Credenciales);
            _AppConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("credenciales");
        }

        public static SecureString[] CargarCredenciales(string ID)
        {
            List<SecureString> Resultado = null;

            try
            {
                AutentificacionSection Credenciales = ConfigurationManager.GetSection("credenciales")
                    as AutentificacionSection;

                if (Credenciales != null)
                {
                    foreach (UsuarioContrasenaElement UsuCon in Credenciales.LlavesDeAcceso)
                    {
                        if (UsuCon.ID == ID)
                        {
                            Resultado = new List<SecureString>();

                            Resultado.Add(UsuCon.Usuario.Desencriptar());
                            Resultado.Add(UsuCon.Contrasena.Desencriptar());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar las credenciales desde el archivo de configuración", ex);
            }

            return (Resultado != null) ? Resultado.ToArray() : null;
        }

        public static void GuardarTablas(ColeccionElementosGenerica<TablaMapeadaElement> ColeccionTablas)
        {
            TablasMapeadasSection Tablas = new TablasMapeadasSection();
            Tablas.Tablas = ColeccionTablas;

            _AppConfig.Sections.Remove("mapas");
            _AppConfig.Sections.Add("mapas", Tablas);
            _AppConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("mapas");
        }

        public static List<string[]> CargarTablas()
        {
            List<string[]> Resultado = null;

            try
            {
                TablasMapeadasSection Tablas = ConfigurationManager.GetSection("mapas") as TablasMapeadasSection;

                if (Tablas != null)
                {
                    Resultado = new List<string[]>();

                    foreach (TablaMapeadaElement Tabla in Tablas.Tablas)
                    {
                        foreach (MapeoDeColumnasElement Columnas in Tabla.TablaMapeada)
                        {
                            string[] Nodos = new string[] 
                            {
                                Columnas.NodoOrigen,
                                Columnas.NodoDestino                        
                            };

                            Resultado.Add(Nodos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los MapaColumnas desde el archivo de configuración", ex);
            }

            return Resultado;
        }

        #endregion
    }
}
