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
    public class AutentificacionViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private RelayCommand accederOrden;
        private bool mostrarView;

        #endregion

        #region Constructores

        public AutentificacionViewModel()
        {
            MostrarView = true;
        }

        ~AutentificacionViewModel()
        {
            this.Dispose(false);
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
                Usuario = value.ConvertirASecureString();
            }
        }
                
        public bool MostrarView 
        {
            get { return mostrarView; }
            private set
            {
                if (value != mostrarView)
                {
                    mostrarView = value;
                    this.RaisePropertyChanged("MostrarView");
                }
            }
        }

        public ICommand AccederOrden
        {
            get { return accederOrden ?? (accederOrden = new RelayCommand(() => this.MostrarView = false)); }
        }
        
        #endregion

        #region Funciones

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            accederOrden = null;
            mostrarView = false;

            if (borrarCodigoAdministrado)
            {
                if (Usuario != null)
                {
                    Usuario.Dispose();
                    Usuario = null;
                }

                if (Contrasena != null)
                {
                    Contrasena.Dispose();
                    Contrasena = null;
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
