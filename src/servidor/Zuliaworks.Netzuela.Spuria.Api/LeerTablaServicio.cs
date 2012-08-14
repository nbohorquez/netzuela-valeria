namespace Zuliaworks.Netzuela.Spuria.Api
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Runtime.Serialization;
	
	using ServiceStack.FluentValidation;
	using ServiceStack.ServiceInterface;
	using ServiceStack.ServiceInterface.ServiceModel;		// ResponseStatus
	using Zuliaworks.Netzuela.Spuria.Tipos;
	using Zuliaworks.Netzuela.Valeria.Logica;
	
	public class LeerTablaValidador : AbstractValidator<LeerTabla>
	{		
		public LeerTablaValidador()
		{
			RuleFor(x => x.TiendaId).NotNull().GreaterThan(0).Must(Validadores.TiendaId).WithMessage(Validadores.ERROR_TIENDA_ID);
			RuleFor(x => x.BaseDeDatos).NotNull().NotEmpty().Must(Validadores.BaseDeDatos).WithMessage(Validadores.ERROR_BASE_DE_DATOS);
			RuleFor(x => x.Tabla).NotNull().NotEmpty().Must((x, tabla) => Validadores.Tabla(x.BaseDeDatos, tabla)).WithMessage(Validadores.ERROR_TABLA);
		}
	}

	public class LeerTablaServicio : ServiceBase<LeerTabla>
	{
		#region Funciones
		
		protected override object Run (LeerTabla request)
		{
			DataTableXml datosAEnviar = null;

            /*
             * Para convertir un LINQ en DataTable:
             * http://msdn.microsoft.com/en-us/library/bb386921.aspx
             * 
             * Para sacar un DataTable de un EF:
             * http://www.codeproject.com/Tips/171006/Convert-LINQ-to-Entity-Result-to-a-DataTable.aspx
             */
			
			Sesion.Usuario = int.Parse(this.GetSession().FirstName);
			LeerTablaValidador validador = new LeerTablaValidador();
			validador.ValidateAndThrow(request);
						
            try
            {
                using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
                {
                    conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);

                    string sql = "SELECT ";
                    Permisos.DescriptorDeTabla descriptor =
                        Permisos.EntidadesPermitidas[request.BaseDeDatos].First(e => string.Equals(e.Nombre, request.Tabla, StringComparison.OrdinalIgnoreCase));

                    for (int i = 0; i < descriptor.Columnas.Length; i++)
                    {
                        if (!string.Equals(descriptor.TiendaId, descriptor.Columnas[i], StringComparison.OrdinalIgnoreCase))
                        {
                            sql += descriptor.Columnas[i];

                            if ((i + 1) < descriptor.Columnas.Length)
                            {
                                sql += ", ";
                            }
                        }
                    }

                    sql += " FROM " + request.Tabla + " WHERE " + descriptor.TiendaId + " = " + request.TiendaId.ToString();

                    DataTable t = conexion.Consultar(Constantes.BaseDeDatos, sql);
                    List<DataColumn> cp = new List<DataColumn>();
					
					/* 
					 * Seleccionamos todas las columnas que sean clave primaria y que no sean iguales a 
					 * descriptor.TiendaID (esta columna no va a ser enviada al cliente). Hay que hacer 
					 * esto para que toda tabla enviada siempre tenga una clave primaria.
					 */
                    foreach (string columna in descriptor.ClavePrimaria)
                    {
                        if (!string.Equals(columna, descriptor.TiendaId, StringComparison.OrdinalIgnoreCase))
                        {
                            cp.Add(t.Columns[columna]);
                        }
                    }

                    t.PrimaryKey = cp.ToArray();
                    datosAEnviar = t.DataTableAXml(request.BaseDeDatos, request.Tabla);
	            }
            }
            catch (Exception ex)
            {
				//log.Fatal("Usuario: " + this.Cliente + ". Error de lectura de tabla: " + ex.Message);
                throw new Exception("Error de lectura de tabla", ex);
            }

            return new LeerTablaResponse { Tabla = datosAEnviar, ResponseStatus = new ResponseStatus() };
		}
		
		#endregion
		
	}
}