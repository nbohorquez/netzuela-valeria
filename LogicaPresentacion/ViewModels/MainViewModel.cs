using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                                               // PropertyObserver<>, ObservableObject
using System.Collections.ObjectModel;                                   // ObservableCollection
using System.Data;                                                      // DataTable
using System.Windows;                                                   // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;                              // Constantes
using Zuliaworks.Netzuela.Valeria.Logica;                               // TablaMapeada

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

            _ConfiguracionLocal = new Configuracion();

            if (CargarParametrosDeConexion())
            {
                ConexionLocal.Parametros = _ConfiguracionLocal.ParametrosConexionLocal;
                ConexionRemota.Parametros = _ConfiguracionLocal.ParametrosConexionRemota;
            }

            if (CargarCredenciales())
            {
                ConexionLocal.Conectar(_ConfiguracionLocal.UsuarioLocal, _ConfiguracionLocal.ContrasenaLocal);
                ConexionRemota.Conectar(_ConfiguracionLocal.UsuarioRemoto, _ConfiguracionLocal.ContrasenaRemota);
            }

            if (CargarTablas())
            {
                ExploradorLocal.ExpandirTodo();
                LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, _ConfiguracionLocal.Mapas);                
            }
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

                if (ConexionRemota.Estado == ConnectionState.Open)
                    DispararAmbasConexionesEstablecidas(new EventArgs());
            }
        }

        private void ConexionRemotaActiva(ConexionRemotaViewModel Conexion)
        {
            if (Conexion.Estado == System.Data.ConnectionState.Open)
            {
                ExploradorRemoto = CrearExplorador(Conexion);
                
                if (ConexionLocal.Estado == ConnectionState.Open)
                    DispararAmbasConexionesEstablecidas(new EventArgs());
            }
        }

        private void SincronizacionLista(SincronizacionViewModel Sincronizacion)
        {
            if (Sincronizacion.Listo == false)
                return;

            List<string> NodosOrigen = new List<string>();
            ExploradorViewModel ExploradorLocalViejo;

            // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
            foreach (TablaMapeada TM in Sincronizacion.Tablas)
            {
                foreach (MapeoDeColumnas MC in TM.MapasColumnas)
                {
                    if (MC.ColumnaOrigen != null)
                        NodosOrigen.Add(MC.ColumnaOrigen.BuscarEnRepositorio().RutaCompleta());
                }
            }
            
            // Guardamos el arbol de nodos de ExploradorLocal. Sera util unos pasos mas adelante
            ExploradorLocalViejo = ExploradorLocal;

            // Creamos un usuario en la base de datos local con los privilegios necesarios 
            // para leer las columnas de origen            
            ConexionLocal.CrearUsuarioNetzuela(NodosOrigen.ToArray());

            // Cambiamos de usuario
            ConexionLocal.Desconectar();
            ConexionLocal.ConexionNetzuela();

            // Expandimos todos los nodos locales para poder operar sobre ellos
            ExploradorLocal.ExpandirTodo();

            // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
            Sincronizacion.RecargarTablasLocales(ExploradorLocal.Nodos);

            // Ahora tenemos que borrar todos los nodos y tablas que no se van a utilizar mas. Para ello, 
            // simplemente borramos todo el ExploradorLocal viejo.
            ExploradorLocalViejo.Dispose();
            ExploradorLocalViejo = null;

            GuardarPreferencias();
            
            // Este codigo deberia ejecutarse periodicamente y no solo cuando se termine de
            // configurar la sincronizacion de las instancias local y remota
            Dictionary<NodoViewModel,DataTable> Tablas = LocalARemota.TablasAEnviar();
            foreach(KeyValuePair<NodoViewModel, DataTable> Par in Tablas)
            {
                ExploradorRemoto.EscribirTabla(Par.Key, Par.Value);
            }
            
            MessageBox.Show("La sincronización se realizó correctamente");                
        }

        private void ManejarAmbasConexionesEstablecidas(object Remitente, EventArgs Args)
        {
            LocalARemota = new SincronizacionViewModel();

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

        #endregion
    }
}
