namespace Zuliaworks.Netzuela.Valeria.Logica
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.ComponentModel;                    // INotifyPropertyChanged
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Comunes;      // Constantes

    /// <summary>
    /// Implementación de la unidad básica de información de una estructura de datos 
    /// en forma de árbol. Se usa para representar la organizacion interna de las 
    /// bases de datos.
    /// </summary>
    public class Nodo : Desechable
    {
        #region Constructores

        /// <summary>
        /// 
        /// </summary>
        public Nodo()
        {
            this.Nombre = null;
            this.Nivel = -1;
            this.Sociedad = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nombre"></param>
        public Nodo(string nombre)
        {
            this.Nombre = nombre;
            this.Nivel = -1;
            this.Sociedad = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="nivel"></param>
        public Nodo(string nombre, int nivel)
        {
            this.Nombre = nombre;
            this.Nivel = nivel;
            this.Sociedad = null;
        }
                  
        ~Nodo()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Nombre del nodo.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Nivel de profundidad dentro del arbol de nodos.
        /// </summary>
        public int Nivel { get; set; }

        /// <summary>
        /// Sociedad con otro nodo.
        /// </summary>
        public AsociacionDeColumnas Sociedad { get; set; }

        /// <summary>
        /// Tabla de sociedades asociada este nodo.
        /// </summary>
        public TablaDeAsociaciones TablaDeSocios { get; set; }

        #endregion

        #region Funciones

        public TablaDeAsociaciones CrearTablaDeAsociaciones(List<Nodo> columnas)
        {
            try
            {
                TablaDeAsociaciones t = new TablaDeAsociaciones(this, columnas);
                this.TablaDeSocios = t;
                return t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AsociarCon(Nodo nodoOrigen)
        {
            if (nodoOrigen == null)
            {
                throw new ArgumentNullException("nodoOrigen");
            }

            try
            {
                this.Sociedad.FijarOrigen(nodoOrigen);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Desasociarse()
        {
            try
            {
                if (this == this.Sociedad.ColumnaDestino)
                {
                    this.Sociedad.QuitarDestino();
                }
                else if (this == this.Sociedad.ColumnaOrigen)
                {
                    this.Sociedad.QuitarOrigen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void Dispose(bool borrarCodigoAdministrado)
        {
            this.QuitarDeRepositorio();
            this.Nombre = null;

            if (this.Sociedad != null)
            {
                this.Desasociarse();
                this.Sociedad = null;
            }

            if (this.TablaDeSocios != null)
            {
                this.TablaDeSocios.Dispose();
                this.TablaDeSocios = null;
            }

            if (borrarCodigoAdministrado)
            {
            }
        }

        #endregion
    }
}
