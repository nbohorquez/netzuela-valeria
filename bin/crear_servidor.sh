#!/bin/bash

source comunes.sh
parse_config $archivo_config

dir_log="/var/log/netzuela"
dir_simbolico_www="/var/www/valeria"
valeria_enabled="/etc/apache2/sites-enabled/valeria"
valeria_available="/etc/apache2/sites-available/valeria"
mod_mono_so="/usr/lib/apache2/modules/mod_mono.so"
mod_mono_conf_available="/etc/apache2/mods-available/mod_mono.conf"
mod_mono_conf_enabled="/etc/apache2/mods-enabled/mod_mono.conf"
mod_mono_load_available="/etc/apache2/mods-available/mod_mono.load"
mod_mono_load_enabled="/etc/apache2/mods-enabled/mod_mono.load"
mod_mono_load="echo LoadModule mono_module $mod_mono_so >> $mod_mono_load_available"
mod_mono_conf="cat > $mod_mono_conf_available << EOF
AddType application/x-asp-net .aspx .asmx .ashx .asax .ascx .soap .rem .axd .cs .config .dll .svc
DirectoryIndex index.aspx
Include /etc/mono-server4/mono-server4-hosts.conf
EOF
"
apache_valeria="cat > $valeria_available << EOF
<VirtualHost *:80>
    ServerName api.netzuela.com
    ServerAdmin tca7410nb@gmail.com

    MonoAutoApplication disabled
    MonoServerPath valeria /usr/bin/mod-mono-server4
        
    Alias / \"$dir_simbolico_www\"
    MonoApplications valeria \"/:$dir_simbolico_www\"
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
        
    # ErrorLog \${APACHE_LOG_DIR}/valeria_error.log
    ErrorLog $dir_log/valeria_error.log
    LogLevel debug
    # CustomLog \${APACHE_LOG_DIR}/valeria_access.log combined
    CustomLog $dir_log/valeria_access.log combined
</VirtualHost>
EOF
"

instalar_mono() {
    apt-get install -y mono-complete
}

instalar_mod_mono() {
    apt-get install -y libapache2-mod-mono mono-apache-server4 mono-xsp4
}

crear_archivo_mod_mono_load() {
    if [ ! -f "$mod_mono_load_available" ]; then
        bash -c "$mod_mono_load"
    fi
    ln -s "$mod_mono_load_available" "$mod_mono_load_enabled" 
}

crear_archivo_mod_mono_conf() {
    if [ ! -f "$mod_mono_conf_available" ]; then
        bash -c "$mod_mono_conf"
    fi
    ln -s "$mod_mono_conf_available" "$mod_mono_conf_enabled"
}

crear_archivo_apache() {
    if [ ! -f "$valeria_available" ]; then
        bash -c "$apache_valeria"
    fi
    ln -s "$valeria_available" "$valeria_enabled" 
}

configurar_var_www() {
    ln -s `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api "$dir_simbolico_www"
}

guardar_credenciales() {
    solucion=`readlink -f "$(pwd)/../src/criptografia/Criptografia.sln"`
    ejecutable=`readlink -f "$(pwd)/../src/criptografia/Criptografia/bin/Debug/Criptografia.exe"`
    web_config=`readlink -f "$(pwd)/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/Web.config"`
    xbuild $solucion
    mono "$ejecutable" "$1" "'"$2"'" "$web_config"
    #chown -R "$owner":"$owner" `pwd`/../src/criptografia/Criptografia/bin/ `pwd`/../src/criptografia/Criptografia/obj
}

compilar_servidor() {
    solucion=`readlink -f "$(pwd)/../src/servidor/Servidor.sln"`
    xbuild "$solucion"
    #chown -R "$owner":"$owner" `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/bin `pwd`/../src/servidor/Zuliaworks.Netzuela.Valeria.Servidor.Api/obj
    #chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Comunes/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Comunes/obj
    #chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Datos/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Datos/obj
    #chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Preferencias/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Preferencias/obj
    #chown -R "$owner":"$owner" `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Tipos/bin `pwd`/../src/comunes/Zuliaworks.Netzuela.Valeria.Tipos/obj
}

# Chequeamos root
if [ "$USER" != "root" ]; then
    echo "Error: Debe correr este script como root"
    exit 1;
fi
echo "Ejecutando script como root"

# Chequeamos mono
command -v mono >/dev/null 2>&1 || { 
    echo "mono no esta instalado, instalando..."
    instalar_mono
}
echo "mono instalado"

# Chequeamos mod_mono
if [ ! -f "$mod_mono_so" ]; then
    echo "mod_mono no esta instalado, instalando..."
    instalar_mod_mono
fi
echo "mod_mono instalado"

# Chequeamos mod_mono.load
if [ ! -f "$mod_mono_load_enabled" ]; then
    echo "mod_mono.load no existe, instalando..."
    crear_archivo_mod_mono_load
fi
echo "mod_mono.load creado"

# Chequeamos mod_mono.conf
if [ ! -f "$mod_mono_conf_enabled" ]; then
    echo "mod_mono.conf no existe, instalando..."
    crear_archivo_mod_mono_conf
fi
echo "mod_mono.conf creado"

# Chequeamos el archivo de configuracion de apache
if [ ! -f "$valeria_enabled" ]; then
    echo "El archivo de configuracion valeria no existe, creandolo..."
    crear_archivo_apache
fi
echo "valeria creado"

# Chequeamos el directorio /var/www/
if [ ! -L "$dir_simbolico_www" ]; then
    echo "Directorio /var/www/ no configurado, trabajando..."
    configurar_var_www
fi
echo "Directorio /var/www/ configurado"

# Colocamos la contraseña de spuria en el web.config
guardar_credenciales "$usuario" "$contrasena"
echo "Credenciales cargados"

# Compilamos el proyecto
compilar_servidor
echo "Servidor compilado"

# Reiniciamos apache para que los cambios tengan efecto
sudo service apache2 restart
exit 0
