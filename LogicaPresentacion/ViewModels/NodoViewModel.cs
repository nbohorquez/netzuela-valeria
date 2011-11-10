using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MvvmFoundation.Wpf;                       // ObservableObject
using System.Collections.ObjectModel;           // ObservableCollection
using Zuliaworks.Netzuela.Valeria.Logica;       // Nodo

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    /// <summary>
    /// Esta clase agrega dos nuevas propiedades a Nodo: Explorador (de tipo ExploradorViewModel)
    /// y Expandido (de tipo bool). Ademas convierte Padre e Hijos a NodoViewModel para mantener
    /// la consistencia de esta clase.
    /// </summary>
    public class NodoViewModel
    {
        #region Variables

        private Nodo _Nodo;
        private NodoViewModel _Padre;
        private ObservableCollection<NodoViewModel> _Hijos;

        #endregion

        #region Constructores

        // Con codigo de: http://www.codeproject.com/KB/cs/3ways_extend_class.aspx

        public NodoViewModel()
        {
            // La parte de Nodo
            _Nodo = new Nodo();
            
            // La parte de NodoViewModel
            this.Expandido = false;
            this.Explorador = null;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }
            
        public NodoViewModel(string Nombre)
        {
            // La parte de Nodo
            _Nodo = new Nodo(Nombre);

            // La parte de NodoViewModel
            this.Expandido = false;
            this.Explorador = null;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }
            
        public NodoViewModel(string Nombre, int Nivel)
        {
            // La parte de Nodo
            _Nodo = new Nodo(Nombre, Nivel);

            // La parte de NodoViewModel
            this.Expandido = false;
            this.Explorador = null;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio 
            _Nodo.AgregarARepositorio(this);
        }            

        public NodoViewModel(string Nombre, NodoViewModel Padre)
        {
            // La parte de Nodo
            _Nodo = new Nodo(Nombre, Padre._Nodo);

            // La parte de NodoViewModel
            this.Padre = Padre;
            //Padre.Hijos.Add(this);
            this.Expandido = false;
            this.Explorador = Padre.Explorador;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }            

        public NodoViewModel(string Nombre, NodoViewModel Padre, string[] Hijos)
        {
            // La parte de Nodo
            _Nodo = new Nodo(Nombre, Padre._Nodo, Hijos);

            // La parte de NodoViewModel
            this.Padre = Padre;
            //Padre.Hijos.Add(this);
            this.Expandido = true;
            this.Explorador = Padre.Explorador;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }

        public NodoViewModel(string Nombre, NodoViewModel Padre, ObservableCollection<NodoViewModel> Hijos)
        {
            // La parte de Nodo
            _Nodo = new Nodo(Nombre, _Padre._Nodo);

            // La parte de NodoViewModel
            this.Padre = Padre;
            this.Hijos = Hijos;
            this.Expandido = true;
            this.Explorador = Padre.Explorador;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }

        private NodoViewModel(Nodo Nodo)
        {
            // La parte de Nodo
            _Nodo = Nodo;

            // La parte de NodoViewModel
            this.Expandido = false;
            this.Explorador = null;

            // Agregamos el par (Nodo, NodoViewModel) al repositorio
            _Nodo.AgregarARepositorio(this);
        }

        #endregion

        #region Propiedades

        public string Nombre
        {
            get { return _Nodo.Nombre; }
            set { _Nodo.Nombre = value; }
        }

        public int Nivel
        {
            get { return _Nodo.Nivel; }
            set { _Nodo.Nivel = value; }
        }
       
        public NodoViewModel Padre 
        {
            get
            {
                if (_Padre == null)
                {
                    if (_Nodo.Padre == null)
                        return null;

                    // Actualizamos este NodoViewModel para que refleje la estructura en Nodo
                    _Padre = _Nodo.Padre.ExisteEnRepositorio() ? _Nodo.Padre.BuscarEnRepositorio() : new NodoViewModel(_Nodo.Padre);
                    
                    if (!_Padre.Hijos.Contains(this))
                        _Padre.Hijos.Add(this);

                    this.Explorador = _Padre.Explorador;
                }

                return _Padre;
            }
            set
            {
                if (value != _Padre)
                {
                    _Padre = value;

                    if (!_Padre.Hijos.Contains(this))
                        _Padre.Hijos.Add(this);

                    this.Explorador = _Padre.Explorador;

                    // Actualizamos Nodo para que refleje la estructura de NodoViewModel
                    if (!_Padre._Nodo.Hijos.Contains(this._Nodo))
                        _Padre._Nodo.AgregarHijo(this._Nodo);
                }
            }
        }

        public ObservableCollection<NodoViewModel> Hijos 
        {
            get
            {
                if (_Hijos == null)
                {
                    if (_Nodo.Hijos == null)
                        return null;

                    // Actualizamos este NodoViewModel para que refleje la estructura en Nodo
                    _Hijos = new ObservableCollection<NodoViewModel>();

                    foreach (Nodo n in _Nodo.Hijos)
                    {
                        //NodoViewModel nvm = new NodoViewModel(n);
                        NodoViewModel nvm = n.ExisteEnRepositorio() ? n.BuscarEnRepositorio() : new NodoViewModel(n);
                        nvm.Padre = this;
                    }
                }

                return _Hijos;
            }
            set
            {
                if (value != _Hijos)
                {
                    _Hijos = value;

                    foreach (NodoViewModel nvm in _Hijos)
                    {
                        nvm.Padre = this;
                        Nodo n = nvm._Nodo;

                        if(!_Nodo.Hijos.Contains(n))
                        {
                            _Nodo.AgregarHijo(n);
                        }
                    }
                }
            }
        }

        // PROBABLEMENTE ESTO TIENE QUE CAMBIARSE
        public MapeoDeColumnas MapaColumna
        {
            get { return _Nodo.MapaColumna; }
            set { _Nodo.MapaColumna = value; }
        }

        public ExploradorViewModel Explorador { get; set; }
        public bool Expandido { get; set; }
        
        #endregion

        #region Funciones

        public void AgregarHijo(NodoViewModel Nodo)
        {
            Nodo.Padre = this;
        }

        #endregion
    }
}
