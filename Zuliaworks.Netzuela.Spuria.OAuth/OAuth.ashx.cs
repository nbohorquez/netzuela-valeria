namespace Zuliaworks.Netzuela.Spuria.ServidorOAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.SessionState;                  // IRequiresSessionState

    using DotNetOpenAuth.Messaging;                 // MessageReceivingEndpoint
    using DotNetOpenAuth.OAuth;                     // ServiceProvider
    using DotNetOpenAuth.OAuth.ChannelElements;     // HmacSha1SigningBindingElement
    using DotNetOpenAuth.OAuth.Messages;            // UserAuthorizationRequest, AuthorizedTokenRequest    

    /// <summary>
    /// Descripción breve de OAuth
    /// </summary>
    public class OAuth : IHttpHandler, IRequiresSessionState
    {
        #region Variables
        
        private readonly Uri urlRaiz;
        private readonly ServiceProviderDescription autoDescripcion;
        private const string PeticionDeAutorizacionPendienteDeSesion = "PeticionDeAutorizacionPendiente";
        private ServiceProvider proveedor;

        #endregion

        #region Constructores

        public OAuth()
        {
            string stringRaiz = HttpContext.Current.Request.ApplicationPath;
            if (!stringRaiz.EndsWith("/", StringComparison.Ordinal))
            {
                stringRaiz += "/";
            }

            this.urlRaiz = new Uri(HttpContext.Current.Request.Url, stringRaiz);

            this.autoDescripcion = new ServiceProviderDescription
            {
                AccessTokenEndpoint = new MessageReceivingEndpoint(new Uri(this.urlRaiz, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
                RequestTokenEndpoint = new MessageReceivingEndpoint(new Uri(this.urlRaiz, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
                UserAuthorizationEndpoint = new MessageReceivingEndpoint(new Uri(this.urlRaiz, "/OAuth.ashx"), HttpDeliveryMethods.PostRequest),
                TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() }
            };

            this.proveedor = new ServiceProvider(this.autoDescripcion, new AdministradorDeTokens());
        }

        #endregion

        #region Propiedades

        public static UserAuthorizationRequest PeticionDeAutorizacionPendiente
        {
            get { return HttpContext.Current.Session[PeticionDeAutorizacionPendienteDeSesion] as UserAuthorizationRequest; }
            set { HttpContext.Current.Session[PeticionDeAutorizacionPendienteDeSesion] = value; }
        }

        public bool IsReusable
        {
            get { return true; }
        }        

        #endregion

        #region Funciones

        public void ProcessRequest(HttpContext context)
        {
            UnauthorizedTokenRequest peticionDeToken;
            UserAuthorizationRequest peticionDeAutorizacion;
            AuthorizedTokenRequest peticionDeTokenDeAcceso;

            try
            {
                IProtocolMessage peticion = this.proveedor.ReadRequest();

                if ((peticionDeToken = peticion as UnauthorizedTokenRequest) != null)
                {
                    var respuesta = this.proveedor.PrepareUnauthorizedTokenMessage(peticionDeToken);
                    this.proveedor.Channel.Send(respuesta);
                }
                else if ((peticionDeAutorizacion = peticion as UserAuthorizationRequest) != null)
                {
                    PeticionDeAutorizacionPendiente = peticionDeAutorizacion;
                    HttpContext.Current.Response.Redirect("~/Autentificacion/Autorizar");
                }
                else if ((peticionDeTokenDeAcceso = peticion as AuthorizedTokenRequest) != null)
                {
                    var respuesta = this.proveedor.PrepareAccessTokenMessage(peticionDeTokenDeAcceso);
                    this.proveedor.Channel.Send(respuesta);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al procesar la petición", ex);
            }
        }

        #endregion
    }
}