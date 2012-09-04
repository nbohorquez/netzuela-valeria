#!/bin/bash

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

	if [ ! -f /etc/apache2/mods-enabled/mod_mono.load ]
	then
		if [ ! -f /etc/apache2/mods-available/mod_mono.load ]
		then
			sh -c "$mod_mono_load"
		fi
		ln -s /etc/apache2/mods-available/mod_mono.load /etc/apache2/mods-enabled/mod_mono.load
	fi

	if [ ! -f /etc/apache2/mods-enabled/mod_mono.conf ]
	then
		if [ ! -f /etc/apache2/mods-available/mod_mono.conf ]
		then
			sh -c "$mod_mono_conf"
		fi
		ln -s /etc/apache2/mods-available/mod_mono.conf /etc/apache2/mods-enabled/mod_mono.conf
	fi
}

crear_archivo_apache() {
	if [ ! -f /etc/apache2/sites-available/valeria ]
	then
		sh -c "$apache_valeria"
	fi
	ln -s /etc/apache2/sites-available/valeria /etc/apache2/sites-enabled/valeria
	
	# Esta parte no deberia estar si estoy en Amazon
	sh -c "$valeria_puerto"
}

configurar_var_www() {
	ln -s `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api /var/www/valeria
}

# Chequeamos mono
[ "$(which mono)" ] || { instalar_mono; echo "mono instalado"; }

# Chequeamos mod_mono
if [ ! -f /usr/lib/apache2/modules/mod_mono.so ]
then
	instalar_mod_mono
	echo "mod_mono instalado"
fi

# Chequeamos el archivo de configuracion de apache
if [ ! -f /etc/apache2/sites-enabled/valeria ]
then
	crear_archivo_apache
	echo "Archivo de configuracion de apache creado"
fi

# Chequeamos el directorio /var/www/
if [ ! -L /var/www/valeria ]
then
	configurar_var_www
	echo "Directorio /var/www/ configurado"
fi

# Compilamos el proyecto
#xbuild `pwd`/../src/servidor/Servidor.sln

# Colocamos la contraseña de spuria en el web.config
# Reiniciamos apache para que los cambios tengan efecto
sudo service apache2 restart
