include .env
export
generate_vdf:
	envsubst < template.vdf > workshop.vdf

upload_workshop: generate_vdf
	steamcmd +login ${AUTHOR} +workshop_build_item ${PWD}/workshop.vdf +quit
