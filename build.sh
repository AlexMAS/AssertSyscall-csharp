#!/bin/bash

cd $(dirname "$0")

rm -rf bin/publish/ 2> /dev/null
mkdir -p bin/publish/

docker build --file build.Dockerfile -t assert-syscall-build:latest .
id=$(docker create assert-syscall-build:latest)
docker cp $id:/src/bin/. bin/publish/
docker rm -v $id

cd - > /dev/null
