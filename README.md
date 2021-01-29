# BlenderAWS

## Introduction

Based on the AwsBatchBlender project from Peter Reay (https://github.com/petetaxi-test/AwsBatchBlender)

Contribute to global warming by leveraging out your Blender renders to cheap AWS Batch machines that do the calculation for you.

My fork makes use of two different AWS machines: one GPU instance which has a lower CPU count, one with a high cpu count.

The reason for that, is that baking mantaflow smoke/water and particles are mostly CPU bound and GPU instances are expensive (and way too slow for that!)

The main idea is to create a process that is driven by packages. Those packages contain a blend file, and a manifest file.
The manifest file can overwrite blender properties (https://docs.blender.org/api/current/bpy.ops.render.html)

The management application was rewritten in .NET Core and the Batch structure was changed to read a JSON string instead of using batch arguments. This makes it easier to configure and overwrite each render/bake.

Currently I can't really offer a readme. Take a look into the source code until then.

## Usage

Soon.

The management application is a CLI tool with a manual