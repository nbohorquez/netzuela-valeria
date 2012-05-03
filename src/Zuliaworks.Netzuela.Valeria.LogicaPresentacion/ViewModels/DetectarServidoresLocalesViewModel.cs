namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using MvvmFoundation.Wpf;                       // ObservableObject

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Linq;
    using System.Text;
    using System.Windows.Input;                     // ICommand

    using Zuliaworks.Netzuela.Valeria.Comunes;      // ServidorLocal, DatosDeConexion
    using Zuliaworks.Netzuela.Valeria.Logica;       // ExponerAnfitrionLocal

    /// <summary>
    /// 
    /// </summary>
    public class DetectarServidoresLocalesViewModel : ObservableObject, IDisposable
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

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            _SeleccionarOrden = null;
            _MostrarView = false;
            Parametros = null;
            
            if (borrarCodigoAdministrado)
            {
                if (ServidoresDetectados != null)
                {
                    ServidoresDetectados.Clear();
                    ServidoresDetectados = null;
                }
            }
        }

        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            /*
             * En este enlace esta la mejor explicacion acerca de como implementar IDisposable
             * http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
             */

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        #endregion
    }
}
