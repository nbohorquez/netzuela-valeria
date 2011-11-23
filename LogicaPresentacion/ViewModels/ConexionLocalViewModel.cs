﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand, ObservableObject
using ObviexGeneradorContrasenas;               // RandomPassword
using System.Data;                              // DataTable, ConnectionState
using System.Security;                          // SecureString
using System.Windows;                           // MessageBox
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion
using Zuliaworks.Netzuela.Valeria.Datos;        // IBaseDeDatos
using Zuliaworks.Netzuela.Valeria.Logica;       // Conexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ConexionLocalViewModel : ConexionViewModel
    {
        #region Variables

        private RelayCommand _ConectarOrden;
        private RelayCommand _DetectarOrden;        
        private bool _MostrarAutentificacionView;
        private bool _MostrarDetectarServidoresLocalesView;
        private PropertyObserver<AutentificacionViewModel> _ObservadorAutentificacion;
        private PropertyObserver<DetectarServidoresLocalesViewModel> _ObservadorServidores;
        private AutentificacionViewModel _Autentificacion;
        private DetectarServidoresLocalesViewModel _ServidoresDetectados;
        private SecureString _UsuarioExterno, _UsuarioNetzuela;
        private SecureString _ContrasenaExterna, _ContrasenaNetzuela;

        #endregion

        #region Contructores

        public ConexionLocalViewModel()
            : base() { }

        #endregion

        #region Propiedades

        public AutentificacionViewModel Autentificacion
        {
            get { return _Autentificacion; }
            private set
            {
                if (value != _Autentificacion)
                {
                    _Autentificacion = value;
                    RaisePropertyChanged("Autentificacion");
                }
            }
        }

        public DetectarServidoresLocalesViewModel ServidoresDetectados
        {
            get { return _ServidoresDetectados; }
            set
            {
                if (value != _ServidoresDetectados)
                {
                    _ServidoresDetectados = value;
                    RaisePropertyChanged("ServidoresDetectados");
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
            get { return _DetectarOrden ?? (_DetectarOrden = new RelayCommand(this.AbrirDetectarServidores)); }
        }

        #endregion

        #region Funciones

        private void AbrirDetectarServidores()
        {
            ServidoresDetectados = new DetectarServidoresLocalesViewModel();

            _ObservadorServidores = new PropertyObserver<DetectarServidoresLocalesViewModel>(this.ServidoresDetectados)
                .RegisterHandler(n => n.MostrarView, this.CerrarDetectarServidores);

            MostrarDetectarServidoresLocalesView = true;
        }

        private void CerrarDetectarServidores(DetectarServidoresLocalesViewModel ServidoresVM)
        {
            if (ServidoresVM.MostrarView == false)
            {
                this.MostrarDetectarServidoresLocalesView = false;
                this.Datos = ServidoresVM.Datos.Clonar();
            }
        }

        private void AbrirAutentificacion()
        {
            Autentificacion = new AutentificacionViewModel();

            _ObservadorAutentificacion = new PropertyObserver<AutentificacionViewModel>(this.Autentificacion)
                .RegisterHandler(n => n.MostrarView, this.CerrarAutentificacion);
            
            MostrarAutentificacionView = true;
        }

        private void CerrarAutentificacion(AutentificacionViewModel AutentificacionVM)
        {
            if (AutentificacionVM.MostrarView == false)
            {
                this.MostrarAutentificacionView = false;
                this._UsuarioExterno = AutentificacionVM.Usuario;
                this._ContrasenaExterna = AutentificacionVM.Contrasena;
                ConexionUsuario();
            }
        }

        public override void ConectarDesconectar()
        {
            if (Estado == ConnectionState.Open)
            {
                Desconectar();
            }
            else
            {
                AbrirAutentificacion();
            }
        }

        public void ConexionUsuario()
        {
            base.Conectar(_UsuarioExterno, _ContrasenaExterna);
        }
            
        public void ConexionNetzuela()
        {
            base.Conectar(_UsuarioNetzuela, _ContrasenaNetzuela);
        }
        
        public void CrearUsuarioNetzuela(string[] ColumnasAutorizadas)
        {
            _UsuarioNetzuela = "netzuela".ConvertirASecureString();
            _ContrasenaNetzuela = RandomPassword.Generate(20).ConvertirASecureString();

            base.CrearUsuario(
                _UsuarioNetzuela, _ContrasenaNetzuela,
                ColumnasAutorizadas, Constantes.Privilegios.SELECCIONAR
            );
        }

        #endregion
    }
}
