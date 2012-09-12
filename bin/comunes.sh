#!/bin/bash

archivo_config="config.ini"

parse_config() {
	if [ ! -f "$1" ]; then
		echo "$1 no existe"
		return 
	fi

	# Hay muchas formas de leer un config file sin usar source:
	# http://stackoverflow.com/questions/4434797/read-a-config-file-in-bash-without-using-source
	while read linea; do
	if [[ "$linea" =~ ^[^#]*= ]]; then
		variable=`echo $linea | cut -d'=' -f 1 | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'`
		#variable=`echo $linea | cut -d'=' -f 1 | tr -d ' '`
		valor=`echo $linea | cut -d'=' -f 2- | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//'`
		#valor=`echo $linea | cut -d'=' -f 2- | tr -d ' '`
		eval "$variable"="$valor"
	fi
	done < "$1"
}
