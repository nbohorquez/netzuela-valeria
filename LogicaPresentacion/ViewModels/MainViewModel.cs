using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                           // PropertyObserver<>
using Zuliaworks.Netzuela.Valeria.Comunes;          // DatosDeConexion, Constantes
using Zuliaworks.Netzuela.Valeria.Logica;           // Conexion

using System.Windows;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel
    {
        #region Variables

        private readonly PropertyObserver<ConexionLocalViewModel> _ObservadorConexionLocal;
        private readonly PropertyObserver<ConexionRemotaViewModel> _ObservadorConexionRemota;

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

            ExploradorLocal = new ExploradorViewModel();
            ExploradorRemoto = new ExploradorViewModel();
        }

        #endregion

        #region Propiedades

        public ConexionRemotaViewModel ConexionRemota { get; private set; }
        public ConexionLocalViewModel ConexionLocal { get; private set; }
        public ExploradorViewModel ExploradorLocal { get; set; }
        public ExploradorViewModel ExploradorRemoto { get; set; }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void ConexionLocalActiva(ConexionLocalViewModel Conexion)
        {
            MessageBox.Show("AJA");
            /*
            if (ConexionLocal.Estado == System.Data.ConnectionState.Open)
            {
                ObservableCollection<Nodo> NodosLocales = new ObservableCollection<Nodo>()
                {
                    new Nodo(Local.Datos.Servidor + "(" + Local.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                };

                ArbolLocal = new Explorador(NodosLocales, Local.BD);

                trv_ExploradorLocal.DataContext = ArbolLocal;
                txt_ElementoLocal.DataContext = ArbolLocal;
                dgr_TablaLocal.DataContext = ArbolLocal;
            }*/
        }

        private void ConexionRemotaActiva(ConexionRemotaViewModel Conexion)
        {/*
            if (Remota.BD.Estado == ConnectionState.Open)
            {
                ObservableCollection<Nodo> NodosRemotos = new ObservableCollection<Nodo>()
                {
                    new Nodo(Remota.Datos.Servidor + "(" + Remota.Datos.Instancia + ")", Constantes.NivelDeNodo.SERVIDOR)
                };

                ArbolRemoto = new Explorador(NodosRemotos, Remota.BD);

                // Leemos todas las tablas de todas las bases de datos del servidor remoto
                ArbolRemoto.ExpandirTodo();

                // Obtenemos todos los nodos que son indice de tablas en la cache
                List<Nodo> NodosCache = ArbolRemoto.ObtenerNodosCache();

                foreach (Nodo Tabla in NodosCache)
                {
                    // Creamos un MapeoDeTablas por cada Tabla listada en el servidor remoto
                    // Luego se agregaran MapeoDeColumnas a estos MapeoDeTablas.
                    MapeoDeTablas MapTab = new MapeoDeTablas(Tabla);
                    LocalARemota.Add(MapTab);
                }

                trv_ExploradorRemoto.DataContext = ArbolRemoto;
                txt_ElementoRemoto.DataContext = ArbolRemoto;
                dgr_TablaRemota.DataContext = ArbolRemoto;
            }*/
        }

        #endregion
    }
}
