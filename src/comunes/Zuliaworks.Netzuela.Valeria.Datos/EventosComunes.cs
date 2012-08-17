﻿namespace Zuliaworks.Netzuela.Valeria.Datos
{
    using System;
    using System.Collections.Generic;
    using System.Data;                      // StateChangeEventHandler
    using System.Linq;
    using System.Text;

    using Zuliaworks.Netzuela.Valeria.Datos.Eventos;

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

        protected void ManejarCambioDeEstado(object remitente, StateChangeEventArgs args)
        {
            this.DispararCambioDeEstado(args);
        }

        protected void ManejarListarBasesDeDatosCompletado(object remitente, EventoListarBDsCompletadoArgs args)
        {
            this.DispararListarBasesDeDatosCompletado(args);
        }

        protected void ManejarListarTablasCompletado(object remitente, EventoListarTablasCompletadoArgs args)
        {
            this.DispararListarTablasCompletado(args);
        }

        protected void ManejarLeerTablaCompletado(object remitente, EventoLeerTablaCompletadoArgs args)
        {
            this.DispararLeerTablaCompletado(args);
        }

        protected void ManejarEscribirTablaCompletado(object remitente, EventoEscribirTablaCompletadoArgs args)
        {
            this.DispararEscribirTablaCompletado(args);
        }

        protected void ManejarCrearUsuarioCompletado(object remitente, EventoCrearUsuarioCompletadoArgs args)
        {
            this.DispararCrearUsuarioCompletado(args);
        }

        protected void ManejarConsultarCompletado(object remitente, EventoConsultarCompletadoArgs args)
        {
            this.DispararConsultarCompletado(args);
        }

        protected virtual void DispararCambioDeEstado(StateChangeEventArgs e)
        {
            if (this.CambioDeEstado != null)
            {
                this.CambioDeEstado(this, e);
            }
        }

        protected virtual void DispararListarBasesDeDatosCompletado(EventoListarBDsCompletadoArgs e)
        {
            if (this.ListarBasesDeDatosCompletado != null)
            {
                this.ListarBasesDeDatosCompletado(this, e);
            }
        }

        protected virtual void DispararListarTablasCompletado(EventoListarTablasCompletadoArgs e)
        {
            if (this.ListarTablasCompletado != null)
            {
                this.ListarTablasCompletado(this, e);
            }
        }

        protected virtual void DispararLeerTablaCompletado(EventoLeerTablaCompletadoArgs e)
        {
            if (this.LeerTablaCompletado != null)
            {
                this.LeerTablaCompletado(this, e);
            }
        }

        protected virtual void DispararEscribirTablaCompletado(EventoEscribirTablaCompletadoArgs e)
        {
            if (this.EscribirTablaCompletado != null)
            {
                this.EscribirTablaCompletado(this, e);
            }
        }

        protected virtual void DispararCrearUsuarioCompletado(EventoCrearUsuarioCompletadoArgs e)
        {
            if (this.CrearUsuarioCompletado != null)
            {
                this.CrearUsuarioCompletado(this, e);
            }
        }

        protected virtual void DispararConsultarCompletado(EventoConsultarCompletadoArgs e)
        {
            if (this.ConsultarCompletado != null)
            {
                this.ConsultarCompletado(this, e);
            }
        }

        #endregion
    }
}