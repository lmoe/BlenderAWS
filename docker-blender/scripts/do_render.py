# Within Blender, run the specified command
import sys
import bpy
import argparse
import json
import re

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
            print("Compute device selected: {0}".format(compute_device_type))
            break
        except TypeError:
            pass

    # Any CUDA/OPENCL devices?
    acceleratedTypes = ['CUDA', 'OPENCL']
    accelerated = any(device.type in acceleratedTypes for device in cprefs.devices)
    print('Accelerated render = {0}'.format(accelerated))

    # If we have CUDA/OPENCL devices, enable only them, otherwise enable
    # all devices (assumed to be CPU)
    print(cprefs.devices)
    for device in cprefs.devices:
        device.use = not accelerated or device.type in acceleratedTypes
        print('Device enabled ({type}) = {enabled}'.format(type=device.type, enabled=device.use))

    return accelerated

def setBakeFolders(args):
  print("Set baking folders.")
  sys.stdout.flush()

  for scene in bpy.data.scenes:
      for object in scene.objects:
          for modifier in object.modifiers:
              print(object.name, " - modifier type: ", modifier.type)
              if modifier.type == 'FLUID':
                  print(object.name, " - fuid_type: ", modifier.fluid_type)
                  if modifier.fluid_type == 'DOMAIN':
                      print("Bake All: ", object.name)
                      try:
                          name = re.sub('[^A-Za-z0-9]+', '', object.name)
                          modifier.domain_settings.cache_directory = f"/mnt/workdrive/{args.blend}/cache/{name}"
                          
                          print(f"/mnt/workdrive/{args.blend}/cache/{name}")
                      except Exception as ex:
                          print("Setting folder failed!")
                          print(ex)


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
      setBakeFolders(args)                        

    sys.stdout.flush()
    bpy.ops.render.render(animation=True)

print("Starting rendering process")
args = parse_args()
with open(f"/mnt/workdrive/{args.blend}/blend/manifest.json") as json_file:
    data = json.load(json_file)
    print("Manifest loaded with these overwrites:")
    print(json.dumps(data, indent=4, sort_keys=True))
    do_run_process(data)