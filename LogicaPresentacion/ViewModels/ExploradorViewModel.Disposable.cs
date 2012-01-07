using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;              // DataTable

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    public partial class ExploradorViewModel : IDisposable
    {
        #region Implementacion de interfaces

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (_CacheDeTablas != null)
            {
                foreach(DataTable T in _CacheDeTablas.Values)
                {
                    T.Clear();
                    T.Columns.Clear();
                    T.DefaultView.Dispose();
                    T.Dispose();
                }

                _CacheDeTablas.Clear();
                _CacheDeTablas = null;
            }

            if (_Conexion != null)
                _Conexion = null;

            if (_NodoActual != null)
                _NodoActual = null;

            if (NodoTablaActual != null)
                NodoTablaActual = null;

            if (BorrarCodigoAdministrado)
            {
                if (Nodos != null)
                {
                    foreach (NodoViewModel N in Nodos)
                    {
                        N.Dispose();
                    }

                    Nodos.Clear();
                    Nodos = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
