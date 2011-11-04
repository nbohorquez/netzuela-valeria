using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand, ObservableObject
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

using System.Windows;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class ConexionLocalViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _DetectarOrden;
        private RelayCommand _ConectarOrden;
        private RelayCommand _DesconectarOrden;
        private bool _MostrarAutentificacionView;
        private bool _MostrarDetectarServidoresLocalesView;
        private PropertyObserver<AutentificacionViewModel> _ObservadorAutentificacion;
        private PropertyObserver<DetectarServidoresLocalesViewModel> _ObservadorServidores;
        private AutentificacionViewModel _Autentificacion;
        private DetectarServidoresLocalesViewModel _Servidores;

        #endregion

        #region Contructor

        public ConexionLocalViewModel()
        {
            Datos = new DatosDeConexion();
        }

        #endregion

        #region Propiedades

        public DatosDeConexion Datos { get; set; }
        public AutentificacionViewModel Autentificacion
        {
            get { return _Autentificacion; }
            set
            {
                if (value != _Autentificacion)
                {
                    _Autentificacion = value;
                    RaisePropertyChanged("Autentificacion");
                }
            }
        }

        public DetectarServidoresLocalesViewModel Servidores
        {
            get { return _Servidores; }
            set
            {
                if (value != _Servidores)
                {
                    _Servidores = value;
                    RaisePropertyChanged("Servidores");
                }
            }
        }

        public bool MostrarAutentificacionView
        {
            get { return _MostrarAutentificacionView; }
            set 
            {
                if (value != _MostrarAutentificacionView)
                {
                    _MostrarAutentificacionView = value;
                    RaisePropertyChanged("MostrarAutentificacionView");
                }
            }
        }

        public bool MostrarDetectarServidoresLocalesView
        {
            get { return _MostrarDetectarServidoresLocalesView; }
            set
            {
                if (value != _MostrarDetectarServidoresLocalesView)
                {
                    _MostrarDetectarServidoresLocalesView = value;
                    RaisePropertyChanged("MostrarDetectarServidoresLocalesView");
                }
            }
        }

        public ICommand DetectarOrden
        {
            get { return _DetectarOrden ?? (_DetectarOrden = new RelayCommand(this.AbrirDetectarServidoresLocalesView)); }
        }

        public ICommand ConectarOrden
        {
            get { return _ConectarOrden ?? (_ConectarOrden = new RelayCommand(this.AbrirAutentificacionView)); }
        }

        public ICommand DesconectarOrden
        {
            get { return _DesconectarOrden ?? (_DesconectarOrden = new RelayCommand(this.DesconectarClic)); }
        }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        private void AbrirDetectarServidoresLocalesView()
        {
            Servidores = new DetectarServidoresLocalesViewModel(this.Datos);

            _ObservadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.Servidores)
                .RegisterHandler(n => n.MostrarView, this.CerrarDetectarServidoresLocalesView);

            MostrarDetectarServidoresLocalesView = true;
        }

        private void CerrarDetectarServidoresLocalesView(DetectarServidoresLocalesViewModel ServidoresVM)
        {
            if (ServidoresVM.MostrarView == false)
            {
                this.MostrarDetectarServidoresLocalesView = false;
            }
        }

        private void AbrirAutentificacionView()
        {
            Autentificacion = new AutentificacionViewModel();

            _ObservadorAutentificacion = new PropertyObserver<AutentificacionViewModel>(this.Autentificacion)
                .RegisterHandler(n => n.MostrarView, this.CerrarAutentificacionView);
            
            MostrarAutentificacionView = true;
        }

        private void CerrarAutentificacionView(AutentificacionViewModel AutentificacionVM)
        {
            if (AutentificacionVM.MostrarView == false)
            {
                this.MostrarAutentificacionView = false;
            }
        }

        private void DesconectarClic()
        {
        }

        #endregion

        #region Implementaciones de interfaces

        // ...

        #endregion

        #region Tipos anidados

        // ...

        #endregion
    }
}
