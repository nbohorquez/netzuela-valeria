﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;               // HybridDictionary
using System.ComponentModel;                        // ProgressChangedEventArgs, AsyncOperation
using System.Threading;                             // Thread, SendOrPostCallback

namespace Zuliaworks.Netzuela.Valeria.Datos.Web
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ClienteSpuria// : Component
    {
        // Referencia 1: http://msdn.microsoft.com/en-us/library/bz33kx67.aspx
        // Referencia 2: http://msdn.microsoft.com/en-us/library/wewwczdw.aspx
        
        #region Variables y constantes

        private Random _Aleatorio;
        private ProxyDinamico _Proxy;
        private HybridDictionary _Hilos;
        private string _UriWsdlServicio;

        private const string CONTRATO_SPURIA = "ISpuria";

        #endregion

        #region Constructores

        public ClienteSpuria()
        {
            InicializarDelegados();

            _Aleatorio = new Random();
            _Hilos = new HybridDictionary();
        }

        public ClienteSpuria(string UriWsdlServicio)
            : this()
        {
            if (UriWsdlServicio == null)
                throw new ArgumentNullException("UriWsdlServicio");

            this.UriWsdlServicio = UriWsdlServicio;
        }

        #endregion

        #region Propiedades

        public string UriWsdlServicio
        {
            get { return (_Proxy == null) ? _UriWsdlServicio : _Proxy.UriWsdlServicio; }
            set
            {
                if (_Proxy == null)
                    _UriWsdlServicio = value;
                else
                    _Proxy.UriWsdlServicio = value;
            }
        }

        #endregion

        #region Funciones

        private bool TareaCancelada(object TareaID)
        {
            return (_Hilos[TareaID] == null);
        }

        protected virtual void InicializarDelegados()
        {
            _DelegadoDispararCambioEnProgresoDeOperacion = new SendOrPostCallback(AntesDeDispararCambioEnProgresoDeOperacion);
            _DelegadoDispararListarBDsCompletado = new SendOrPostCallback(AntesDeDispararListarBDsCompletado);
            _DelegadoDispararListarTablasCompletado = new SendOrPostCallback(AntesDeDispararListarTablasCompletado);
            _DelegadoDispararLeerTablaCompletado = new SendOrPostCallback(AntesDeDispararLeerTablaCompletado);
            _DelegadoDispararEscribirTablaCompletado = new SendOrPostCallback(AntesDeDispararEscribirTablaCompletado);
            _DelegadoDispararCrearUsuarioCompletado = new SendOrPostCallback(AntesDeDispararCrearUsuarioCompletado);
            _Carpintero = new DelegadoComenzarOperacion(ComenzarOperacion);
        }

        public void Armar()
        {
            try
            {
                _Proxy = new ProxyDinamico(UriWsdlServicio);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creando ProxyDinamico con argumento: \"" + UriWsdlServicio + "\"", ex);
            }
        }

        public void Conectar()
        {
            try
            {
                if (_Proxy == null)
                {
                    if (UriWsdlServicio == null)
                        throw new ArgumentNullException("UriWsdlServicio");

                    Armar();
                }

                _Proxy.Conectar(CONTRATO_SPURIA);
            }
            catch (Exception ex)
            {
                throw new Exception("Error conectando con servidor remoto Spuria", ex);
            }
        }

        public void Desconectar()
        {
            try
            {
                if (_Proxy != null)
                    _Proxy.Desconectar();
            }
            catch (Exception ex)
            {
                throw new Exception("Error desconectando de servidor remoto Spuria", ex);
            }
        }

        public void CancelarTarea(object TareaID)
        {
            try
            {
                AsyncOperation Asincronico = _Hilos[TareaID] as AsyncOperation;

                if (Asincronico != null)
                {
                    lock (_Hilos.SyncRoot)
                    {
                        _Hilos.Remove(TareaID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error cancelando la tarea " + TareaID.ToString(), ex);
            }
        }

        #endregion
    }
}
