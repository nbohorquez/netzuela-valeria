using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;
using System.Security;
using System.Windows.Input;
using Zuliaworks.Netzuela.Valeria.Comunes;

using System.Windows;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Este es el ViewModel asociado con AutentificacionView. Lleva el registro del nombre de usuario y contraseña.
    /// </summary>
    public class AutentificacionViewModel : ObservableObject
    {
        #region Variables

        private RelayCommand _AccederOrden;
        private SecureString _Usuario;
        private bool _MostrarView;

        #endregion

        #region Constructores

        public AutentificacionViewModel()
        {
            Usuario = string.Empty;
            MostrarView = true;
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

        public SecureString Contrasena { get; set; }
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

        public ICommand AccederOrden
        {
            get { return _AccederOrden ?? (_AccederOrden = new RelayCommand(() => this.MostrarView = false)); }
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
