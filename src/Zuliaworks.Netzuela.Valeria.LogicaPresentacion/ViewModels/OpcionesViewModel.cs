namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;                         // ConfigurationManager
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;

    using MvvmFoundation.Wpf;                           // PropertyObserver<>, ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;          // Constantes
    using Zuliaworks.Netzuela.Valeria.Logica;           // TablaDeAsociaciones
    using Zuliaworks.Netzuela.Valeria.Preferencias;     // CargarGuardar
    
    /// <summary>
    /// 
    /// </summary>
    public class OpcionesViewModel : ObservableObject, IDisposable
    {
        #region Constructores

        public OpcionesViewModel()
        {
            Mensajeria.Mensajero.Register<object[]>(Mensajeria.GuardarConfiguracion, new Action<object[]>(this.MensajeroGuardarConfiguracion));
            Mensajeria.Mensajero.Register(Mensajeria.CargarConfiguracion, new Action(this.MensajeroCargarConfiguracion));
        }

        ~OpcionesViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public int IntervaloSincronizacion { get; set; }

        #endregion

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            if (borrarCodigoAdministrado)
            {
            }
        }

        private void MensajeroCargarConfiguracion()
        {
            Configuracion conf = this.CargarConfiguracion();
            Mensajeria.Mensajero.NotifyColleagues(Mensajeria.ConfiguracionCargada, conf);
        }

        private void MensajeroGuardarConfiguracion(object[] parametros)
        {
            Configuracion conf = new Configuracion();
            conf.ParametrosConexionLocal = (ParametrosDeConexion)parametros[0];
            conf.ParametrosConexionRemota = (ParametrosDeConexion)parametros[1];
            conf.UsuarioLocal = (SecureString)parametros[2];
            conf.ContrasenaLocal = (SecureString)parametros[3];
            conf.UsuarioRemoto = (SecureString)parametros[4];
            conf.ContrasenaRemota = (SecureString)parametros[5];
            conf.Tablas = (List<TablaDeAsociaciones>)parametros[6];

            this.GuardarConfiguracion(conf);
        }

        public Configuracion CargarConfiguracion()
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

            Resultado.Asociaciones = CargarGuardar.CargarTablas();

            return Resultado;
        }

        public void GuardarConfiguracion(Configuracion Preferencias)
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
                AsociacionDeColumnasElement Columnas;
                TablaDeAsociacionesElement Tabla;

                ColeccionElementosGenerica<TablaDeAsociacionesElement> ColeccionTablas =
                    new ColeccionElementosGenerica<TablaDeAsociacionesElement>();

                foreach (TablaDeAsociaciones T in Preferencias.Tablas)
                {
                    ColeccionElementosGenerica<AsociacionDeColumnasElement> ColeccionColumnas =
                        new ColeccionElementosGenerica<AsociacionDeColumnasElement>();

                    foreach (AsociacionDeColumnas MP in T.Sociedades)
                    {
                        Columnas = new AsociacionDeColumnasElement();
                        Columnas.NodoDestino = MP.ColumnaDestino.BuscarEnRepositorioDeNodos().RutaCompleta();
                        if (MP.ColumnaOrigen != null)
                            Columnas.NodoOrigen = MP.ColumnaOrigen.BuscarEnRepositorioDeNodos().RutaCompleta();

                        ColeccionColumnas.Add(Columnas);
                    }

                    Tabla = new TablaDeAsociacionesElement();
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

        #region Implementacion de interfaces

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
