namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	
	using ServiceStack.FluentValidation;
	using ServiceStack.ServiceInterface;
	using ServiceStack.ServiceInterface.ServiceModel;		// ResponseStatus
	using Zuliaworks.Netzuela.Spuria.Tipos;
	using Zuliaworks.Netzuela.Valeria.Logica;
	
	public class ListarTablasValidador : AbstractValidator<ListarTablas> 
	{		
		public ListarTablasValidador()
		{
			RuleFor(x => x.BaseDeDatos).NotNull().NotEmpty().Must(Validadores.BaseDeDatos).WithMessage(Validadores.ERROR_BASE_DE_DATOS);
		}
	}

	public class ListarTablasServicio : ServiceBase<ListarTablas>
	{
		#region Funciones
		
		protected override object Run (ListarTablas request)
		{
			Sesion.Usuario = int.Parse(this.GetSession().FirstName);
			ListarTablasValidador validador = new ListarTablasValidador();
			validador.ValidateAndThrow(request);
			
			List<string> resultado = new List<string>();

			try
            {
                using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
                {
                    conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);
                    string[] tablas = conexion.ListarTablas(request.BaseDeDatos);

                    var tablasAMostrar = (from tabla in tablas
                                          where Permisos.EntidadesPermitidas[request.BaseDeDatos].Any(t => string.Equals(t.Nombre, tabla, StringComparison.OrdinalIgnoreCase))
                                          select tabla).ToList();

                    resultado = tablasAMostrar;
                }
            }
            catch (Exception ex)
            {
				//log.Fatal("Usuario: " + this.Cliente + ". Error de listado de base de tablas: " + ex.Message);
                throw new Exception("Error de listado de tablas", ex);
            }

            return new ListarTablasResponse { Tablas = resultado.ToArray(), ResponseStatus = new ResponseStatus() };
		}
		
		#endregion
	}
}