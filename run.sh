#!/bin/bash
set -euo pipefail

PROJECT_NAME="Yozian.DependencyInjectionPlus"

PROJECT_CS_PROJ="src/$PROJECT_NAME/$PROJECT_NAME.csproj"

usage() {
   cat <<'EOF'
Usage: ./run.sh <build|pack|publish> [version]

Commands:
  build                 Compile the library and refresh nuget/lib artifacts.
  pack    <version>     Pack the project into nuget/*.nupkg using the given version.
  publish <version>     Push the specified package to nuget.org.

Notes:
  - pack/publish require a semantic version argument (e.g. 10.0.0-preview).
  - publish expects NUGET_API_KEY to be exported in the environment.
EOF
}

prompt_action() {
   local options=(build pack publish exit)
   PS3="Select an action (1-${#options[@]}): "
   select opt in "${options[@]}"; do
      case "$opt" in
         build|pack|publish)
            echo "$opt"
            return 0
            ;;
         exit)
            echo "No action selected. Exiting."
            exit 0
            ;;
         *)
            echo "Invalid selection. Please choose a number from 1-${#options[@]}."
            ;;
      esac
   done
}

run_build() {
   dotnet publish "$PROJECT_CS_PROJ" \
      --force \
      -c Release \
      -o "nuget/lib/netstandard2.0"

   find nuget/lib/netstandard2.0/ -type f ! -name "Yozian.DependencyInjectionPlus*" -exec rm -f {} +
}

run_pack() {
   local version="${1:-}"
   if [ -z "$version" ]; then
      echo "version should be provided!"
      exit 1
   fi

   local commit
   commit=$(git rev-parse --short HEAD)
   echo "pack with commit: $commit"

   sed -i -e "s/commit=\"*\"/commit=\"$commit\"/g" nuget/Yozian.DependencyInjectionPlus.nuspec

   mkdir -p legacy-version
   shopt -s nullglob
   local -a artifacts=(nuget/*.nupkg nuget/*.snupkg)
   if [ ${#artifacts[@]} -gt 0 ]; then
      mv "${artifacts[@]}" legacy-version/
   fi
   shopt -u nullglob

   dotnet pack "$PROJECT_CS_PROJ" \
      -p:PackageVersion="$version" \
      -o nuget

   git checkout -- nuget/Yozian.DependencyInjectionPlus.nuspec
}

run_publish() {
   local version="${1:-}"
   if [ -z "$version" ]; then
      echo "version should be provided!"
      exit 1
   fi

   local package_path="nuget/Yozian.DependencyInjectionPlus.$version.nupkg"
   if [ ! -f "$package_path" ]; then
      echo "package $package_path not found. Run the pack command first."
      exit 1
   fi

   if [ -z "${NUGET_API_KEY:-}" ]; then
      echo "NUGET_API_KEY environment variable must be set before publishing."
      exit 1
   fi

   dotnet nuget push "$package_path" --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY"

   if command -v nuget >/dev/null 2>&1; then
      nuget push "$package_path" -source https://api.nuget.org/v3/index.json
   else
      echo "nuget CLI not found; skipped secondary nuget push command."
   fi
}

if [ $# -lt 1 ]; then
   echo "No arguments provided."
   action=$(prompt_action)
else
   action="$1"
   shift
fi

version_arg="${1:-}"
if [[ "$action" == "pack" || "$action" == "publish" ]]; then
   if [ -z "$version_arg" ]; then
      read -rp "Enter semantic version (e.g. 10.0.0-preview): " version_arg
   fi
fi

case "$action" in
   build)
      run_build
      ;;
   pack)
      run_pack "$version_arg"
      ;;
   publish)
      run_publish "$version_arg"
      ;;
   *)
      usage
      exit 1
      ;;
esac
