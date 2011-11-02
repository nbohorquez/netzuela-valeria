using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zuliaworks.Netzuela.Valeria.Datos;        // ServidorLocal
using System.Collections.ObjectModel;           // ObservableCollection

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public class DetectarServidoresLocalesViewModel
    {
        #region Variables

        // ...

        #endregion

        #region Constructores

        public DetectarServidoresLocalesViewModel(ObservableCollection<ServidorLocal> ServidoresDetectados)
        {
        }

        #endregion

        #region Propiedades

        public ObservableCollection<ServidorLocal> ServidoresDetectados { get; set; }

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
