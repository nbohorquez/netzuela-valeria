using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;              // StateChangeEventHandler

namespace Zuliaworks.Netzuela.Valeria.Datos
{
    public class EventosComunes
    {
        #region Eventos

        public event StateChangeEventHandler CambioDeEstado;
        public event EventHandler<EventoListarBDsCompletadoArgs> ListarBasesDeDatosCompletado;
        public event EventHandler<EventoListarTablasCompletadoArgs> ListarTablasCompletado;
        public event EventHandler<EventoLeerTablaCompletadoArgs> LeerTablaCompletado;
        public event EventHandler<EventoEscribirTablaCompletadoArgs> EscribirTablaCompletado;
        public event EventHandler<EventoCrearUsuarioCompletadoArgs> CrearUsuarioCompletado;
        public event EventHandler<EventoConsultarCompletadoArgs> ConsultarCompletado;

        #endregion

        #region Funciones

        protected void ManejarCambioDeEstado(object Remitente, StateChangeEventArgs Args)
        {
            DispararCambioDeEstado(Args);
        }

        protected void ManejarListarBasesDeDatosCompletado(object Remitente, EventoListarBDsCompletadoArgs Args)
        {
            DispararListarBasesDeDatosCompletado(Args);
        }

        protected void ManejarListarTablasCompletado(object Remitente, EventoListarTablasCompletadoArgs Args)
        {
            DispararListarTablasCompletado(Args);
        }

        protected void ManejarLeerTablaCompletado(object Remitente, EventoLeerTablaCompletadoArgs Args)
        {
            DispararLeerTablaCompletado(Args);
        }

        protected void ManejarEscribirTablaCompletado(object Remitente, EventoEscribirTablaCompletadoArgs Args)
        {
            DispararEscribirTablaCompletado(Args);
        }

        protected void ManejarCrearUsuarioCompletado(object Remitente, EventoCrearUsuarioCompletadoArgs Args)
        {
            DispararCrearUsuarioCompletado(Args);
        }

        protected void ManejarConsultarCompletado(object Remitente, EventoConsultarCompletadoArgs Args)
        {
            DispararConsultarCompletado(Args);
        }

        protected virtual void DispararCambioDeEstado(StateChangeEventArgs e)
        {
            if (CambioDeEstado != null)
            {
                CambioDeEstado(this, e);
            }
        }

        protected virtual void DispararListarBasesDeDatosCompletado(EventoListarBDsCompletadoArgs e)
        {
            if (ListarBasesDeDatosCompletado != null)
            {
                ListarBasesDeDatosCompletado(this, e);
            }
        }

        protected virtual void DispararListarTablasCompletado(EventoListarTablasCompletadoArgs e)
        {
            if (ListarTablasCompletado != null)
            {
                ListarTablasCompletado(this, e);
            }
        }

        protected virtual void DispararLeerTablaCompletado(EventoLeerTablaCompletadoArgs e)
        {
            if (LeerTablaCompletado != null)
            {
                LeerTablaCompletado(this, e);
            }
        }

        protected virtual void DispararEscribirTablaCompletado(EventoEscribirTablaCompletadoArgs e)
        {
            if (EscribirTablaCompletado != null)
            {
                EscribirTablaCompletado(this, e);
            }
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoCrearUsuarioCompletadoArgs e)
        {
            if (CrearUsuarioCompletado != null)
            {
                CrearUsuarioCompletado(this, e);
            }
        }

        protected virtual void DispararConsultarCompletado(EventoConsultarCompletadoArgs e)
        {
            if (ConsultarCompletado != null)
            {
                ConsultarCompletado(this, e);
            }
        }

        #endregion
    }
}
