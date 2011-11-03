using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Datos;        // ServidorLocal
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class DetectarServidoresLocalesViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _SeleccionarOrden;
        private bool? _CerrarView;

        #endregion

        #region Constructores

        public DetectarServidoresLocalesViewModel(ObservableCollection<ServidorLocal> ServidoresDetectados)
        {
            this.ServidoresDetectados = ServidoresDetectados;
            this._SeleccionarOrden = null;
            this.Datos = new DatosDeConexion();
            this._CerrarView = null;
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; private set; }
        public DatosDeConexion Datos { get; set; }
        public bool? CerrarView
        {
            get { return _CerrarView; }
            private set
            {
                if (value != _CerrarView)
                {
                    _CerrarView = value;
                    base.RaisePropertyChanged("CerrarView");
                }
            }
        }

        public ICommand SeleccionarOrden
        {
            get { return _SeleccionarOrden ?? (_SeleccionarOrden = new RelayCommand(() => this.CerrarView = true)); }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        // ...

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
