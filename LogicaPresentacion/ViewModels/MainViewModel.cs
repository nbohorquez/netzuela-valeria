using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                                               // PropertyObserver<>
using System.Collections.ObjectModel;                                   // ObservableCollection
using System.Configuration;                                             // ConfigurationManager
using System.Windows;                                                   // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;                              // DatosDeConexion, Constantes
using Zuliaworks.Netzuela.Valeria.Logica;                               // Conexion
using Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones;   // ConexionesConfig

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        #region Variables

        private Configuracion _ConfiguracionLocal;
        private ExploradorViewModel _ExploradorLocal;
        private ExploradorViewModel _ExploradorRemoto;
        private SincronizacionViewModel _LocalARemota;
        private readonly PropertyObserver<ConexionLocalViewModel> _ObservadorConexionLocal;
        private readonly PropertyObserver<ConexionRemotaViewModel> _ObservadorConexionRemota;
        private PropertyObserver<SincronizacionViewModel> _ObservadorSincronizacion;

        #endregion
        
        #region Constructores

        public MainViewModel()
        {
            ConexionLocal = new ConexionLocalViewModel();
            ConexionRemota = new ConexionRemotaViewModel();

            _ObservadorConexionLocal = new PropertyObserver<ConexionLocalViewModel>(this.ConexionLocal)
                .RegisterHandler(n => n.Estado, this.ConexionLocalActiva);

            _ObservadorConexionRemota = new PropertyObserver<ConexionRemotaViewModel>(this.ConexionRemota)
                .RegisterHandler(n => n.Estado, this.ConexionRemotaActiva);

            if (CargarConfiguracion())
            {
                ConexionLocal.Parametros = _ConfiguracionLocal.ParametrosConexionLocal;
                ConexionRemota.Parametros = _ConfiguracionLocal.ParametrosConexionRemota;

                ConexionLocal.Conectar(_ConfiguracionLocal.UsuarioLocal, _ConfiguracionLocal.ContrasenaLocal);
                ConexionRemota.Conectar(_ConfiguracionLocal.UsuarioRemoto, _ConfiguracionLocal.ContrasenaRemota);
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

        #region Funciones

        private void ConexionLocalActiva(ConexionLocalViewModel Conexion)
        {
            if (Conexion.Estado == System.Data.ConnectionState.Open)
            {
                ObservableCollection<NodoViewModel> NodosLocales = new ObservableCollection<NodoViewModel>()
                {
                    new NodoViewModel(Conexion.Parametros.Servidor + "(" + Conexion.Parametros.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                };

                ExploradorLocal = new ExploradorViewModel(NodosLocales, Conexion.BD);
            }
        }

        private void ConexionRemotaActiva(ConexionRemotaViewModel Conexion)
        {
            if (Conexion.Estado == System.Data.ConnectionState.Open)
            {
                ObservableCollection<NodoViewModel> NodosRemotos = new ObservableCollection<NodoViewModel>()
                {
                    new NodoViewModel(Conexion.Parametros.Servidor + "(" + Conexion.Parametros.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                };

                ExploradorRemoto = new ExploradorViewModel(NodosRemotos, Conexion.BD);

                // Leemos todas las tablas de todas las bases de datos del servidor remoto
                ExploradorRemoto.ExpandirTodo();

                // Obtenemos todos los nodos que son indice de tablas en la cache
                List<NodoViewModel> NodosCache = ExploradorRemoto.ObtenerNodosCache();

                LocalARemota = new SincronizacionViewModel(NodosCache);

                _ObservadorSincronizacion = new PropertyObserver<SincronizacionViewModel>(this.LocalARemota)
                    .RegisterHandler(n => n.Listo, this.SincronizacionLista);
            }
        }

        private void SincronizacionLista(SincronizacionViewModel Sincronizacion)
        {
            if (Sincronizacion.Listo == false)
                return;

            List<string> NodosOrigen = new List<string>();

            // Obtenemos las columnas de origen que son utilizadas en la sincronizacion
            foreach (TablaMapeada TM in Sincronizacion.Tablas)
            {
                foreach (MapeoDeColumnas MC in TM.MapasColumnas)
                {
                    if (MC.ColumnaOrigen != null)
                        NodosOrigen.Add(MC.ColumnaOrigen.RutaCompleta());
                }
            }
            
            // Creamos un usuario en la base de datos local con los privilegios necesarios 
            // para leer las columnas de origen            
            ConexionLocal.CrearUsuarioNetzuela(NodosOrigen.ToArray());

            // Cambiamos de usuario
            ConexionLocal.Desconectar();
            ConexionLocal.ConexionNetzuela();

            // Expandimos todos los nodos locales para poder operar sobre ellos
            ExploradorLocal.ExpandirTodo();

            // Atamos nuevamente las columnas de origen (recien cargadas) a las columnas destino
            Sincronizacion.Resincronizar(ExploradorLocal.Nodos);

            GuardarConfiguracion();

            MessageBox.Show("La sincronización se realizó correctamente");                
        }

        private void GuardarConfiguracion()
        {
            // Con codigo de 
            // http://msdn.microsoft.com/es-es/library/system.configuration.configurationmanager%28v=VS.100%29.aspx

            Configuration AppConfig;
            ConexionesConfig ConexionesGuardadas;
            AutentificacionConfig Credenciales;
            
            try
            {
                AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                ConexionesGuardadas = new ConexionesConfig();
                ConexionesGuardadas.ParametrosConexionLocal = new ParametrosDeConexionConfig(ConexionLocal.Parametros);
                ConexionesGuardadas.ParametrosConexionRemota = new ParametrosDeConexionConfig(ConexionRemota.Parametros);

                Credenciales = new AutentificacionConfig();
                Credenciales.UsuarioLocal = ConexionLocal.UsuarioNetzuela.Encriptar();
                Credenciales.ContrasenaLocal = ConexionLocal.ContrasenaNetzuela.Encriptar();

                // Esto esta aqui por joda... cuando tenga el servidor de Netzuela listo, aqui va 
                // a haber una vaina seria.
                Credenciales.UsuarioRemoto = "maricoerconio".ConvertirASecureString().Encriptar();
                Credenciales.ContrasenaRemota = "1234".ConvertirASecureString().Encriptar();

                AppConfig.Sections.Add("conexionesGuardadas", ConexionesGuardadas);
                AppConfig.Sections.Add("credenciales", Credenciales);

                AppConfig.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("conexionesGuardadas");
                ConfigurationManager.RefreshSection("credenciales");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }
        }

        private bool CargarConfiguracion()
        {
            bool Resultado = false;
            _ConfiguracionLocal = new Configuracion();

            try
            {
                ConexionesConfig ConexionesGuardadas = ConfigurationManager.GetSection("conexionesGuardadas")
                    as ConexionesConfig;

                AutentificacionConfig Credenciales = ConfigurationManager.GetSection("credenciales")
                    as AutentificacionConfig;

                if (ConexionesGuardadas != null && Credenciales != null)
                {
                    _ConfiguracionLocal.ParametrosConexionLocal = ConexionesGuardadas.ParametrosConexionLocal.ConvertirAParametrosDeConexion();
                    _ConfiguracionLocal.ParametrosConexionRemota = ConexionesGuardadas.ParametrosConexionRemota.ConvertirAParametrosDeConexion();

                    _ConfiguracionLocal.UsuarioLocal = Credenciales.UsuarioLocal.Desencriptar();
                    _ConfiguracionLocal.ContrasenaLocal = Credenciales.ContrasenaLocal.Desencriptar();
                    _ConfiguracionLocal.UsuarioRemoto = Credenciales.UsuarioRemoto.Desencriptar();
                    _ConfiguracionLocal.ContrasenaRemota = Credenciales.ContrasenaRemota.Desencriptar();

                    Resultado = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }

            return Resultado;
        }

        #endregion
    }
}
