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
        private bool _MostrarView;

        #endregion

        #region Constructores

        public DetectarServidoresLocalesViewModel(DatosDeConexion Datos)
        {
            this.ServidoresDetectados = new ObservableCollection<ServidorLocal>();
            this.MostrarView = true;
            this.Datos = Datos;
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; private set; }
        public DatosDeConexion Datos { get; set; }
        public bool MostrarView
        {
            get { return _MostrarView; }
            private set
            {
                if (value != _MostrarView)
                {
                    _MostrarView = value;
                    base.RaisePropertyChanged("MostrarView");
                }
            }
        }

        public ICommand SeleccionarOrden
        {
            get { return _SeleccionarOrden ?? (_SeleccionarOrden = new RelayCommand(() => this.MostrarView = false)); }
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
