#!/bin/bash
version=$1

if [ "$version" == "" ];then
   echo "version should be provided!"
   exit;
fi

dotnet nuget push "nuget/Yozian.DependencyInjectionPlus.$version.nupkg" --source https://api.nuget.org/v3/index.json --api-key $NUGET_API_KEY

nuget push Yozian.DependencyInjectionPlus.$1.nupkg  -source https://api.nuget.org/v3/index.json

cd ..
