#!/bin/bash
version=$1

if [ "$version" == "" ];then
   echo "version should be provided!"
   exit;
fi

commit=`git rev-parse --short HEAD`

echo "pack with commit: $commit"

# change commit hash

sed -i -e "s/commit=\"*\"/commit=\"$commit\"/g" nuget/Yozian.DependencyInjectionPlus.nuspec

# move old package away

mkdir -p legacy-version
mv nuget/*.nupkg legacy-version/
mv nuget/*.snupkg legacy-version/


dotnet pack src/Yozian.DependencyInjectionPlus/Yozian.DependencyInjectionPlus.csproj -p:PackageVersion=$version -o nuget


# recover
git checkout nuget/Yozian.DependencyInjectionPlus.nuspec
