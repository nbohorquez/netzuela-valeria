using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                           // PropertyObserver<>
using System.Collections.ObjectModel;               // ObservableCollection
using System.Windows;                               // MessageBox
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion, Constantes
using Zuliaworks.Netzuela.Valeria.Logica;           // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        #region Variables

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
                    new NodoViewModel(Conexion.Datos.Servidor + "(" + Conexion.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
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
                    new NodoViewModel(Conexion.Datos.Servidor + "(" + Conexion.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
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

            /*
                * Creamos un usuario en la base de datos local con los privilegios necesarios 
                * para leer las columnas de origen
                */
            ConexionLocal.CrearUsuarioNetzuela(NodosOrigen.ToArray());
            ConexionLocal.Desconectar();

            // Cambiamos de usuario
            ConexionLocal.ConexionNetzuela();

            ExploradorLocal.ExpandirTodo();

            // Atamos nuevamente las columnas de origen recien cargadas a las columnas destino
            Sincronizacion.Resincronizar(ExploradorLocal.Nodos);

            MessageBox.Show("La sincronización se realizó correctamente");                
        }

        #endregion
    }
}
