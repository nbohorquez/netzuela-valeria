﻿using System;
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
        private bool _MostrarView;

        #endregion

        #region Constructores

        public AutentificacionViewModel()
        {
            MostrarView = true;
        }

        #endregion

        #region Propiedades

        public SecureString Usuario { get; set; }
        public SecureString Contrasena { get; set; }

        /// <summary>
        /// Esta propiedad es una fachada. Solo sirve para tomar el string desde el View y 
        /// convertirlo inmediatamente a SecureString en Usuario. No se puede leer
        /// </summary>
        public string UsuarioString
        {
            set
            {
                SecureString Valor = value.ConvertirASecureString();
                Usuario = Valor;
            }
        }
                
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
    }
}
