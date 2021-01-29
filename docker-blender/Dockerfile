FROM elmorino/blenderbase:latest

LABEL authors="Lmoe"

# Blender scripts for configuring and rendering
ADD ./scripts/* /root/scripts/

# Invoke Blender. The 'command' arguments are passed through
# to this script when docker is invoked
ENTRYPOINT ["/bin/bash", "/root/scripts/process_job.sh"]
