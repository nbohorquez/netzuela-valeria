namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;               // ObservableCollection
    using System.Linq;
    using System.Text;
    using System.Windows;                               // MessageBox
    using System.Windows.Input;                         // ICommand

    using MvvmFoundation.Wpf;
    using Zuliaworks.Netzuela.Valeria.Comunes;
    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;
    using Zuliaworks.Netzuela.Valeria.Logica;

    public class SeleccionarTiendaViewModel : ObservableObject, IDisposable
    {
        #region Variables

        private RelayCommand seleccionarOrden;
        private bool mostrarView;
        private ConexionRemotaViewModel conexion;
        private ObservableCollection<Tienda> listaTiendas;

        #endregion

        #region Constructores

        public SeleccionarTiendaViewModel(ConexionRemotaViewModel conexion)
        {
            this.MostrarView = true;
            this.conexion = conexion;
            
            this.conexion.ListarTiendasCompletado -= this.ManejarListarTiendasCompletado;
            this.conexion.ListarTiendasCompletado += this.ManejarListarTiendasCompletado;
            this.conexion.ListarTiendasAsinc();
        }

        ~SeleccionarTiendaViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Propiedades

        public Tienda Seleccion { get; set; }

        public bool MostrarView
        {
            get 
            { 
                return this.mostrarView; 
            }
            private set
            {
                if (value != this.mostrarView)
                {
                    this.mostrarView = value;
                    this.RaisePropertyChanged("MostrarView");
                }
            }
        }

        public ObservableCollection<Tienda> ListaTiendas 
        {
            get
            {
                return this.listaTiendas;
            }
            private set
            {
                if (value != this.listaTiendas)
                {
                    this.listaTiendas = value;
                    this.RaisePropertyChanged("ListaTiendas");
                }
            }
        }

        public ICommand SeleccionarOrden
        {
            get { return this.seleccionarOrden ?? (this.seleccionarOrden = new RelayCommand(() => this.MostrarView = false)); }
        }

        #endregion

        #region Funciones
        
        private void ManejarListarTiendasCompletado(object remitente, EventoListarTiendasCompletadoArgs args)
        {
            try
            {
                if (this.ListaTiendas != null)
                {
                    this.ListaTiendas.Clear();
                    this.ListaTiendas = null;
                }

                this.ListaTiendas = new ObservableCollection<Tienda>();

                foreach (string t in args.Resultado)
                {
                    string[] fila = t.Split(':');
                    this.ListaTiendas.Add(new Tienda()
                    {
                        Id = int.Parse(fila[0]),
                        Nombre = fila[1]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.MostrarPilaDeExcepciones());
            }
        }
        
        protected void Dispose(bool borrarCodigoAdministrado)
        {
            mostrarView = false;
            this.conexion.ListarTiendasCompletado -= this.ManejarListarTiendasCompletado;
            this.conexion = null;

            if (this.ListaTiendas != null)
            {
                this.ListaTiendas.Clear();
                this.ListaTiendas = null;
            }

            if (borrarCodigoAdministrado)
            {
                
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

        #region Tipos anidados

        public struct Tienda
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        #endregion
    }
}
