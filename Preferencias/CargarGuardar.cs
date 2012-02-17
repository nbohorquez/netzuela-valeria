namespace Zuliaworks.Netzuela.Valeria.Preferencias
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;                         // ConfigurationManager
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;          // ParametrosDeConexion

    public static class CargarGuardar
    {
        #region Funciones

        public static void GuardarParametrosDeConexion(Configuration archivoConfig, ColeccionElementosGenerica<ParametrosDeConexionElement> coleccionParametros)
        {
            try
            {
                ConexionesSection conexionesGuardadas = new ConexionesSection();
                conexionesGuardadas.ParametrosDeConexion = coleccionParametros;

                archivoConfig.Sections.Remove("conexionesGuardadas");
                archivoConfig.Sections.Add("conexionesGuardadas", conexionesGuardadas);
                archivoConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("conexionesGuardadas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los ParametrosDeConexion en el archivo de configuración", ex);
            }
        }
        
        public static ParametrosDeConexion CargarParametrosDeConexion(string id)
        {
            ParametrosDeConexion resultado = null;

            try
            {
                ConexionesSection conexionesGuardadas = (ConexionesSection)ConfigurationManager.GetSection("conexionesGuardadas");

                if (conexionesGuardadas != null)
                {
                    foreach (ParametrosDeConexionElement param in conexionesGuardadas.ParametrosDeConexion)
                    {
                        if (param.ID == id)
                        {
                            resultado = param.ConvertirAParametrosDeConexion();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los ParametrosDeConexion desde el archivo de configuración", ex);
            }

            return resultado;
        }

        public static void GuardarCredenciales(Configuration archivoConfig, ColeccionElementosGenerica<UsuarioContrasenaElement> coleccionDeLlaves)
        {
            try
            {
                AutentificacionSection credenciales = new AutentificacionSection();
                credenciales.LlavesDeAcceso = coleccionDeLlaves;

                archivoConfig.Sections.Remove("credenciales");
                archivoConfig.Sections.Add("credenciales", credenciales);
                archivoConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("credenciales");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar las credenciales en el archivo de configuración", ex);
            }
        }

        public static SecureString[] CargarCredenciales(string id)
        {
            List<SecureString> resultado = null;

            try
            {
                AutentificacionSection credenciales = (AutentificacionSection)ConfigurationManager.GetSection("credenciales");

                if (credenciales != null)
                {
                    foreach (UsuarioContrasenaElement usuCon in credenciales.LlavesDeAcceso)
                    {
                        if (usuCon.ID == id)
                        {
                            resultado = new List<SecureString>();

                            resultado.Add(usuCon.Usuario.Desencriptar());
                            resultado.Add(usuCon.Contrasena.Desencriptar());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar las credenciales desde el archivo de configuración", ex);
            }

            return (resultado != null) ? resultado.ToArray() : null;
        }

        public static void GuardarTablas(Configuration archivoConfig, ColeccionElementosGenerica<TablaDeAsociacionesElement> coleccionTablas)
        {
            try
            {
                TablasDeAsociacionesSection tablas = new TablasDeAsociacionesSection();
                tablas.Tablas = coleccionTablas;

                archivoConfig.Sections.Remove("mapas");
                archivoConfig.Sections.Add("mapas", tablas);
                archivoConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("mapas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los MapaColumnas en el archivo de configuración", ex);
            }
        }

        public static List<string[]> CargarTablas()
        {
            List<string[]> resultado = null;

            try
            {
                TablasDeAsociacionesSection tablas = (TablasDeAsociacionesSection)ConfigurationManager.GetSection("mapas");

                if (tablas != null)
                {
                    resultado = new List<string[]>();

                    foreach (TablaDeAsociacionesElement tabla in tablas.Tablas)
                    {
                        foreach (AsociacionDeColumnasElement columnas in tabla.TablaMapeada)
                        {
                            string[] nodos = new string[] 
                            {
                                columnas.NodoOrigen,
                                columnas.NodoDestino                        
                            };

                            resultado.Add(nodos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los MapaColumnas desde el archivo de configuración", ex);
            }

            return resultado;
        }

        #endregion
    }
}
