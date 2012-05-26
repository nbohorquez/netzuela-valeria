namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using MvvmFoundation.Wpf;                       // ObservableObject

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;           // ObservableCollection
    using System.Linq;
    using System.Text;
    
    using Zuliaworks.Netzuela.Valeria.Comunes;      // ConvertirALista
    using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

    /// <summary>
    /// Esta clase agrega dos nuevas propiedades a Nodo: Explorador (de tipo ExploradorViewModel)
    /// y Expandido (de tipo bool). Ademas convierte Padre e Hijos a NodoViewModel para mantener
    /// la consistencia de esta clase.
    /// </summary>
    public class NodoViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private Nodo nodo;

        #endregion

        #region Constructores
        
        private NodoViewModel(Nodo nodo)
        {
            this.nodo = nodo;
            this.InicializacionComun1();
        }

        public NodoViewModel()
        {
            this.nodo = new Nodo();
            this.InicializacionComun1();
        }
            
        public NodoViewModel(string nombre)
        {
            this.nodo = new Nodo(nombre);
            this.InicializacionComun1();
        }
            
        public NodoViewModel(string nombre, int nivel)
        {
            this.nodo = new Nodo(nombre, nivel);
            this.InicializacionComun1();
        }            

        public NodoViewModel(string nombre, NodoViewModel padre)
        {
            if (padre == null)
            {
                throw new ArgumentNullException("padre");
            }

            this.nodo = new Nodo(nombre);
            this.InicializacionComun2(padre);
        }

        public NodoViewModel(string nombre, NodoViewModel padre, string[] hijos)
        {
            if (padre == null)
            {
                throw new ArgumentNullException("padre");
            }

            this.nodo = new Nodo(nombre);
            this.InicializacionComun2(padre);

            foreach (string s in hijos)
            {
                NodoViewModel N = new NodoViewModel(s, this);
            }

            this.Expandido = true;        
        }

        public NodoViewModel(string nombre, NodoViewModel padre, ObservableCollection<NodoViewModel> hijos)
        {
            if (padre == null)
            {
                throw new ArgumentNullException("padre");
            }

            nodo = new Nodo(nombre);
            InicializacionComun2(padre);

            foreach (NodoViewModel n in hijos)
            {
                this.AgregarHijo(n);
            }

            this.Expandido = true;
        }

        ~NodoViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public string Nombre
        {
            get { return nodo.Nombre; }
            set { nodo.Nombre = value; }
        }

        public string NombreParaMostrar
        {
            get
            {
                string Resultado = this.Nombre;

                if (this.Sociedad != null)
                {
                    if (this.nodo == this.Sociedad.ColumnaDestino)
                    {
                        Resultado += (this.Sociedad.ColumnaOrigen == null) ? "" : "<-" + this.Sociedad.ColumnaOrigen.Nombre;
                    }
                    else if (this.nodo == this.Sociedad.ColumnaOrigen)
                    {
                        Resultado += (this.Sociedad.ColumnaDestino == null) ? "" : "->" + this.Sociedad.ColumnaDestino.Nombre;
                    }
                }

                return Resultado;
            }
        }

        public int Nivel
        {
            get { return nodo.Nivel; }
            set { nodo.Nivel = value; }
        }

        public NodoViewModel Padre { get; set; }
        public ObservableCollection<NodoViewModel> Hijos { get; private set; }
        
        public AsociacionDeColumnas Sociedad
        {
            get { return nodo.Sociedad; }
            private set { nodo.Sociedad = value; }
        }

        public TablaDeAsociaciones TablaDeSocios
        {
            get { return nodo.TablaDeSocios; }
            private set { nodo.TablaDeSocios = value; }
        }

        public ExploradorViewModel Explorador { get; set; }
        public bool Expandido { get; set; }
        
        #endregion

        #region Funciones

        private void InicializacionComun1()
        {
            this.Padre = null;
            this.Hijos = new ObservableCollection<NodoViewModel>();
            this.Expandido = false;
            this.Explorador = null;
            nodo.AgregarARepositorioDeNodos(this);
        }

        private void InicializacionComun2(NodoViewModel padre)
        {
            padre.AgregarHijo(this);
            this.Hijos = new ObservableCollection<NodoViewModel>();
            this.Expandido = false;
            nodo.AgregarARepositorioDeNodos(this);
        }

        protected virtual void ManejarCambioEnColumnas(object remitente, EventoCambioEnColumnasArgs argumentos)
        {
            this.RaisePropertyChanged("NombreParaMostrar");
        }

        protected void Dispose(bool borrarCodigoAdministrado)
        {
            if (Sociedad != null)
            {
                // Nodo.Dispose() se encarga de desechar esta propiedad
                Sociedad.CambioEnColumnas -= this.ManejarCambioEnColumnas;
            }

            if (this.ExisteEnRepositorioDeTablas())
            {
                this.BuscarEnRepositorioDeTablas().Dispose();
                this.QuitarDeRepositorioDeTablas();
            }

            this.Explorador = null;

            if (borrarCodigoAdministrado)
            {
                if (nodo != null)
                {
                    if (this.ExisteEnRepositorioDeNodos())
                    {
                        nodo.QuitarDeRepositorioDeNodos();
                    }

                    nodo.Dispose();
                    nodo = null;
                }

                if (Padre != null)
                {
                    /* Si escribo aqui: -----Padre.Hijos.Remove(this)------ ocurre un "bug" muy dificil 
                     * de detectar que se presenta la siguiente situacion:
                     * 
                     * (Empleo el termino "borrar" como equivalente a llamar a "dispose()")
                     * 
                     * 1) Suponga que procedo a borrar un nodo (nodo A) que tiene un numero arbitrario de 10 hijos a 
                     * quienes llamaremos de forma generica "nodos H".
                     * 2) Al llegar a este condicional, remuevo al nodo A de la lista de hijos de su padre con 
                     * Padre.Hijos.Remove(this) sin mayor problema.
                     * 3) Comienzo a borrar el primer hijo H y al llegar a este mismo condicional tengo que removerlo 
                     * de la lista de hijos del padre (nodo A).
                     * 4) Supongamos que este hijo H no tiene hijos propios para facilitar la explicacion de este caso.
                     * No incurrimos en "perdida de generalidad", sin embargo.
                     * 5) Cuando continue borrando los hijos H, me voy a dar cuenta que el numero de iteraciones 
                     * "Hijos.Count" cada vez se hace mas pequeño. De hecho, todo el ciclo de borrado de nodos H solo 
                     * se ejecutara 5 veces, borrando 5 hijos en lugar de los 10 originales.
                     * 
                     * ¿Que fue lo que sucedio?
                     * 
                     * Lo que ocurrio fue que, cuando se llama a Padre.Hijos.Remove(), el contador de hijos se actualiza 
                     * automaticamente afectando la iteracion que esta ocurriendo actualmente. Si se coloca un foreach
                     * en lugar de un for, el depurador se dara cuenta de esto y provocara una excepcion.
                     * 
                     * Para evitar esta situacion no se elimina el nodo de la lista de hijos, sino que se sustituye a si 
                     * mismo con un objeto nulo.
                     */

                    int i = Padre.Hijos.IndexOf(this);
                    Padre.Hijos[i] = null;
                    Padre = null;
                }

                if (Hijos != null)
                {
                    for (int i = 0; i < Hijos.Count; i++)
                    {
                        Hijos[i].Dispose();
                    }
                    
                    Hijos.Clear();
                    Hijos = null;
                }
            }
        }

        public void AgregarHijo(NodoViewModel nodo)
        {
            if (nodo == null)
            {
                throw new ArgumentNullException("nodo");
            }

            try
            {
                nodo.Padre = this;
                nodo.Nivel = this.Nivel + 1;
                nodo.Explorador = this.Explorador;
                this.Hijos.Add(nodo);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el hijo \"" + nodo.Nombre + "\" al NodoViewModel \"" + this.Nombre + "\"", ex);
            }
        }

        public TablaDeAsociaciones CrearTablaDeAsociaciones()
        {
            try
            {
                List<Nodo> lista = new List<Nodo>();

                foreach (NodoViewModel n in Hijos)
                {
                    lista.Add(n.nodo);
                }

                nodo.CrearTablaDeAsociaciones(lista);

                foreach (NodoViewModel hijo in Hijos)
                {
                    /*
                     * Quiero ser notificado cuando ocurra una cambio en ColumnaOrigen o 
                     * ColumnaDestino del MapeoDeColumnas asociado a este NodoViewModel.
                     */
                    hijo.Sociedad.CambioEnColumnas -= hijo.ManejarCambioEnColumnas;
                    hijo.Sociedad.CambioEnColumnas += hijo.ManejarCambioEnColumnas;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo crear la TablaDeAsociaciones sobre el NodoViewModel \"" + this.Nombre + "\"", ex);
            }

            return TablaDeSocios;
        }

        public void AsociarCon(NodoViewModel nodoOrigen)
        {
            try
            {
                if (nodoOrigen == null)
                {
                    throw new ArgumentNullException("nodoOrigen");
                }
                
                if (nodoOrigen.Sociedad != null)
                {
                    nodoOrigen.Desasociarse();
                }

                // El nuevo nodo tambien quiere saber cuándo ocurre un cambio en las columnas
                this.Sociedad.CambioEnColumnas -= nodoOrigen.ManejarCambioEnColumnas;
                this.Sociedad.CambioEnColumnas += nodoOrigen.ManejarCambioEnColumnas;
                this.nodo.AsociarCon(nodoOrigen.nodo);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo completar la asociación de nodos", ex);
            }
        }

        public void Desasociarse()
        {
            try
            {
                NodoViewModel NodoOrigen = this.Sociedad.ColumnaOrigen.BuscarEnRepositorioDeNodos();
                nodo.Desasociarse();
                this.Sociedad.CambioEnColumnas -= NodoOrigen.ManejarCambioEnColumnas;
                this.Sociedad = null;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo completar la desasociación de nodos", ex);
            }
        }

        #endregion

        #region Implementacion de interfaces

        public void Dispose()
        {
            // En este enlace esta la mejor explicacion acerca de como implementar IDisposable
            // http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}