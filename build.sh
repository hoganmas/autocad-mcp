#! /bin/bash

cd Plugin
./buildPlugin.sh

cd ..

cd Server
uv run server.py
