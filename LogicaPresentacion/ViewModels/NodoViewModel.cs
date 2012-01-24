using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Comunes;      // ConvertirALista
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Esta clase agrega dos nuevas propiedades a Nodo: Explorador (de tipo ExploradorViewModel)
    /// y Expandido (de tipo bool). Ademas convierte Padre e Hijos a NodoViewModel para mantener
    /// la consistencia de esta clase.
    /// </summary>
    public class NodoViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private Nodo _Nodo;

        #endregion

        #region Constructores
        
        private NodoViewModel(Nodo Nodo)
        {
            _Nodo = Nodo;
            InicializacionComun1();
        }

        public NodoViewModel()
        {
            _Nodo = new Nodo();
            InicializacionComun1();
        }
            
        public NodoViewModel(string Nombre)
        {
            _Nodo = new Nodo(Nombre);
            InicializacionComun1();
        }
            
        public NodoViewModel(string Nombre, int Nivel)
        {
            _Nodo = new Nodo(Nombre, Nivel);
            InicializacionComun1();
        }            

        public NodoViewModel(string Nombre, NodoViewModel Padre)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");

            _Nodo = new Nodo(Nombre);
            InicializacionComun2(Padre);
        }

        public NodoViewModel(string Nombre, NodoViewModel Padre, string[] Hijos)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");

            _Nodo = new Nodo(Nombre);
            InicializacionComun2(Padre);

            foreach (string s in Hijos)
            {
                NodoViewModel N = new NodoViewModel(s, this);
            }

            this.Expandido = true;        
        }

        public NodoViewModel(string Nombre, NodoViewModel Padre, ObservableCollection<NodoViewModel> Hijos)
        {
            if (Padre == null)
                throw new ArgumentNullException("Padre");

            _Nodo = new Nodo(Nombre);
            InicializacionComun2(Padre);

            foreach (NodoViewModel n in Hijos)
            {
                this.AgregarHijo(n);
            }

            this.Expandido = true;
        }

        ~NodoViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region Propiedades

        public string Nombre
        {
            get { return _Nodo.Nombre; }
            set { _Nodo.Nombre = value; }
        }

        public string NombreParaMostrar
        {
            get
            {
                string Resultado = this.Nombre;

                if (this.Sociedad != null)
                {
                    if (this._Nodo == this.Sociedad.ColumnaDestino)
                    {
                        Resultado += (this.Sociedad.ColumnaOrigen == null) ? "" : "<-" + this.Sociedad.ColumnaOrigen.Nombre;
                    }
                    else if (this._Nodo == this.Sociedad.ColumnaOrigen)
                    {
                        Resultado += (this.Sociedad.ColumnaDestino == null) ? "" : "->" + this.Sociedad.ColumnaDestino.Nombre;
                    }
                }

                return Resultado;
            }
        }

        public int Nivel
        {
            get { return _Nodo.Nivel; }
            set { _Nodo.Nivel = value; }
        }

        public NodoViewModel Padre { get; set; }
        public ObservableCollection<NodoViewModel> Hijos { get; private set; }
        
        public AsociacionDeColumnas Sociedad
        {
            get { return _Nodo.Sociedad; }
            private set { _Nodo.Sociedad = value; }
        }

        public TablaDeAsociaciones TablaDeSocios
        {
            get { return _Nodo.TablaDeSocios; }
            private set { _Nodo.TablaDeSocios = value; }
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
            _Nodo.AgregarARepositorio(this);
        }

        private void InicializacionComun2(NodoViewModel Padre)
        {
            Padre.AgregarHijo(this);
            this.Hijos = new ObservableCollection<NodoViewModel>();
            this.Expandido = false;
            _Nodo.AgregarARepositorio(this);
        }

        protected virtual void ManejarCambioEnColumnas(object Remitente, EventoCambioEnColumnasArgs Argumentos)
        {
            RaisePropertyChanged("NombreParaMostrar");
        }

        protected void Dispose(bool BorrarCodigoAdministrado)
        {
            if (Sociedad != null)
            {
                Sociedad.CambioEnColumnas -= this.ManejarCambioEnColumnas;

                if (_Nodo == Sociedad.ColumnaDestino)
                {
                    Sociedad.QuitarDestino();
                }
                else if (_Nodo == Sociedad.ColumnaOrigen)
                {
                    Sociedad.QuitarOrigen();
                }
                
                Sociedad = null;
            }

            if (_Nodo != null)
            {
                _Nodo.QuitarDeRepositorio();
                _Nodo = null;
            }

            if (Explorador != null)
                Explorador = null;

            if (BorrarCodigoAdministrado)
            {
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

        public void AgregarHijo(NodoViewModel Nodo)
        {
            if (Nodo == null)
                throw new ArgumentNullException("Nodo");

            try
            {
                Nodo.Padre = this;
                Nodo.Nivel = this.Nivel + 1;
                Nodo.Explorador = this.Explorador;

                this.Hijos.Add(Nodo);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el hijo \"" + Nodo.Nombre + "\" al NodoViewModel \"" + this.Nombre + "\"", ex);
            }
        }

        public TablaDeAsociaciones CrearTablaDeAsociaciones()
        {
            try
            {
                List<Nodo> Lista = new List<Nodo>();

                foreach (NodoViewModel N in Hijos)
                {
                    Lista.Add(N._Nodo);
                }

                _Nodo.CrearTablaDeAsociaciones(Lista);

                foreach (NodoViewModel Hijo in Hijos)
                {
                    /*
                     * Quiero ser notificado cuando ocurra una cambio en ColumnaOrigen o 
                     * ColumnaDestino del MapeoDeColumnas asociado a este NodoViewModel.
                     */
                    Hijo.Sociedad.CambioEnColumnas -= Hijo.ManejarCambioEnColumnas;
                    Hijo.Sociedad.CambioEnColumnas += Hijo.ManejarCambioEnColumnas;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo crear la TablaDeAsociaciones sobre el NodoViewModel \"" + this.Nombre + "\"", ex);
            }

            return TablaDeSocios;
        }

        public void AsociarCon(NodoViewModel NodoOrigen)
        {
            try
            {
                if (NodoOrigen == null)
                    throw new ArgumentNullException("NodoOrigen");                

                // El nuevo nodo tambien quiere saber cuándo ocurre un cambio en las columnas
                this.Sociedad.CambioEnColumnas -= NodoOrigen.ManejarCambioEnColumnas;
                this.Sociedad.CambioEnColumnas += NodoOrigen.ManejarCambioEnColumnas;

                _Nodo.AsociarCon(NodoOrigen._Nodo);                
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
                NodoViewModel NodoOrigen = this.Sociedad.ColumnaOrigen.BuscarEnRepositorio();

                _Nodo.Desasociarse();
                this.Sociedad.CambioEnColumnas -= NodoOrigen.ManejarCambioEnColumnas;
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

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
