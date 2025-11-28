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
shopt -s nullglob

mkdir -p legacy-version
mv nuget/*.nupkg legacy-version/

cd nuget


nuget pack -version $1

cd ..


# recover
git checkout nuget/Yozian.DependencyInjectionPlus.nuspec
