# Within Blender, run the specified command
import sys
import bpy
import argparse
import json
import re

def fastPrint(printString):
  sys.stdout.write(str(printString) + str('\n'))

def parse_args():
    my_args = sys.argv[sys.argv.index('--') + 1:]

    parser = argparse.ArgumentParser('Run Blender and render animation')

    parser.add_argument('-b', '--blend', required=False, type=str, default=None)
    
    return parser.parse_args(my_args)

def enable_cuda_devices():
    prefs = bpy.context.preferences

    cprefs = prefs.addons['cycles'].preferences
    cprefs.get_devices()

    # Attempt to set GPU device types if available
    for compute_device_type in ('CUDA', 'OPENCL', 'NONE'):
        try:
            cprefs.compute_device_type = compute_device_type
            fastPrint("Compute device selected: {0}".format(compute_device_type))
            break
        except TypeError:
            pass

    # Any CUDA/OPENCL devices?
    acceleratedTypes = ['CUDA', 'OPENCL']
    accelerated = any(device.type in acceleratedTypes for device in cprefs.devices)
    fastPrint('Accelerated render = {0}'.format(accelerated))

    # If we have CUDA/OPENCL devices, enable only them, otherwise enable
    # all devices (assumed to be CPU)
    fastPrint(cprefs.devices)
    for device in cprefs.devices:
        device.use = not accelerated or device.type in acceleratedTypes
        fastPrint('Device enabled ({type}) = {enabled}'.format(type=device.type, enabled=device.use))

    return accelerated

def tryBake(args):
    fastPrint("Baking scene. This takes some time.")

    for scene in bpy.data.scenes:
        for object in scene.objects:
            for modifier in object.modifiers:
                if modifier.type == 'FLUID':
                    if modifier.fluid_type == 'DOMAIN':
                        fastPrint(f"Baking: {object.name}", )
                        try:
                            name = re.sub('[^A-Za-z0-9]+', '', object.name)
                            modifier.domain_settings.cache_directory = f"/mnt/workdrive/{args.blend}/cache/{name}"
                            bpy.ops.fluid.bake_all({'scene': scene, 'active_object': object})
                            fastPrint("bake done!")
                        except Exception as ex:
                            fastPrint("Bake failed!")
                            fastPrint(ex)


def do_run_process(manifest):
    sys.stdout.flush()
    accelerated = enable_cuda_devices()

    scene = bpy.context.scene

    if 'render' in manifest:
      for e in manifest['render']:
        setattr(scene.render, e, manifest['render'][e])

    if 'cycles' in manifest:
      for e in manifest['cycles']:
        setattr(scene.cycles, e, manifest['cycles'][e])

    if 'eevee' in manifest:
      for e in manifest['eevee']:
        setattr(scene.eevee, e, manifest['eevee'][e])

    if 'scene' in manifest:
      for e in manifest['scene']:
        setattr(scene, e, manifest['scene'][e])

    scene.render.use_compositing = True
    scene.render.filepath = f"/mnt/workdrive/{args.blend}/render_output/"
    
    if 'bake' in manifest and manifest['bake'] is True:
      tryBake(args)                        

    fastPrint('Baking complete. Exiting')
    

fastPrint("Starting rendering process")
args = parse_args()
with open(f"/mnt/workdrive/{args.blend}/blend/manifest.json") as json_file:
    data = json.load(json_file)
    fastPrint("Manifest loaded with these overwrites:")
    fastPrint(json.dumps(data, indent=4, sort_keys=True))
    do_run_process(data)
