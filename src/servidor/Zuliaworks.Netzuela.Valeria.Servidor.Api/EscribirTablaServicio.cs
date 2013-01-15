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
    using ServiceStack.ServiceInterface.ServiceModel;        // ResponseStatus
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

                    using (DataTable tablaRecibida = request.TablaXml.XmlADataTable())
                    using (DataTable tablaProcesada = tablaRecibida.Copy())
                    {
                        Permisos.DescriptorDeTabla descriptor =
                            Permisos.EntidadesPermitidas[request.TablaXml.BaseDeDatos].First(e => string.Equals(e.Nombre, request.TablaXml.NombreTabla, StringComparison.OrdinalIgnoreCase));

                        // Si la tabla original poseia una columna tienda_id, debemos colocarsela de nuevo
                        if (descriptor && descriptor.TiendaId != null)
                        {
                            List<DataRow> filasEliminadas = new List<DataRow>();
                            DataColumn col = new DataColumn(descriptor.TiendaId, request.TiendaId.GetType());
                            tablaProcesada.Columns.Add(col);

                            for (int i = 0; i < tablaProcesada.Rows.Count; i++)
                            {
                                if (tablaProcesada.Rows[i].RowState == DataRowState.Deleted)
                                {
                                    // Hacemos que la fila borrada "vuelva a la vida", agregamos la nueva
                                    // columna PK y llevamos cuenta de cual fila era para luego borrarla nuevamente.
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
                        for (int i = 0; descriptor && i < descriptor.Columnas.Length; i++)
                        {
                            tablaProcesada.Columns[descriptor.Columnas[i]].SetOrdinal(i);
                        }

                        //conexion.EscribirTabla(request.TablaXml.BaseDeDatos, request.TablaXml.NombreTabla, tablaProcesada);
                        // Leemos la tabla actual sin modificaciones desde la base de datos
                        DataTable t = conexion.LeerTabla(request.TablaXml.BaseDeDatos, request.TablaXml.NombreTabla);
                        t.TableName = request.TablaXml.NombreTabla;

                        // Este paso es crucial. Por alguna razon no se pueden reforzar los "constraints"
                        // luego de hacer un "merge" con filas que han sido eliminadas (no hay problema
                        // con las filas modificadas o agregadas)
                        // http://social.msdn.microsoft.com/Forums/en-US/Vsexpressvb/thread/27aec612-5ca4-41ba-80d6-0204893fdcd1/
                        t.DataSet.EnforceConstraints = false;
                        // http://msdn.microsoft.com/es-es/library/wtk78t63%28v=vs.80%29.aspx
                        t.DataSet.Merge(tablaProcesada, false, MissingSchemaAction.Error);

                        DataViewRowState[] estados = new DataViewRowState[]
                        {
                            DataViewRowState.Deleted, DataViewRowState.ModifiedCurrent, DataViewRowState.Added
                        };

                        foreach (DataViewRowState estado in estados)
                        {
                            foreach (DataRow fila in t.Select(null, null, estado))
                            {
                                DataRowVersion version = DataRowVersion.Current;
                                if (fila.RowState == DataRowState.Deleted)
                                {
                                    version = DataRowVersion.Original;
                                }

                            }
                        }

                        MySqlRowUpdatingEventHandler actualizandoFila = (r, a) =>
                        {
                            List<string> parametros = new List<string>();
                            DataRowVersion version = DataRowVersion.Current;
        
                            if (a.StatementType == StatementType.Delete)
                            {
                                version = DataRowVersion.Original;
                            }
        
                            for (int i = 0; i < a.Row.Table.Columns.Count; i++)
                            {
                                parametros.Add(a.Row[i, version].ToString().Replace(",", "."));
                            }
        
                            a.Command.Parameters["a_Parametros"].Value = string.Join(",", parametros.ToArray());
                        };
                        
                        MySqlRowUpdatedEventHandler filaActualizada = (r, a) =>
                        {
                            // http://www.hesab.net/book/asp.net/Additional%20Documents/Resolving%20Conflicts%20on%20a%20Row-by-Row%20Basis.pdf
                            if (a.Errors != null)
                            {
                                switch (a.Status)
                                {
                                    case UpdateStatus.Continue:
                                        break;
                                    case UpdateStatus.ErrorsOccurred:
                                        string msj = "Tipo de error=" + a.Errors.GetType().ToString()
                                                + "\nFilas afectadas=" + a.RecordsAffected.ToString()
                                                + "\nFila tiene errores=" + a.Row.HasErrors.ToString()
                                                + "\nTipo de instruccion ejecutada=" + a.StatementType
                                                + "\nEstatus de la fila=" + a.Status.ToString()
                                                + "\nErrores=" + a.Row.RowError;
        
                                        for (int i = 0; i < a.Row.ItemArray.Length; i++)
                                        {
                                            msj += "\nValorActual[" + i.ToString() + "]=" + a.Row[i, DataRowVersion.Current];
                                        }
        
                                        foreach (System.Collections.DictionaryEntry de in a.Errors.Data)
                                        {
                                            msj += "\nClave=" + de.Key.ToString() + "Valor=" + de.Value.ToString();
                                        }
        
                                        throw new Exception(msj, a.Errors);
                                    case UpdateStatus.SkipAllRemainingRows:
                                        break;
                                    case UpdateStatus.SkipCurrentRow:
                                        break;
                                    default:
                                        throw new Exception("Estatus de fila desconocido", a.Errors);
                                }
                            }
                        };

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