namespace Criptografia {
	using System;
	using System.Configuration;
	using System.IO;
	using Zuliaworks.Netzuela.Valeria.Comunes;
	using Zuliaworks.Netzuela.Valeria.Preferencias;
	
	class MainClass {
		public static void Main (string[] args)
		{
			if (args.Length != 3) {
				System.Console.Write("\nUso correcto: mono Criptografia.exe <usuario> <contraseña> <ruta archivo de configuracion>\n");
				return;
			}
			
			try {
				string usuario = args[0];
				string contrasena = args[1];
				string rutaArchivoConfig = args[2];
				string llave = string.Format("{0}:{1}", usuario, contrasena).ConvertirASecureString().Encriptar();
				string ruta = Path.GetFullPath(rutaArchivoConfig);
				
				Console.Write("llave: " + llave + "\nruta: " + ruta + "\n");
				
				ColeccionElementosGenerica<UsuarioContrasenaElement> coleccionDeLlaves =
					new ColeccionElementosGenerica<UsuarioContrasenaElement>();
				
				UsuarioContrasenaElement llaveLocal = new UsuarioContrasenaElement() { 
					Id = "Local",
					Llave = llave
				};
	            
				coleccionDeLlaves.Add(llaveLocal);
				
				ExeConfigurationFileMap mapa = new ExeConfigurationFileMap();
				mapa.ExeConfigFilename = ruta;
				Configuration archivoConfig = ConfigurationManager.OpenMappedExeConfiguration(mapa, ConfigurationUserLevel.None);
				Console.Write(archivoConfig.FilePath + "\n");
				CargarGuardar.GuardarCredenciales(archivoConfig, coleccionDeLlaves);
			} catch(Exception ex) {
				Console.Write(ex.MostrarPilaDeExcepciones());
			}
		}
	}
}

	