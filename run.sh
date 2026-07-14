#!/bin/bash
set -euo pipefail

PROJECT_NAME="Yozian.DependencyInjectionPlus"

PROJECT_CS_PROJ="src/$PROJECT_NAME/$PROJECT_NAME.csproj"
NUSPEC_FILE="nuget/$PROJECT_NAME.nuspec"

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

show_recent_packages() {
   shopt -s nullglob
   local -a packages=(nuget/"$PROJECT_NAME".*.nupkg)
   shopt -u nullglob

   if [ ${#packages[@]} -eq 0 ]; then
      echo "Local nuget/ latest 3 package files: none"
   else
      echo "Local nuget/ latest 3 package files:"
      printf '%s\n' "${packages[@]##*/}" | sort -V | tail -n 3 | sed 's/^/  - /'
   fi

   if ! command -v curl >/dev/null 2>&1; then
      echo "nuget.org latest 5 versions (including preview): unavailable (curl not found)"
      return 0
   fi

   local package_id_lower api_url remote_versions
   package_id_lower=$(printf '%s' "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]')
   api_url="https://api.nuget.org/v3/registration5-semver1/$package_id_lower/index.json"

   remote_versions=$(curl --connect-timeout 5 --max-time 10 -fsSL "$api_url" \
      | tr -d '\n' \
      | grep -Eo '"listed":(true|false)[^}]*"version":"[^"]*"' \
      | sed -E 's/.*"listed":(true|false)[^}]*"version":"([^"]*)"/\2\t\1/' \
      | tail -n 5 || true)

   if [ -z "$remote_versions" ]; then
      echo "nuget.org latest 5 versions (including preview): unavailable"
      return 0
   fi

   echo "nuget.org latest 5 versions (including preview):"
   printf '  %-20s %s\n' "VERSION" "STATUS"
   printf '%s\n' "$remote_versions" | awk -F'\t' '{printf "  %-20s %s\n", $1, ($2 == "true" ? "listed" : "unlisted")}'
}

run_build() {
   dotnet publish "$PROJECT_CS_PROJ" \
      --force \
      -c Release \
      -o "nuget/lib/netstandard2.0"

   find nuget/lib/netstandard2.0/ -type f ! -name "$PROJECT_NAME*" -exec rm -f {} +
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

   sed -i -e "s/commit=\"*\"/commit=\"$commit\"/g" "$NUSPEC_FILE"

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

   git checkout -- "$NUSPEC_FILE"
}

run_publish() {
   local version="${1:-}"
   if [ -z "$version" ]; then
      echo "version should be provided!"
      exit 1
   fi

   local package_path="nuget/$PROJECT_NAME.$version.nupkg"
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

prompted_action=false

if [ $# -lt 1 ]; then
   echo "No arguments provided."
   action=$(prompt_action)
   prompted_action=true
else
   action="$1"
   shift
fi

version_arg="${1:-}"
if [[ "$action" == "pack" || "$action" == "publish" ]]; then
   if [ "$prompted_action" = true ] || [ -z "$version_arg" ]; then
      show_recent_packages
   fi

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
