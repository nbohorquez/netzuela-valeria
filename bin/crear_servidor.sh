#!/bin/bash

owner="gustavo"

mod_mono_load="echo LoadModule mono_module /usr/lib/apache2/modules/mod_mono.so >> /etc/apache2/mods-available/mod_mono.load"

mod_mono_conf="cat > /etc/apache2/mods-available/mod_mono.conf << EOF
AddType application/x-asp-net .aspx .asmx .ashx .asax .ascx .soap .rem .axd .cs .config .dll .svc
DirectoryIndex index.aspx
Include /etc/mono-server4/mono-server4-hosts.conf
EOF
"

apache_valeria="cat > /etc/apache2/sites-available/valeria << EOF
<VirtualHost *:8080>
	ServerName api.netzuela.com
	ServerAdmin tca7410nb@gmail.com

	MonoAutoApplication disabled
    	MonoServerPath valeria /usr/bin/mod-mono-server4
	
    	Alias / "/var/www/valeria"
    	MonoApplications valeria "/:/var/www/valeria"
    	<Location / >
        	SetHandler mono
        	MonoSetServerAlias valeria
        	Order allow,deny
        	Allow from all
    	</Location>
	
   	<Location /mono>
        	SetHandler mono-ctrl
        	Order deny,allow
        	Deny from all
        	Allow from 127.0.0.1
    	</Location>
	
    	# ErrorLog ${APACHE_LOG_DIR}/valeria_error.log
	ErrorLog /var/log/netzuela/valeria_error.log
    	LogLevel debug
    	# CustomLog ${APACHE_LOG_DIR}/valeria_access.log combined
	CustomLog /var/log/netzuela/valeria_access.log combined
</VirtualHost>
EOF
"

valeria_puerto="cat >> /etc/apache2/ports.conf << EOF
NameVirtualHost *:8080
Listen 8080
EOF
"

instalar_mono() {
	apt-get install mono-complete
}

instalar_mod_mono() {
	apt-get install libapache2-mod-mono mono-apache-server4 mono-xsp4
}

crear_archivo_mod_mono_load() {
	if [ ! -f /etc/apache2/mods-available/mod_mono.load ]; then
		sh -c "$mod_mono_load"
	fi
	ln -s /etc/apache2/mods-available/mod_mono.load /etc/apache2/mods-enabled/mod_mono.load
}

crear_archivo_mod_mono_conf() {
	if [ ! -f /etc/apache2/mods-available/mod_mono.conf ]; then
		sh -c "$mod_mono_conf"
	fi
	ln -s /etc/apache2/mods-available/mod_mono.conf /etc/apache2/mods-enabled/mod_mono.conf
}

crear_archivo_apache() {
	if [ ! -f /etc/apache2/sites-available/valeria ]; then
		sh -c "$apache_valeria"
	fi
	ln -s /etc/apache2/sites-available/valeria /etc/apache2/sites-enabled/valeria
	
	# Esta parte no deberia estar si estoy en Amazon
	if ! grep -Fxq "Listen 8080" /etc/apache2/ports.conf; then
		sh -c "$valeria_puerto"
	fi
}

configurar_var_www() {
	ln -s `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api /var/www/valeria
}

cargar_credenciales() {
	xbuild `pwd`/../src/criptografia/Criptografia.sln
	mono `pwd`/../src/criptografia/Criptografia/bin/Debug/Criptografia.exe chivo '#HK_@20MamA!pAPa13?#3864' `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/Web.config
	chown -R "$owner":"$owner" `pwd`/../src/criptografia/Criptografia/bin/ `pwd`/../src/criptografia/Criptografia/obj
}

compilar_servidor() {
	xbuild `pwd`/../src/servidor/Servidor.sln
	chown -R "$owner":"$owner" `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/bin `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/obj
	chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Comunes/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Comunes/obj
	chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Datos/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Datos/obj
	chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Preferencias/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Preferencias/obj
	chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Tipos/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Tipos/obj
}

# Chequeamos root
if [ "$USER" != "root" ]; then
        echo "Error: Debe correr este script como root"
        exit 1;
fi
echo "Ejecutando script como root"

# Chequeamos mono
[ "$(which mono)" ] || { echo "mono no esta instalado, instalando..."; instalar_mono; }
echo "mono instalado"

# Chequeamos mod_mono
if [ ! -f /usr/lib/apache2/modules/mod_mono.so ]; then
	echo "mod_mono no esta instalado, instalando..."
	instalar_mod_mono
fi
echo "mod_mono instalado"

# Chequeamos mod_mono.load
if [ ! -f /etc/apache2/mods-enabled/mod_mono.load ]; then
	echo "mod_mono.load no existe, instalando..."
	crear_archivo_mod_mono_load
fi
echo "mod_mono.load creado"

# Chequeamos mod_mono.conf
if [ ! -f /etc/apache2/mods-enabled/mod_mono.conf ]; then
	echo "mod_mono.conf no existe, instalando..."
	crear_archivo_mod_mono_conf
fi
echo "mod_mono.conf creado"

# Chequeamos el archivo de configuracion de apache
if [ ! -f /etc/apache2/sites-enabled/valeria ]; then
	echo "El archivo de configuracion valeria no existe, creandolo..."
	crear_archivo_apache
fi
echo "valeria creado"

# Chequeamos el directorio /var/www/
if [ ! -L /var/www/valeria ]; then
	echo "Directorio /var/www/ no configurado, trabajando..."
	configurar_var_www
fi
echo "Directorio /var/www/ configurado"

# Colocamos la contraseña de spuria en el web.config
cargar_credenciales
echo "Credenciales cargados"

# Compilamos el proyecto
compilar_servidor
echo "Servidor compilado"

# Reiniciamos apache para que los cambios tengan efecto
sudo service apache2 restart
