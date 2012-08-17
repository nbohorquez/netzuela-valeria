namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;                         // ConfigurationManager
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;
    using System.Windows;

    using MvvmFoundation.Wpf;                           // PropertyObserver<>, ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;          // Constantes
    using Zuliaworks.Netzuela.Valeria.Cliente.Logica;   // TablaDeAsociaciones
    using Zuliaworks.Netzuela.Valeria.Preferencias;     // CargarGuardar
    
    /// <summary>
    /// 
    /// </summary>
    public class OpcionesViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private TimeSpan intervaloCompensacion;

        #endregion

        #region Constructores

        public OpcionesViewModel()
        {
            this.IntervaloCompensacion = new TimeSpan(0, 5, 0);
        }

        ~OpcionesViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public TimeSpan IntervaloCompensacion 
        {
            get { return this.intervaloCompensacion; }
            set
            {
                if (value != this.intervaloCompensacion)
                {
                    this.intervaloCompensacion = value;
                    this.RaisePropertyChanged("IntervaloCompensacion");
                }
            } 
        }

        public string IntervaloCompensacionString
        {
            get { return this.IntervaloCompensacion.ToString(); }
        }

        public int BarraDeDesplazamiento
        {
            get { return (int)this.IntervaloCompensacion.TotalSeconds; }
            set 
            {
                if (value != this.BarraDeDesplazamiento)
                {
                    this.IntervaloCompensacion = new TimeSpan(0, 0, value);
                    this.RaisePropertyChanged("BarraDeDesplazamiento");
                    this.RaisePropertyChanged("IntervaloCompensacionString");
                }
            }
        }

        #endregion

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            if (borrarCodigoAdministrado)
            {
            }
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
                Resultado.TiendaId = Int32.Parse(((SecureString)Credenciales[2]).ConvertirAUnsecureString());
                Resultado.NombreTienda = ((SecureString)Credenciales[3]).ConvertirAUnsecureString();
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
                ParametrosLocales.Id = "Local";
                ParametrosRemotos.Id = "Remoto";

                ColeccionParametros.Add(ParametrosLocales);
                ColeccionParametros.Add(ParametrosRemotos);
                CargarGuardar.GuardarParametrosDeConexion(ArchivoConfig, ColeccionParametros);

                // Credenciales
                ColeccionElementosGenerica<UsuarioContrasenaElement> ColeccionDeLlaves =
                    new ColeccionElementosGenerica<UsuarioContrasenaElement>();

                UsuarioContrasenaElement LlaveLocal = new UsuarioContrasenaElement();
                UsuarioContrasenaElement LlaveRemota = new UsuarioContrasenaElement();

                LlaveLocal.Id = "Local";
                LlaveLocal.Llave = string.Format(
                    "{0}:{1}", 
                    Preferencias.UsuarioLocal.ConvertirAUnsecureString(), 
                    Preferencias.ContrasenaLocal.ConvertirAUnsecureString()
                ).ConvertirASecureString().Encriptar();
                
                LlaveRemota.Id = "Remoto";
                LlaveRemota.Llave = string.Format(
                    "{0}:{1}:{2}:{3}", 
                    Preferencias.UsuarioRemoto.ConvertirAUnsecureString(), 
                    Preferencias.ContrasenaRemota.ConvertirAUnsecureString(),
                    Preferencias.TiendaId,
                    Preferencias.NombreTienda
                ).ConvertirASecureString().Encriptar();

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
                    Tabla.Id = T.NodoTabla.Nombre;
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
