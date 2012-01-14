using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                                               // PropertyObserver<>, ObservableObject
using System.Collections.ObjectModel;                                   // ObservableCollection
using System.Data;                                                      // DataTable
using System.Security;                                                  // SecureString
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

            _ConfiguracionLocal = Configuracion.CargarConfiguracion();
            
            ConexionLocal.Parametros = _ConfiguracionLocal.ParametrosConexionLocal;
            ConexionRemota.Parametros = _ConfiguracionLocal.ParametrosConexionRemota;

            if (_ConfiguracionLocal.UsuarioLocal != null && _ConfiguracionLocal.ContrasenaLocal != null)
                ConexionLocal.Conectar(_ConfiguracionLocal.UsuarioLocal, _ConfiguracionLocal.ContrasenaLocal);

            if (_ConfiguracionLocal.UsuarioRemoto != null && _ConfiguracionLocal.ContrasenaRemota != null)
                ConexionRemota.Conectar(_ConfiguracionLocal.UsuarioRemoto, _ConfiguracionLocal.ContrasenaRemota);

            // Si las dos conexiones estan establecidas...
            if (LocalARemota != null)
            {
                ExploradorLocal.ExpandirTodo();
                //LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, _ConfiguracionLocal.Mapas);
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

            try
            {
                // Creamos un usuario en la base de datos local con los privilegios necesarios 
                // para leer las columnas de origen            
                ConexionLocal.CrearUsuarioNetzuela(NodosOrigen.ToArray());

                // Cambiamos de usuario
                ConexionLocal.Desconectar();
                ConexionLocal.ConexionNetzuela();

                // Expandimos los nodos locales para poder operar sobre ellos
                HashSet<string> RutasDeTabla = new HashSet<string>();

                foreach(string RutaDeColumna in NodosOrigen)
                {
                    // Servidor + Base de datos + tabla + columna + ""
                    string[] PasosDeRuta = RutaDeColumna.Split('\\');

                    // Servidor + Base de datos + tabla
                    string RutaDeTabla = PasosDeRuta[0] + "\\" + PasosDeRuta[1] + "\\" + PasosDeRuta[2];

                    if(RutasDeTabla.Add(RutaDeTabla))
                        ExploradorLocal.ExpandirRuta(RutaDeTabla);
                }
            
                // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
                Sincronizacion.RecargarTablasLocales(ExploradorLocal.Nodos);            

                // Ahora tenemos que borrar todos los nodos y tablas que no se van a utilizar mas. Para ello, 
                // simplemente borramos todo el ExploradorLocal viejo.
                ExploradorLocalViejo.Dispose();
                ExploradorLocalViejo = null;

                _ConfiguracionLocal = new Configuracion();
                _ConfiguracionLocal.ParametrosConexionLocal = ConexionLocal.Parametros;
                _ConfiguracionLocal.ParametrosConexionRemota = ConexionRemota.Parametros;
                _ConfiguracionLocal.UsuarioLocal = ConexionLocal.UsuarioNetzuela;
                _ConfiguracionLocal.ContrasenaLocal = ConexionLocal.ContrasenaNetzuela;
                // Esto esta aqui por joda... cuando tenga el servidor de Netzuela listo, aqui va 
                // a haber una vaina seria.
                _ConfiguracionLocal.UsuarioRemoto = "maricoerconio".ConvertirASecureString();
                _ConfiguracionLocal.ContrasenaRemota = "1234".ConvertirASecureString();
                _ConfiguracionLocal.Tablas = Sincronizacion.Tablas;

                Configuracion.GuardarConfiguracion(_ConfiguracionLocal);
                        
                // Este codigo deberia ejecutarse periodicamente y no solo cuando se termine de
                // configurar la sincronizacion de las instancias local y remota
                Dictionary<NodoViewModel, DataTable> Tablas = Sincronizacion.TablasAEnviar();
                foreach(KeyValuePair<NodoViewModel, DataTable> Par in Tablas)
                {
                    ExploradorRemoto.EscribirTabla(Par.Key, Par.Value);
                }
            
                MessageBox.Show("La sincronización se realizó correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
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
