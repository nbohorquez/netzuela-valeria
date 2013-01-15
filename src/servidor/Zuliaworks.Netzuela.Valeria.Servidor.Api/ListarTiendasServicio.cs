namespace Zuliaworks.Netzuela.Valeria.Servidor.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;
    
    using ServiceStack.ServiceInterface;
    using ServiceStack.ServiceInterface.ServiceModel;        // ResponseStatus
    using Zuliaworks.Netzuela.Valeria.Datos;
    using Zuliaworks.Netzuela.Valeria.Tipos;
    
    public class ListarTiendasServicio : ServiceBase<ListarTiendas>
    {
        #region Implementacion de interfaces
        
        protected override object Run (ListarTiendas request)
        {
            int usuario = int.Parse(this.GetSession().FirstName);
            List<string> resultado = new List<string>();
            
            try
            {
                using (Conexion conexion = new Conexion(Sesion.CadenaDeConexion))
                {
                    conexion.Conectar(Sesion.Credenciales[0], Sesion.Credenciales[1]);
                    
                    string sql = "SELECT t.tienda_id, c.nombre_legal "
                                + "FROM tienda AS t "
                                + "JOIN cliente AS c ON c.rif = t.cliente_p "
                                + "JOIN usuario AS u ON u.usuario_id = c.propietario_id "
                                + "WHERE u.usuario_id = " + usuario.ToString();
                    DataTable t = conexion.Consultar(Constantes.BaseDeDatos, sql);
                    
                    foreach(DataRow r in t.Rows)
                    {
                        resultado.Add(r[0].ToString() + ":" + r[1].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                //log.Fatal("Usuario: " + sesion.UserName + ". Error de listado de base de datos: " + ex.Message);
                throw new Exception("Error de listado de base de datos", ex);
            }

            return new ListarTiendasResponse { Tiendas = resultado.ToArray(), ResponseStatus = new ResponseStatus() };
        }
        
        #endregion
    }
}