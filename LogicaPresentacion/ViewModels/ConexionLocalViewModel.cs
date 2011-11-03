using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // RelayCommand
using System.Windows.Input;                     // ICommand
using Zuliaworks.Netzuela.Valeria.Comunes;      // DatosDeConexion

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class ConexionLocalViewModel
    {
        #region Variables

        private RelayCommand _DetectarOrden;
        private RelayCommand _ConectarOrden;
        private RelayCommand _DesconectarOrden;

        #endregion

        #region Contructor

        public ConexionLocalViewModel()
        {
        }

        #endregion

        #region Propiedades

        public DatosDeConexion Datos { get; set; }

        #endregion

        #region Eventos

        // ...

        #endregion

        #region Funciones

        public ICommand DetectarOrden
        {
            get { }
        }

        public ICommand ConectarOrden
        {
            get { }
        }

        public ICommand DesconectarOrden
        {
            get { }
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
