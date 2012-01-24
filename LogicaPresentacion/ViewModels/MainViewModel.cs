using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // PropertyObserver<>, ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using System.Data;                              // DataTable
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using System.Windows.Threading;                 // DispatcherTimer
using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes
using Zuliaworks.Netzuela.Valeria.Logica;       // TablaMapeada

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        #region Variables

        private Configuracion _ConfiguracionLocal;
        private ExploradorViewModel _ExploradorLocal;
        private ExploradorViewModel _ExploradorRemoto;
        private SincronizacionViewModel _LocalARemota;
        private readonly PropertyObserver<ConexionLocalViewModel> _ObservadorConexionLocalEstablecida;
        private readonly PropertyObserver<ConexionRemotaViewModel> _ObservadorConexionRemotaEstablecida;
        private PropertyObserver<SincronizacionViewModel> _ObservadorSincronizacion;
        private DispatcherTimer _Temporizador;
        private string _TiendaID;

        #endregion
        
        #region Constructores

        public MainViewModel()
        {
            ConexionLocal = new ConexionLocalViewModel();
            ConexionRemota = new ConexionRemotaViewModel();
                       
            _ObservadorConexionLocalEstablecida = new PropertyObserver<ConexionLocalViewModel>(this.ConexionLocal)
                .RegisterHandler(n => n.Estado, this.ConexionLocalActiva);

            _ObservadorConexionRemotaEstablecida = new PropertyObserver<ConexionRemotaViewModel>(this.ConexionRemota)
                .RegisterHandler(n => n.Estado, this.ConexionRemotaActiva);

            AmbasConexionesEstablecidas += new EventHandler<EventArgs>(ManejarAmbasConexionesEstablecidas);

            _ConfiguracionLocal = Configuracion.CargarConfiguracion();

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
            
            _Temporizador = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 5),
                IsEnabled = true,
            };

            _Temporizador.Stop();
            _Temporizador.Tick += new EventHandler(AlarmaTemporizador);
        }

        #endregion

        #region Propiedades

        public ConexionRemotaViewModel ConexionRemota { get; private set; }
        public ConexionLocalViewModel ConexionLocal { get; private set; }
        
        public ExploradorViewModel ExploradorLocal
        {
            get { return _ExploradorLocal; }
            private set
            {
                if (value != _ExploradorLocal)
                {
                    _ExploradorLocal = value;
                    RaisePropertyChanged("ExploradorLocal");
                }
            }
        }

        public ExploradorViewModel ExploradorRemoto
        {
            get { return _ExploradorRemoto; }
            private set
            {
                if (value != _ExploradorRemoto)
                {
                    _ExploradorRemoto = value;
                    RaisePropertyChanged("ExploradorRemoto");
                }
            }
        }

        public SincronizacionViewModel LocalARemota
        {
            get { return _LocalARemota; }
            private set
            {
                if (value != _LocalARemota)
                {
                    _LocalARemota = value;
                    RaisePropertyChanged("LocalARemota");
                }
            }
        }

        #endregion

        #region Eventos

        public event EventHandler<EventArgs> AmbasConexionesEstablecidas;

        #endregion

        #region Funciones

        private void InicializarConexiones()
        {
            ConexionLocal.Parametros = _ConfiguracionLocal.ParametrosConexionLocal;
            ConexionRemota.Parametros = _ConfiguracionLocal.ParametrosConexionRemota;

            if (ConexionLocal.Parametros != null && _ConfiguracionLocal.UsuarioLocal != null && _ConfiguracionLocal.ContrasenaLocal != null)
                ConexionLocal.Conectar(_ConfiguracionLocal.UsuarioLocal, _ConfiguracionLocal.ContrasenaLocal);

            if (ConexionRemota.Parametros != null && _ConfiguracionLocal.UsuarioRemoto != null && _ConfiguracionLocal.ContrasenaRemota != null)
                ConexionRemota.Conectar(_ConfiguracionLocal.UsuarioRemoto, _ConfiguracionLocal.ContrasenaRemota);
        }

        private void InicializarSincronizacion()
        {
            bool AsincronicoLocal = ExploradorLocal.OperacionAsincronica;
            bool AsincronicoRemoto = ExploradorRemoto.OperacionAsincronica;

            ExploradorLocal.OperacionAsincronica = false;
            ExploradorRemoto.OperacionAsincronica = false;

            HashSet<string> RutasDeTablaLocales = new HashSet<string>();
            HashSet<string> RutasDeTablaRemotas = new HashSet<string>();
            List<string[]> MapasValidos = new List<string[]>();

            foreach (string[] Mapa in _ConfiguracionLocal.Mapas)
            {
                string RutaLocal = RutaColumnaARutaTabla(Mapa[0]);
                string RutaRemota = RutaColumnaARutaTabla(Mapa[1]);

                if (RutasDeTablaLocales.Add(RutaLocal))
                    ExploradorLocal.ExpandirRuta(RutaLocal);

                if (RutasDeTablaRemotas.Add(RutaRemota))
                    ExploradorRemoto.ExpandirRuta(RutaRemota);

                // Pueden haber espacios en blanco debido a que no todas las columnas destino estan apareadas
                if (RutaLocal != string.Empty && RutaRemota != string.Empty)
                    MapasValidos.Add(Mapa);
            }

            ExploradorLocal.OperacionAsincronica = AsincronicoLocal;
            ExploradorRemoto.OperacionAsincronica = AsincronicoRemoto;

            LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, MapasValidos);
        }

        private string RutaColumnaARutaTabla(string RutaColumna)
        {
            if (RutaColumna == null)
                throw new ArgumentNullException("RutaColumna");

            if (RutaColumna == string.Empty)
                return string.Empty;

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
                new NodoViewModel(Conexion.Parametros.Servidor + "(" + Conexion.Parametros.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
            };

            return new ExploradorViewModel(Nodos, Conexion.Conexion);            
        }

        private void ConexionLocalActiva(ConexionLocalViewModel Conexion)
        {
            if (Conexion.Estado == System.Data.ConnectionState.Open)
            {
                ExploradorLocal = CrearExplorador(Conexion);
                ExploradorLocal.OperacionAsincronica = false;

                if (ConexionRemota.Estado == ConnectionState.Open)
                    DispararAmbasConexionesEstablecidas(new EventArgs());
            }
        }

        private void ConexionRemotaActiva(ConexionRemotaViewModel Conexion)
        {
            if (Conexion.Estado == System.Data.ConnectionState.Open)
            {
                ExploradorRemoto = CrearExplorador(Conexion);
                ExploradorRemoto.OperacionAsincronica = true;
                
                if (ConexionLocal.Estado == ConnectionState.Open)
                    DispararAmbasConexionesEstablecidas(new EventArgs());
            }
        }

        private void SincronizacionLista(SincronizacionViewModel Sincronizacion)
        {
            if (Sincronizacion.Listo == false)
            {
                _Temporizador.Stop();
                return;
            }

            List<string> NodosOrigen = new List<string>();
            ExploradorViewModel ExploradorLocalViejo;

            // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
            foreach (TablaDeAsociaciones TM in Sincronizacion.Tablas)
            {
                foreach (AsociacionDeColumnas MC in TM.Sociedades)
                {
                    if (MC.ColumnaOrigen != null)
                        NodosOrigen.Add(MC.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta());
                }
            }
            
            // Guardamos el arbol de nodos de ExploradorLocal. Sera util unos pasos mas adelante
            ExploradorLocalViejo = ExploradorLocal;

            try
            {
                // Creamos un usuario en la base de datos local con los privilegios necesarios 
                // para leer las columnas de origen
                if (!ConexionLocal.CrearUsuarioNetzuela(NodosOrigen.ToArray()))
                    throw new Exception("No se pudo crear el usuario Netzuela dentro de la base de datos local. La sincronización no puede proceder");

                // Cambiamos de usuario
                ConexionLocal.Desconectar();
                ConexionLocal.ConexionNetzuela();

                // Expandimos los nodos locales para poder operar sobre ellos
                HashSet<string> RutasDeTabla = new HashSet<string>();

                foreach(string RutaDeColumna in NodosOrigen)
                {
                    string RutaDeTabla = RutaColumnaARutaTabla(RutaDeColumna);

                    // Si ya esa ruta fue expandida, no la expandamos otra vez
                    if(RutasDeTabla.Add(RutaDeTabla))
                        ExploradorLocal.ExpandirRuta(RutaDeTabla);
                }
            
                // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
                Sincronizacion.RecargarTablasLocales(ExploradorLocal.Nodos);

                // Ahora tenemos que borrar todos los nodos y tablas que no se van a utilizar mas. Para ello, 
                // simplemente borramos todo el ExploradorLocal viejo.
                ExploradorLocalViejo.Dispose();
                ExploradorLocalViejo = null;

                GuardarConfiguracion(Sincronizacion.Tablas);

                _Temporizador.Start();
                
                MessageBox.Show("La sincronización se realizó correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        private void GuardarConfiguracion(List<TablaDeAsociaciones> Tablas)
        {
            _ConfiguracionLocal.ParametrosConexionLocal = ConexionLocal.Parametros;
            _ConfiguracionLocal.ParametrosConexionRemota = ConexionRemota.Parametros;
            _ConfiguracionLocal.UsuarioLocal = ConexionLocal.UsuarioNetzuela;
            _ConfiguracionLocal.ContrasenaLocal = ConexionLocal.ContrasenaNetzuela;
            // Esto esta aqui por joda... cuando tenga el servidor de Netzuela listo, aqui va 
            // a haber una vaina seria.
            _ConfiguracionLocal.UsuarioRemoto = "maricoerconio".ConvertirASecureString();
            _ConfiguracionLocal.ContrasenaRemota = "1234".ConvertirASecureString();
            _ConfiguracionLocal.Tablas = Tablas;

            Configuracion.GuardarConfiguracion(_ConfiguracionLocal);
        }

        private void ManejarAmbasConexionesEstablecidas(object Remitente, EventArgs Args)
        {
            //LocalARemota = new SincronizacionViewModel();

            _ObservadorSincronizacion = new PropertyObserver<SincronizacionViewModel>(this.LocalARemota)
                .RegisterHandler(n => n.Listo, this.SincronizacionLista);
        }

        protected virtual void DispararAmbasConexionesEstablecidas(EventArgs e)
        {
            if (AmbasConexionesEstablecidas != null)
            {
                AmbasConexionesEstablecidas(this, e);
            }
        }

        protected virtual void AlarmaTemporizador(object Remitente, EventArgs Argumentos)
        {
            try
            {
                //_Temporizador.Stop();

                List<string> NodosOrigen = new List<string>();

                // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
                foreach (TablaDeAsociaciones TM in LocalARemota.Tablas)
                {
                    foreach (AsociacionDeColumnas MC in TM.Sociedades)
                    {
                        if (MC.ColumnaOrigen != null)
                            NodosOrigen.Add(MC.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta());
                    }
                }

                // Expandimos los nodos locales para poder operar sobre ellos
                HashSet<string> RutasDeTabla = new HashSet<string>();

                foreach (string RutaDeColumna in NodosOrigen)
                {
                    string RutaDeTabla = RutaColumnaARutaTabla(RutaDeColumna);

                    // Si ya esa ruta fue expandida, no la expandamos otra vez
                    if (RutasDeTabla.Add(RutaDeTabla))
                        ExploradorLocal.Reexpandir(NodoViewModelExtensiones.RutaANodo(RutaDeTabla, ExploradorLocal.Nodos));
                }

                LocalARemota.RecargarTablasLocales(ExploradorLocal.Nodos);

                foreach (KeyValuePair<NodoViewModel, DataTable> Par in LocalARemota.TablasAEnviar())
                {
                    ExploradorRemoto.EscribirTabla(Par.Key, Par.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }

        #endregion
    }
}
