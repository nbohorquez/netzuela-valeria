using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;
using System.Security;
using System.Windows.Input;
using Zuliaworks.Netzuela.Valeria.Comunes;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class AutentificacionViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _AccederOrden;
        private SecureString _Usuario;
        private SecureString _Contrasena;
        private bool? _CerrarView;

        #endregion

        #region Constructores

        public AutentificacionViewModel()
        {
            _Usuario = string.Empty.ConvertirASecureString();
            _AccederOrden = null;
            _CerrarView = null;
        }

        #endregion

        #region Propiedades

        public string Usuario 
        {
            get { return _Usuario.ConvertirAUnsecureString(); }
            set
            {
                SecureString Valor = value.ConvertirASecureString();
                _Usuario = Valor;
            }
        }

        public SecureString Contrasena 
        {
            get { return _Contrasena; }
            set 
            { 
                _Contrasena = value;
            }
        }

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

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        public ICommand AccederOrden
        {
            get { return _AccederOrden ?? (_AccederOrden = new RelayCommand(() => this.CerrarView = true)); }
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
