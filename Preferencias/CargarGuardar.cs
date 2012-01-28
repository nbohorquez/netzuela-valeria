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
        #region Funciones

        public static void GuardarParametrosDeConexion(Configuration ArchivoConfig, ColeccionElementosGenerica<ParametrosDeConexionElement> ColeccionParametros)
        {
            try
            {
                ConexionesSection ConexionesGuardadas = new ConexionesSection();
                ConexionesGuardadas.ParametrosDeConexion = ColeccionParametros;

                ArchivoConfig.Sections.Remove("conexionesGuardadas");
                ArchivoConfig.Sections.Add("conexionesGuardadas", ConexionesGuardadas);
                ArchivoConfig.Save(ConfigurationSaveMode.Modified);

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

        public static void GuardarCredenciales(Configuration ArchivoConfig, ColeccionElementosGenerica<UsuarioContrasenaElement> ColeccionDeLlaves)
        {
            try
            {
                AutentificacionSection Credenciales = new AutentificacionSection();
                Credenciales.LlavesDeAcceso = ColeccionDeLlaves;

                ArchivoConfig.Sections.Remove("credenciales");
                ArchivoConfig.Sections.Add("credenciales", Credenciales);
                ArchivoConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("credenciales");
            }
            catch(Exception ex)
            {
                throw new Exception("Error al guardar las credenciales en el archivo de configuración", ex);
            }
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

        public static void GuardarTablas(Configuration ArchivoConfig, ColeccionElementosGenerica<TablaDeAsociacionesElement> ColeccionTablas)
        {
            try
            {
                TablasDeAsociacionesSection Tablas = new TablasDeAsociacionesSection();
                Tablas.Tablas = ColeccionTablas;

                ArchivoConfig.Sections.Remove("mapas");
                ArchivoConfig.Sections.Add("mapas", Tablas);
                ArchivoConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("mapas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los MapaColumnas en el archivo de configuración", ex);
            }
        }

        public static List<string[]> CargarTablas()
        {
            List<string[]> Resultado = null;

            try
            {
                TablasDeAsociacionesSection Tablas = ConfigurationManager.GetSection("mapas") as TablasDeAsociacionesSection;

                if (Tablas != null)
                {
                    Resultado = new List<string[]>();

                    foreach (TablaDeAsociacionesElement Tabla in Tablas.Tablas)
                    {
                        foreach (AsociacionDeColumnasElement Columnas in Tabla.TablaMapeada)
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
