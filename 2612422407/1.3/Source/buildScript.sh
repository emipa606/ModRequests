#!/bin/bash

projectName=$1
csprojFile="${projectName}.csproj"

if [ -f "$csprojFile" ]; then
    FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.7.2-api/ dotnet build $csprojFile /property:Configuration=Release
    rm -r obj
else
    echo "${projectName}.csproj not found"
fi

