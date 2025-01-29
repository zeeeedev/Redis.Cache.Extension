#!/bin/bash
dotnet pack ./Redis.Cache.Extension -c Release -o ./
dotnet nuget push *.nupkg -s local-nuget-feed
rm *.nupkg
# Clear all local nuget cache
dotnet nuget locals all -c
