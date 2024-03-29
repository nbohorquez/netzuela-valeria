﻿using System;
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
        private bool _SistemaConfigurado;
        private readonly PropertyObserver<ConexionLocalViewModel> _ObservadorConexionLocalEstablecida;
        private readonly PropertyObserver<ConexionRemotaViewModel> _ObservadorConexionRemotaEstablecida;
        private PropertyObserver<SincronizacionViewModel> _ObservadorSincronizacion;
        private DispatcherTimer _Temporizador;
        private string _TiendaID;

        #endregion
        
        #region Constructores

        public MainViewModel()
        {
            InicializarTemporizador();

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

        private void InicializarTemporizador()
        {
            _Temporizador = new DispatcherTimer();
            _Temporizador.Stop();
            _Temporizador.Interval = new TimeSpan(0, 0, 20);
            _Temporizador.Tick += new EventHandler(ManejarAlarmaTemporizador);
        }

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
            List<string[]> AsociacionesValidas = new List<string[]>();

            foreach (string[] Asociacion in _ConfiguracionLocal.Asociaciones)
            {
                string RutaLocal = RutaColumnaARutaTabla(Asociacion[0]);
                string RutaRemota = RutaColumnaARutaTabla(Asociacion[1]);

                if (RutasDeTablaLocales.Add(RutaLocal))
                    ExploradorLocal.ExpandirRuta(RutaLocal);

                if (RutasDeTablaRemotas.Add(RutaRemota))
                    ExploradorRemoto.ExpandirRuta(RutaRemota);

                // Pueden haber espacios en blanco debido a que no todas las columnas destino estan apareadas
                if (RutaLocal != string.Empty && RutaRemota != string.Empty)
                    AsociacionesValidas.Add(Asociacion);
            }

            ExploradorLocal.OperacionAsincronica = AsincronicoLocal;
            ExploradorRemoto.OperacionAsincronica = AsincronicoRemoto;

            _SistemaConfigurado = true;
            LocalARemota.Sincronizar(ExploradorLocal.Nodos, ExploradorRemoto.Nodos, AsociacionesValidas);
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
            }
            else
            {
                if (_SistemaConfigurado == false)
                {
                    try
                    {
                        ConfigurarSistema();
                        _SistemaConfigurado = true;
                        _Temporizador.Start();

                        MessageBox.Show("El sistema se configuró correctamente");                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.MostrarPilaDeExcepciones());
                    }
                }
                else
                {
                    _Temporizador.Start();
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
            if (!ConexionLocal.CrearUsuarioNetzuela(NodosOrigen))
                throw new Exception("No se pudo crear el usuario Netzuela dentro de la base de datos local. La sincronización no puede proceder");

            // Cambiamos de usuario
            ConexionLocal.Desconectar();
            ConexionLocal.ConexionNetzuela();

            // Expandimos los nodos locales para poder operar sobre ellos
            HashSet<string> RutasDeTabla = new HashSet<string>();

            foreach (string RutaDeColumna in NodosOrigen)
            {
                string RutaDeTabla = RutaColumnaARutaTabla(RutaDeColumna);

                // Si ya esa ruta fue expandida, no la expandamos otra vez
                if (RutasDeTabla.Add(RutaDeTabla))
                    ExploradorLocal.ExpandirRuta(RutaDeTabla);
            }

            // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
            LocalARemota.RecargarTablasLocales(ExploradorLocal.Nodos);

            // Ahora tenemos que borrar todos los nodos y tablas que no se van a utilizar mas. Para ello, 
            // simplemente borramos todo el ExploradorLocal viejo.
            ExploradorLocalViejo.Dispose();
            ExploradorLocalViejo = null;

            GuardarConfiguracion(LocalARemota.Tablas);
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

        protected virtual void ManejarAlarmaTemporizador(object Remitente, EventArgs Argumentos)
        {
            try
            {
                //_Temporizador.Stop();

                NodoViewModel[] NodosOrigen = LocalARemota.NodosDeOrigen();

                // Expandimos nuevamente los nodos locales para verificar cambios
                HashSet<NodoViewModel> Tablas = new HashSet<NodoViewModel>();

                foreach (NodoViewModel NodoColumna in NodosOrigen)
                {
                    // Si ya esta tabla fue expandida, no la expandamos otra vez
                    if (Tablas.Add(NodoColumna.Padre))
                        ExploradorLocal.Reexpandir(NodoColumna.Padre);
                }

                LocalARemota.ActualizarTodasLasTablas();

                foreach (KeyValuePair<NodoViewModel, DataTable> Par in LocalARemota.TablasAEnviar())
                {
                    DataTable T = Par.Value.GetChanges();
                    /*
                    if (T != null)
                    {
                        ExploradorRemoto.EscribirTabla(Par.Key, T);
                        T.Dispose();
                        Par.Value.AcceptChanges();
                    }*/
                    Par.Value.AcceptChanges();
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
