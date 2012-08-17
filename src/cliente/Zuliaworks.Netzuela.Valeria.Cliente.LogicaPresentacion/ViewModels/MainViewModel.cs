namespace Zuliaworks.Netzuela.Valeria.Cliente.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;               // ObservableCollection
    using System.Data;                                  // DataTable
    using System.Linq;
    using System.Security;                              // SecureString
    using System.Text;
    using System.Windows;                               // MessageBox
    using System.Windows.Threading;                     // DispatcherTimer
    
    using MvvmFoundation.Wpf;                           // PropertyObserver<>, ObservableObject
    using Zuliaworks.Netzuela.Valeria.Comunes;          // Constantes
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;    // EventoEscribirTablaCompletado
    using Zuliaworks.Netzuela.Valeria.Cliente.Logica;   // TablaMapeada

    /// <summary>
    /// 
    /// </summary>
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        #region Variables y Constantes

        private static TimeSpan intervaloTemporizador = new TimeSpan(0, 0, 1);
        private static TimeSpan tiempoCero = new TimeSpan(0, 0, 0);
        private readonly PropertyObserver<ConexionLocalViewModel> observadorConexionLocalEstablecida;
        private readonly PropertyObserver<ConexionRemotaViewModel> observadorConexionRemotaEstablecida;
        private readonly PropertyObserver<OpcionesViewModel> observadorIntervaloCompensacion;
        private PropertyObserver<SincronizacionViewModel> observadorSincronizacion;
        private Configuracion configuracion;
        private ExploradorViewModel exploradorLocal;
        private ExploradorViewModel exploradorRemoto;
        private SincronizacionViewModel localARemota;
        private DispatcherTimer temporizador;
        private TimeSpan contador;
        private bool sistemaConfigurado;

        #endregion
        
        #region Constructores

        public MainViewModel()
        {
            this.InicializarTemporizador();
            
            this.ConexionLocal = new ConexionLocalViewModel();
            this.ConexionRemota = new ConexionRemotaViewModel();
            this.Opciones = new OpcionesViewModel();
            
            this.observadorIntervaloCompensacion = new PropertyObserver<OpcionesViewModel>(this.Opciones)
                .RegisterHandler(n => n.IntervaloCompensacion, (o) => this.Contador = o.IntervaloCompensacion);

            this.observadorConexionLocalEstablecida = new PropertyObserver<ConexionLocalViewModel>(this.ConexionLocal)
                .RegisterHandler(n => n.Estado, this.ConexionLocalActiva);

            this.observadorConexionRemotaEstablecida = new PropertyObserver<ConexionRemotaViewModel>(this.ConexionRemota)
                .RegisterHandler(n => n.Estado, this.ConexionRemotaActiva);

            this.AmbasConexionesEstablecidas += new EventHandler<EventArgs>(ManejarAmbasConexionesEstablecidas);

            this.configuracion = Opciones.CargarConfiguracion();
            this.InicializarConexiones();

            // Si las dos conexiones estan establecidas...
            if (this.LocalARemota != null)
            {
                this.InicializarSincronizacion();
            }
            else
            {
                this.LocalARemota = new SincronizacionViewModel();
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

        public string ContadorString
        {
            get { return this.contador.ToString(); }
        }

        public TimeSpan Contador
        {
            get { return this.contador; }
            set
            {
                if (value != this.contador)
                {
                    this.contador = value;
                    this.RaisePropertyChanged("Contador");
                    this.RaisePropertyChanged("ContadorString");
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
            temporizador.Interval = MainViewModel.intervaloTemporizador;
            temporizador.Tick += new EventHandler(ManejarAlarmaTemporizador);
        }

        private void InicializarConexiones()
        {
            if (ConexionLocal.Parametros != null && configuracion.UsuarioLocal != null && configuracion.ContrasenaLocal != null)
            {
                ConexionLocal.Parametros = configuracion.ParametrosConexionLocal;
                ConexionLocal.Conectar(configuracion.UsuarioLocal, configuracion.ContrasenaLocal);
            }

            ConexionRemota.Parametros = configuracion.ParametrosConexionRemota;

            if (ConexionRemota.Parametros != null && configuracion.UsuarioRemoto != null && configuracion.ContrasenaRemota != null)
            {
                ConexionRemota.Conectar(configuracion.UsuarioRemoto, configuracion.ContrasenaRemota);
                ConexionRemota.TiendaId = configuracion.TiendaId;
                ConexionRemota.NombreTienda = configuracion.NombreTienda;
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
            string[] nodosOrigen = LocalARemota.RutasDeNodosDeOrigen();
            List<string[]> asociaciones = LocalARemota.Asociaciones();
            
            // Creamos un usuario en la base de datos local con los privilegios necesarios 
            // para leer las columnas de origen
            if (!ConexionLocal.CrearUsuarioOrdinario(nodosOrigen))
            {
                throw new Exception("No se pudo crear el usuario Netzuela dentro de la base de datos local. La sincronización no puede proceder");
            }

            // Cambiamos de usuario
            ConexionLocal.Desconectar();
            ConexionLocal.ConexionOrdinaria();

            // Expandimos los nodos locales en la nueva conexion para poder operar sobre ellos
            HashSet<string> RutasDeTabla = new HashSet<string>();

            foreach (string RutaDeColumna in nodosOrigen)
            {
                string RutaDeTabla = RutaColumnaARutaTabla(RutaDeColumna);

                // Si ya esa ruta fue expandida, no la expandamos otra vez
                if (RutasDeTabla.Add(RutaDeTabla))
                {
                    ExploradorLocal.ExpandirRuta(RutaDeTabla);
                }
            }
            
            // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
            LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, asociaciones);
            
            sistemaConfigurado = true;
            Opciones.GuardarConfiguracion(new Configuracion()
            {
                TiendaId = ConexionRemota.TiendaId,
                NombreTienda = ConexionRemota.NombreTienda,
                Tablas = LocalARemota.Tablas,
                ContrasenaLocal = this.ConexionLocal.Contrasena,
                ContrasenaRemota = this.ConexionRemota.Contrasena,
                ParametrosConexionLocal = this.ConexionLocal.Parametros,
                ParametrosConexionRemota = this.ConexionRemota.Parametros,
                UsuarioLocal = this.ConexionLocal.Usuario,
                UsuarioRemoto = this.ConexionRemota.Usuario
            });
        }

        private void CompensarBasesDeDatos()
        {
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

        protected virtual void ManejarAmbasConexionesEstablecidas(object remitente, EventArgs args)
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

        protected virtual void ManejarAlarmaTemporizador(object remitente, EventArgs args)
        {
            this.Contador = this.Contador.Subtract(MainViewModel.intervaloTemporizador);

            if (this.Contador <= MainViewModel.tiempoCero)
            {
                try
                {
                    //this.temporizador.Stop();
                    this.Contador = this.Opciones.IntervaloCompensacion;
                    this.CompensarBasesDeDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.MostrarPilaDeExcepciones());
                }
                finally
                {
                    //this.temporizador.Start();
                }
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