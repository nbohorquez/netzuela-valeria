﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // ServidorLocal, DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Logica;       // ExponerAnfitrionLocal

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class DetectarServidoresLocalesViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _SeleccionarOrden;
        private bool _MostrarView;

        #endregion

        #region Constructores

        public DetectarServidoresLocalesViewModel()
        {
            this.ServidoresDetectados = ExponerAnfitrionLocal.DetectarServidoresLocales().ConvertirAObservableCollection();
            this.MostrarView = true;
            this.Parametros = new ParametrosDeConexion();

            // Ya se jodio el chamo, ahora se conecta con el localhost a juro :)
            this.Parametros.Anfitrion = "localhost";
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; private set; }
        public ParametrosDeConexion Parametros { get; set; }
        public bool MostrarView
        {
            get { return _MostrarView; }
            private set
            {
                if (value != _MostrarView)
                {
                    _MostrarView = value;
                    RaisePropertyChanged("MostrarView");
                }
            }
        }

        public ICommand SeleccionarOrden
        {
            get { return _SeleccionarOrden ?? (_SeleccionarOrden = new RelayCommand(() => this.MostrarView = false)); }
        }

        #endregion
    }
}
