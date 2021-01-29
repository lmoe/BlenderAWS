#! /bin/bash
# A bash script, which is a poor man's substitute for the Python version
# but has the considerable advantage that it works

# Abort on errors
set -e

# Process the command line args. It would be nicer to use the longform
# rather than single letter arguments. TODO - work out how to do this.
while getopts "j:" option
do
 case "${option}"
 in
  j) RENDER_JOB=${OPTARG};;
 esac
done

fdisk -l || true
lsblk
ls /dev

# Mount the working drive to give us extra disk space
mkdir /mnt/workdrive
if [[ `lsblk | grep nvme0n1p128` ]]; then
    echo "Device found at /dev/nvme0n1p128, mounting work drive"
    mount /dev/nvme0n1p1 /mnt/workdrive
else
    echo "No work drive found"
fi
df -ah

# Source and destination file names
SOURCE_OBJECT=${RENDER_JOB}.zip
DEST_OBJECT="${RENDER_JOB}.Cache.zip"
DEST_OBJECT="$(echo -e "${DEST_OBJECT}" | tr -d '[:space:]')"

BLENDER=/usr/local/blender/blender
PYTHON_SCRIPT=/root/scripts/do_render.py

UNIQUE_KEY=`cat /proc/sys/kernel/random/uuid`
WORK_DIR=/mnt/workdrive/${UNIQUE_KEY}
mkdir ${WORK_DIR}
echo "Created work dir ${WORK_DIR}"

# Clean up the workdir whether successful or failed
trap 'echo "Deleting work directory"; cd ~; rm -Rf ${WORK_DIR}' EXIT

BLEND_DIR=${WORK_DIR}/blend
CACHE_DIR=${WORK_DIR}/cache
CACHE_DIR_TMP=${WORK_DIR}/cache_tmp

SOURCE_LOCAL_ZIP=${BLEND_DIR}/job.zip
OUTPUT_DIR=${WORK_DIR}/render_output

# Get the input ready to render
mkdir ${BLEND_DIR}
mkdir ${BLEND_DIR}/cache
mkdir ${BLEND_DIR}/cache_tmp
aws s3 cp "s3://${RENDER_SOURCE_BUCKET}/${SOURCE_OBJECT}" "${SOURCE_LOCAL_ZIP}"
unzip "${SOURCE_LOCAL_ZIP}" -d "${BLEND_DIR}"

BLEND_FILE=${BLEND_DIR}/job.blend

lscpu

echo "Running blender"

${BLENDER} -b --verbose 2 -noaudio "${BLEND_FILE}" --python ${PYTHON_SCRIPT} -- --blend ${UNIQUE_KEY}

cd ${CACHE_DIR}

pwd

zip -r0 "${CACHE_DIR}/${DEST_OBJECT}" . 
aws s3 cp "${CACHE_DIR}/${DEST_OBJECT}" "s3://${RENDER_DEST_BUCKET}/${DEST_OBJECT}"

echo "Done!"
