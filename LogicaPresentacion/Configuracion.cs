using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;                                         // ConfigurationManager
using System.Security;                                              // SecureString
using Zuliaworks.Netzuela.Valeria.Comunes;                          // ParametrosDeConexion
using Zuliaworks.Netzuela.Valeria.Logica;                           // TablaMapeada, MapeoDeColumnas
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels;    // TablaMapeada, MapeoDeColumnas
using Zuliaworks.Netzuela.Valeria.Preferencias;                     // CargarGuardar

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion
{
    public class Configuracion
    {
        #region Constructores

        public Configuracion() { }

        #endregion

        #region Propiedades

        public ParametrosDeConexion ParametrosConexionLocal { get; set; }
        public ParametrosDeConexion ParametrosConexionRemota { get; set; }
        public SecureString UsuarioLocal { get; set; }
        public SecureString ContrasenaLocal { get; set; }
        public SecureString UsuarioRemoto { get; set; }
        public SecureString ContrasenaRemota { get; set; }
        public List<string[]> Mapas { get; set; }
        public List<TablaDeAsociaciones> Tablas { get; set; }

        #endregion

        #region Funciones

        public static Configuracion CargarConfiguracion()
        {
            Configuracion Resultado = new Configuracion();
            object[] Credenciales;

            Resultado.ParametrosConexionLocal = CargarGuardar.CargarParametrosDeConexion("Local");
            Resultado.ParametrosConexionRemota = CargarGuardar.CargarParametrosDeConexion("Remoto");

            Credenciales = CargarGuardar.CargarCredenciales("Local");
            if (Credenciales != null)
            {
                Resultado.UsuarioLocal = (SecureString)Credenciales[0];
                Resultado.ContrasenaLocal = (SecureString)Credenciales[1];
            }

            Credenciales = CargarGuardar.CargarCredenciales("Remoto");
            if (Credenciales != null)
            {
                Resultado.UsuarioRemoto = (SecureString)Credenciales[0];
                Resultado.ContrasenaRemota = (SecureString)Credenciales[1];
            }

            Resultado.Mapas = CargarGuardar.CargarTablas();

            return Resultado;
        }

        public static void GuardarConfiguracion(Configuracion Preferencias)
        {
            /* 
             * Codigo importado
             * ================
             * 
             * Autor: MSDN Microsoft
             * Titulo: ConfigurationManager (Clase)
             * Licencia: DESCONOCIDA
             * Fuente: http://msdn.microsoft.com/es-es/library/system.configuration.configurationmanager%28v=VS.100%29.aspx
             * 
             * Tipo de uso
             * ===========
             * 
             * Textual                                              []
             * Adaptado                                             [X]
             * Solo se cambiaron los nombres de las variables       []
             * 
             */

            try
            {
                Configuration ArchivoConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Parametros de las conexiones
                ColeccionElementosGenerica<ParametrosDeConexionElement> ColeccionParametros =
                    new ColeccionElementosGenerica<ParametrosDeConexionElement>();

                ParametrosDeConexionElement ParametrosLocales = new ParametrosDeConexionElement(Preferencias.ParametrosConexionLocal);
                ParametrosDeConexionElement ParametrosRemotos = new ParametrosDeConexionElement(Preferencias.ParametrosConexionRemota);
                ParametrosLocales.ID = "Local";
                ParametrosRemotos.ID = "Remoto";

                ColeccionParametros.Add(ParametrosLocales);
                ColeccionParametros.Add(ParametrosRemotos);
                CargarGuardar.GuardarParametrosDeConexion(ArchivoConfig, ColeccionParametros);

                // Credenciales
                ColeccionElementosGenerica<UsuarioContrasenaElement> ColeccionDeLlaves =
                    new ColeccionElementosGenerica<UsuarioContrasenaElement>();

                UsuarioContrasenaElement LlaveLocal = new UsuarioContrasenaElement();
                UsuarioContrasenaElement LlaveRemota = new UsuarioContrasenaElement();

                LlaveLocal.ID = "Local";
                LlaveLocal.Usuario = Preferencias.UsuarioLocal.Encriptar();
                LlaveLocal.Contrasena = Preferencias.ContrasenaLocal.Encriptar();

                
                LlaveRemota.ID = "Remoto";
                LlaveRemota.Usuario = Preferencias.UsuarioRemoto.Encriptar();
                LlaveRemota.Contrasena = Preferencias.ContrasenaRemota.Encriptar();

                ColeccionDeLlaves.Add(LlaveLocal);
                ColeccionDeLlaves.Add(LlaveRemota);
                CargarGuardar.GuardarCredenciales(ArchivoConfig, ColeccionDeLlaves);

                // Mapas de tablas
                MapeoDeColumnasElement Columnas;
                TablaMapeadaElement Tabla;

                ColeccionElementosGenerica<TablaMapeadaElement> ColeccionTablas =
                    new ColeccionElementosGenerica<TablaMapeadaElement>();

                foreach (TablaDeAsociaciones T in Preferencias.Tablas)
                {
                    ColeccionElementosGenerica<MapeoDeColumnasElement> ColeccionColumnas =
                        new ColeccionElementosGenerica<MapeoDeColumnasElement>();

                    foreach (AsociacionDeColumnas MP in T.Sociedades)
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

                CargarGuardar.GuardarTablas(ArchivoConfig, ColeccionTablas);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los parametros de configuración en el archivo de configuración", ex);
            }
        }

        #endregion
    }
}