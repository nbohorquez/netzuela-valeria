namespace Zuliaworks.Netzuela.Valeria.Servidor.Api
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	
	using log4net;
	using ServiceStack.FluentValidation;
	using ServiceStack.ServiceInterface;
	using ServiceStack.ServiceInterface.ServiceModel;		// ResponseStatus
	using Zuliaworks.Netzuela.Valeria.Comunes;
	using Zuliaworks.Netzuela.Valeria.Datos;
	using Zuliaworks.Netzuela.Valeria.Tipos;
		
	public class EscribirTablaValidador : AbstractValidator<EscribirTabla>
	{		
		public EscribirTablaValidador(int usuarioId)
		{
			RuleFor(x => x.TiendaId).NotNull().GreaterThan(0).Must((x, tiendaId) => Validadores.TiendaId(tiendaId, usuarioId)).WithMessage(Validadores.ERROR_TIENDA_ID);
			RuleFor(x => x.TablaXml).NotNull().SetValidator(new DataTableXmlValidador());
		}
	}
	
	public class DataTableXmlValidador : AbstractValidator<DataTableXml>
	{		
		public DataTableXmlValidador()
		{
			RuleFor(x => x.BaseDeDatos).NotNull().NotEmpty().Must(Validadores.BaseDeDatos).WithMessage(Validadores.ERROR_BASE_DE_DATOS);
			RuleFor(x => x.NombreTabla).NotNull().NotEmpty().Must((x, tabla) => Validadores.Tabla(x.BaseDeDatos, tabla)).WithMessage(Validadores.ERROR_TABLA);
			RuleFor(x => x.EsquemaXml).NotNull().NotEmpty();
			RuleFor(x => x.Xml).NotNull().NotEmpty();			
		}
	}

	public class EscribirTablaServicio : ServiceBase<EscribirTabla>
	{
		private readonly ILog log = LogManager.GetLogger(typeof(EscribirTablaServicio));
		protected override object Run (EscribirTabla request)
		{
			bool resultado = false;
			
			int usuario = int.Parse(this.GetSession().FirstName);
			EscribirTablaValidador validador = new EscribirTablaValidador(usuario);
			validador.ValidateAndThrow(request);

            try
            {
                using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
                {
                    conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);

                    Permisos.DescriptorDeTabla descriptor =
                        Permisos.EntidadesPermitidas[request.TablaXml.BaseDeDatos].First(e => string.Equals(e.Nombre, request.TablaXml.NombreTabla, StringComparison.OrdinalIgnoreCase));
					
                    using (DataTable tablaRecibida = request.TablaXml.XmlADataTable())
					using (DataTable tablaProcesada = tablaRecibida.Copy())
					{					
						// Si la tabla original poseia una columna tienda_id, debemos colocarsela de nuevo
						if (descriptor.TiendaId != null)
						{
							List<DataRow> filasEliminadas = new List<DataRow>();
		                    DataColumn col = new DataColumn(descriptor.TiendaId, request.TiendaId.GetType());
		                    tablaProcesada.Columns.Add(col);
							
							for (int i = 0; i < tablaProcesada.Rows.Count; i++)
							{
								if (tablaProcesada.Rows[i].RowState == DataRowState.Deleted)
								{
									// Hacemos que las columnas borradas "vuelvan a la vida"
									// pero llevamos cuenta de cuales son para luego borrarlas nuevamente.
									tablaProcesada.Rows[i].RejectChanges();
									tablaProcesada.Rows[i][col] = request.TiendaId;
									filasEliminadas.Add(tablaProcesada.Rows[i]);
									tablaProcesada.Rows[i].AcceptChanges();
								}
								else
								{
									tablaProcesada.Rows[i][col] = request.TiendaId;
								}
							}
							
							// Agregamos tienda_id al conjunto de claves primarias
							List<DataColumn> cp = new List<DataColumn>();
							
							foreach (string pk in descriptor.ClavePrimaria)
							{
								cp.Add(tablaProcesada.Columns[pk]);
							}
							
							tablaProcesada.PrimaryKey = cp.ToArray();
							filasEliminadas.ToList().ForEach(f => f.Delete());
						}
							
						// Reordenamos las columnas
		                for (int i = 0; i < descriptor.Columnas.Length; i++)
		                {
		                    tablaProcesada.Columns[descriptor.Columnas[i]].SetOrdinal(i);
		                }
						
						conexion.EscribirTabla(request.TablaXml.BaseDeDatos, request.TablaXml.NombreTabla, tablaProcesada);
						DataTable nuevos = tablaProcesada.GetChanges(DataRowState.Added);
						if (nuevos != null)
						{
							// Ahora le ponemos tarea al "worker" que registra productos nuevos en la base de datos
							Celery.RegistrarProducto(nuevos);
							nuevos.Dispose();
							nuevos = null;
						}
					}
					
					resultado = true;
                }
            }
            catch (Exception ex)
            {
				log.Fatal("Usuario: " + usuario.ToString() + ". Error de escritura de tabla: " + ex.MostrarPilaDeExcepciones());
                throw new Exception("Error de escritura de tabla", ex);
            }

            return new EscribirTablaResponse { Exito = resultado, ResponseStatus = new ResponseStatus() };
		}
	}
}