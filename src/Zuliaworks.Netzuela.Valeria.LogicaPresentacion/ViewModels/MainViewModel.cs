﻿namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Data;                              // DataTable
    using System.Linq;
    using System.Security;                          // SecureString
    using System.Text;
    using System.Windows;                           // MessageBox
    using System.Windows.Threading;                 // DispatcherTimer
    
    using MvvmFoundation.Wpf;                       // PropertyObserver<>, ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes
    using Zuliaworks.Netzuela.Valeria.Logica;       // TablaMapeada

    /// <summary>
    /// 
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private readonly PropertyObserver<ConexionLocalViewModel> observadorConexionLocalEstablecida;
        private readonly PropertyObserver<ConexionRemotaViewModel> observadorConexionRemotaEstablecida;
        private PropertyObserver<SincronizacionViewModel> observadorSincronizacion;
        private Configuracion configuracion;
        private ExploradorViewModel exploradorLocal;
        private ExploradorViewModel exploradorRemoto;
        private SincronizacionViewModel localARemota;
        private DispatcherTimer temporizador;
        private bool sistemaConfigurado;

        #endregion
        
        #region Constructores

        public MainViewModel()
        {
            InicializarTemporizador();

            ConexionLocal = new ConexionLocalViewModel();
            ConexionRemota = new ConexionRemotaViewModel();
            Opciones = new OpcionesViewModel();
                       
            observadorConexionLocalEstablecida = new PropertyObserver<ConexionLocalViewModel>(this.ConexionLocal)
                .RegisterHandler(n => n.Estado, this.ConexionLocalActiva);

            observadorConexionRemotaEstablecida = new PropertyObserver<ConexionRemotaViewModel>(this.ConexionRemota)
                .RegisterHandler(n => n.Estado, this.ConexionRemotaActiva);

            AmbasConexionesEstablecidas += new EventHandler<EventArgs>(ManejarAmbasConexionesEstablecidas);

            /*
            Mensajeria.Mensajero.Register<object>(Mensajeria.ConfiguracionCargada, new Action<object>(this.MensajeroConfiguracionCargada));
            Mensajeria.Mensajero.Register(Mensajeria.ConfiguracionGuardada, new Action(this.MensajeroConfiguracionGuardada));
            Mensajeria.Mensajero.NotifyColleagues(Mensajeria.CargarConfiguracion);
             */

            this.configuracion = Opciones.CargarConfiguracion();
            InicializarConexiones();

            // Si las dos conexiones estan establecidas...
            if (LocalARemota != null)
            {
                InicializarSincronizacion();
            }
            else
            {
                LocalARemota = new SincronizacionViewModel();
            }
        }

        ~MainViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public ConexionRemotaViewModel ConexionRemota { get; private set; }
        public ConexionLocalViewModel ConexionLocal { get; private set; }
        public OpcionesViewModel Opciones { get; private set; }
        
        public ExploradorViewModel ExploradorLocal
        {
            get { return exploradorLocal; }
            private set
            {
                if (value != exploradorLocal)
                {
                    exploradorLocal = value;
                    this.RaisePropertyChanged("ExploradorLocal");
                }
            }
        }

        public ExploradorViewModel ExploradorRemoto
        {
            get { return exploradorRemoto; }
            private set
            {
                if (value != exploradorRemoto)
                {
                    exploradorRemoto = value;
                    this.RaisePropertyChanged("ExploradorRemoto");
                }
            }
        }

        public SincronizacionViewModel LocalARemota
        {
            get { return localARemota; }
            private set
            {
                if (value != localARemota)
                {
                    localARemota = value;
                    this.RaisePropertyChanged("LocalARemota");
                }
            }
        }

        #endregion

        #region Eventos

        public event EventHandler<EventArgs> AmbasConexionesEstablecidas;

        #endregion

        #region Funciones

        private void InicializarTemporizador()
        {
            temporizador = new DispatcherTimer();
            temporizador.Stop();
            temporizador.Interval = new TimeSpan(0, 0, 20);
            temporizador.Tick += new EventHandler(ManejarAlarmaTemporizador);
        }

        private void InicializarConexiones()
        {
            ConexionLocal.Parametros = configuracion.ParametrosConexionLocal;
            ConexionRemota.Parametros = configuracion.ParametrosConexionRemota;

            if (ConexionLocal.Parametros != null && configuracion.UsuarioLocal != null && configuracion.ContrasenaLocal != null)
            {
                ConexionLocal.Conectar(configuracion.UsuarioLocal, configuracion.ContrasenaLocal);
            }

            if (ConexionRemota.Parametros != null && configuracion.UsuarioRemoto != null && configuracion.ContrasenaRemota != null)
            {
                ConexionRemota.Conectar(configuracion.UsuarioRemoto, configuracion.ContrasenaRemota);
            }
        }

        private void InicializarSincronizacion()
        {
            bool AsincronicoLocal = ExploradorLocal.OperacionAsincronica;
            bool AsincronicoRemoto = ExploradorRemoto.OperacionAsincronica;

            ExploradorLocal.OperacionAsincronica = false;
            ExploradorRemoto.OperacionAsincronica = false;

            HashSet<string> RutasDeTablaLocales = new HashSet<string>();
            HashSet<string> RutasDeTablaRemotas = new HashSet<string>();
            List<string[]> AsociacionesValidas = new List<string[]>();

            foreach (string[] Asociacion in configuracion.Asociaciones)
            {
                string RutaLocal = RutaColumnaARutaTabla(Asociacion[0]);
                string RutaRemota = RutaColumnaARutaTabla(Asociacion[1]);

                if (RutasDeTablaLocales.Add(RutaLocal))
                {
                    ExploradorLocal.ExpandirRuta(RutaLocal);
                }

                if (RutasDeTablaRemotas.Add(RutaRemota))
                {
                    ExploradorRemoto.ExpandirRuta(RutaRemota);
                }

                // Pueden haber espacios en blanco debido a que no todas las columnas destino estan apareadas
                if (RutaLocal != string.Empty && RutaRemota != string.Empty)
                {
                    AsociacionesValidas.Add(Asociacion);
                }
            }

            ExploradorLocal.OperacionAsincronica = AsincronicoLocal;
            ExploradorRemoto.OperacionAsincronica = AsincronicoRemoto;

            sistemaConfigurado = true;
            LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, AsociacionesValidas);
        }
        /*
        private void MensajeroConfiguracionCargada(object parametro)
        {
            this.configuracion = (Configuracion)parametro;

            InicializarConexiones();

            // Si las dos conexiones estan establecidas...
            if (LocalARemota != null)
            {
                InicializarSincronizacion();
            }
            else
            {
                LocalARemota = new SincronizacionViewModel();
            }
        }

        private void MensajeroConfiguracionGuardada()
        {
            MessageBox.Show("Configuracion guardada correctamente");
        }
        */

        private string RutaColumnaARutaTabla(string RutaColumna)
        {
            if (RutaColumna == null)
            {
                throw new ArgumentNullException("RutaColumna");
            }

            if (RutaColumna == string.Empty)
            {
                return string.Empty;
            }

            // PasosRutaColumna = Servidor + Base de datos + Tabla + Columna + ""
            string[] PasosRutaColumna = RutaColumna.Split('\\');
            // RutaTabla = Servidor + Base de datos + Tabla
            string RutaTabla = PasosRutaColumna[0] + "\\" + PasosRutaColumna[1] + "\\" + PasosRutaColumna[2];

            return RutaTabla;
        }

        private ExploradorViewModel CrearExplorador(ConexionViewModel Conexion)
        {            
            ObservableCollection<NodoViewModel> Nodos = new ObservableCollection<NodoViewModel>()
            {
                new NodoViewModel(Conexion.Parametros.Servidor + "(" + Conexion.Parametros.Instancia + ")", NivelDeNodo.Servidor)
            };

            return new ExploradorViewModel(Nodos, Conexion);            
        }

        private void ConexionLocalActiva(ConexionLocalViewModel Conexion)
        {
            if (Conexion.Estado == ConnectionState.Open)
            {
                if (ExploradorLocal != null)
                {
                    ExploradorLocal.Dispose();
                    ExploradorLocal = null;
                }

                ExploradorLocal = CrearExplorador(Conexion);
                ExploradorLocal.OperacionAsincronica = false;

                if (ConexionRemota.Estado == ConnectionState.Open)
                {
                    DispararAmbasConexionesEstablecidas(new EventArgs());
                }
            }
        }

        private void ConexionRemotaActiva(ConexionRemotaViewModel Conexion)
        {
            if (Conexion.Estado == ConnectionState.Open)
            {
                if (ExploradorRemoto != null)
                {
                    ExploradorRemoto.Dispose();
                    ExploradorRemoto = null;
                }

                ExploradorRemoto = CrearExplorador(Conexion);
                ExploradorRemoto.OperacionAsincronica = true;

                if (ConexionLocal.Estado == ConnectionState.Open)
                {
                    DispararAmbasConexionesEstablecidas(new EventArgs());
                }
            }
        }

        private void SincronizacionLista(SincronizacionViewModel Sincronizacion)
        {
            if (Sincronizacion.Listo == false)
            {
                temporizador.Stop();
            }
            else
            {
                if (sistemaConfigurado == false)
                {
                    try
                    {
                        ConfigurarSistema();
                        if (sistemaConfigurado == true)
                        {
                            temporizador.Start();
                            MessageBox.Show("El sistema se configuró correctamente");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.MostrarPilaDeExcepciones());
                    }
                }
                else
                {
                    temporizador.Start();
                }
            }
        }

        private void ConfigurarSistema()
        {
            string[] NodosOrigen = LocalARemota.RutasDeNodosDeOrigen();

            // Guardamos el arbol de nodos de ExploradorLocal. Sera util unos pasos mas adelante
            ExploradorViewModel ExploradorLocalViejo = ExploradorLocal;

            // Creamos un usuario en la base de datos local con los privilegios necesarios 
            // para leer las columnas de origen
            if (!ConexionLocal.CrearUsuarioOrdinario(NodosOrigen))
            {
                throw new Exception("No se pudo crear el usuario Netzuela dentro de la base de datos local. La sincronización no puede proceder");
            }

            // Cambiamos de usuario
            ConexionLocal.Desconectar();
            ConexionLocal.ConexionOrdinaria();

            // Expandimos los nodos locales en la nueva conexion para poder operar sobre ellos
            HashSet<string> RutasDeTabla = new HashSet<string>();

            foreach (string RutaDeColumna in NodosOrigen)
            {
                string RutaDeTabla = RutaColumnaARutaTabla(RutaDeColumna);

                // Si ya esa ruta fue expandida, no la expandamos otra vez
                if (RutasDeTabla.Add(RutaDeTabla))
                {
                    ExploradorLocal.ExpandirRuta(RutaDeTabla);
                }
            }
            
            // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
            LocalARemota.RecargarTablasLocales(ExploradorLocal.Nodos);

            // Ahora tenemos que borrar todos los nodos y tablas que no se van a utilizar mas. Para ello, 
            // simplemente borramos todo el ExploradorLocal viejo.
            ExploradorLocalViejo.Dispose();
            ExploradorLocalViejo = null;

            sistemaConfigurado = true;
            /*
            object parametros = new object [] {
                this.ConexionLocal.Parametros, 
                this.ConexionRemota.Parametros,
                this.ConexionLocal.UsuarioNetzuela,
                this.ConexionLocal.ContrasenaNetzuela,
                "maricoerconio".ConvertirASecureString(),
                "1234".ConvertirASecureString(),
                LocalARemota.Tablas
            };
            Mensajeria.Mensajero.NotifyColleagues(Mensajeria.GuardarConfiguracion, parametros);
             */
            Opciones.GuardarConfiguracion(new Configuracion()
            { 
                Tablas = LocalARemota.Tablas,
                ContrasenaLocal = this.ConexionLocal.Contrasena,
                ContrasenaRemota = this.ConexionRemota.Contrasena,
                ParametrosConexionLocal = this.ConexionLocal.Parametros,
                ParametrosConexionRemota = this.ConexionRemota.Parametros,
                UsuarioLocal = this.ConexionLocal.Usuario,
                UsuarioRemoto = this.ConexionRemota.Usuario
            });
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            configuracion = null;
            sistemaConfigurado = false;
            observadorSincronizacion = null;
            AmbasConexionesEstablecidas -= ManejarAmbasConexionesEstablecidas;
            
            if (borrarCodigoAdministrado)
            {
                if (exploradorLocal != null)
                {
                    exploradorLocal.Dispose();
                    exploradorLocal = null;
                }

                if (exploradorRemoto != null)
                {
                    exploradorRemoto.Dispose();
                    exploradorRemoto = null;
                }

                if (localARemota != null)
                {
                    localARemota.Dispose();
                    localARemota = null;
                }

                if (temporizador != null)
                {
                    temporizador.Tick -= ManejarAlarmaTemporizador;
                    temporizador.Stop();
                    temporizador = null;
                }

                if (ConexionLocal != null)
                {
                    ConexionLocal.Dispose();
                    ConexionLocal = null;
                }

                if (ConexionRemota != null)
                {
                    ConexionRemota.Dispose();
                    ConexionRemota = null;
                }
            }
        }

        protected virtual void ManejarAmbasConexionesEstablecidas(object Remitente, EventArgs Args)
        {
            if (LocalARemota == null)
            {
                LocalARemota = new SincronizacionViewModel();
            }

            observadorSincronizacion = new PropertyObserver<SincronizacionViewModel>(this.LocalARemota)
                .RegisterHandler(n => n.Listo, this.SincronizacionLista);
        }

        protected virtual void DispararAmbasConexionesEstablecidas(EventArgs e)
        {
            if (AmbasConexionesEstablecidas != null)
            {
                AmbasConexionesEstablecidas(this, e);
            }
        }

        protected virtual void ManejarAlarmaTemporizador(object Remitente, EventArgs Argumentos)
        {
            try
            {
                //_Temporizador.Stop();

                NodoViewModel[] NodosOrigen = LocalARemota.NodosDeOrigen();
                NodoViewModel[] NodosDestino = LocalARemota.NodosDeDestino();

                // Expandimos nuevamente los nodos locales para verificar cambios
                HashSet<NodoViewModel> Tablas = new HashSet<NodoViewModel>();

                foreach (NodoViewModel NodoColumna in NodosOrigen)
                {
                    // Si ya esta tabla fue expandida, no la expandamos otra vez
                    if (Tablas.Add(NodoColumna.Padre))
                    {
                        ExploradorLocal.Reexpandir(NodoColumna.Padre);
                    }
                }

                Tablas.Clear();

                // Expandimos nuevamente los nodos remotos para saber que cambios hay que hacerles
                bool anterior = ExploradorRemoto.OperacionAsincronica;
                ExploradorRemoto.OperacionAsincronica = false;

                foreach (NodoViewModel NodoColumna in NodosDestino)
                {
                    // Si ya esta tabla fue expandida, no la expandamos otra vez
                    if (Tablas.Add(NodoColumna.Padre))
                    {
                        ExploradorRemoto.Reexpandir(NodoColumna.Padre);
                    }
                }

                ExploradorRemoto.OperacionAsincronica = anterior;
                LocalARemota.ActualizarTodasLasTablas();

                foreach (KeyValuePair<NodoViewModel, DataTable> Par in LocalARemota.TablasAEnviar())
                {
                    DataTable t = Par.Value.GetChanges();
                    
                    if (t != null)
                    {
                        ExploradorRemoto.EscribirTabla(Par.Key, t);
                        t.Dispose();
                        Par.Value.AcceptChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
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