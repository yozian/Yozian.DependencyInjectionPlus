#!/bin/bash
set -euo pipefail

PROJECT_NAME="Yozian.DependencyInjectionPlus"
DEFAULT_GIT_REMOTE="github"

PROJECT_CS_PROJ="src/$PROJECT_NAME/$PROJECT_NAME.csproj"
NUSPEC_FILE="nuget/$PROJECT_NAME.nuspec"

usage() {
   cat <<'EOF'
Usage: ./run.sh <build|pack|publish> [version]

Commands:
  build                 Compile the library and refresh nuget/lib artifacts.
  pack    <version>     Pack the project into nuget/*.nupkg using the given version.
   publish <version>     Tag the current commit and push the current branch to GitHub.

Notes:
  - pack/publish require a semantic version argument (e.g. 10.0.0-preview).
   - publish expects a clean git working tree and a Git remote named 'github' or 'origin'.
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

validate_version() {
   local version="$1"
   local version_pattern='^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?(\+[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$'

   if [[ ! "$version" =~ $version_pattern ]]; then
      echo "version '$version' is invalid. Use semantic version format like 10.0.2, 10.0.2-preview.1, or 10.0.2-preview.1+build.5."
      exit 1
   fi
}

run_pack() {
   local version="${1:-}"
   if [ -z "$version" ]; then
      echo "version should be provided!"
      exit 1
   fi

   validate_version "$version"

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

resolve_git_remote() {
   if git remote get-url "$DEFAULT_GIT_REMOTE" >/dev/null 2>&1; then
      printf '%s\n' "$DEFAULT_GIT_REMOTE"
      return 0
   fi

   if git remote get-url origin >/dev/null 2>&1; then
      printf '%s\n' "origin"
      return 0
   fi

   echo "No Git remote named '$DEFAULT_GIT_REMOTE' or 'origin' was found."
   exit 1
}

ensure_clean_worktree() {
   if [ -n "$(git status --short)" ]; then
      echo "Git working tree has uncommitted changes. Commit or stash them before publishing."
      exit 1
   fi
}

run_publish() {
   local version="${1:-}"
   if [ -z "$version" ]; then
      echo "version should be provided!"
      exit 1
   fi

   validate_version "$version"

   local current_branch
   current_branch=$(git branch --show-current)
   if [ -z "$current_branch" ]; then
      echo "publish requires a checked-out branch; detached HEAD is not supported."
      exit 1
   fi

   ensure_clean_worktree

   local remote_name tag_name
   remote_name=$(resolve_git_remote)
   tag_name="v$version"

   if git show-ref --verify --quiet "refs/tags/$tag_name"; then
      echo "tag $tag_name already exists locally."
      exit 1
   fi

   if [ -n "$(git ls-remote --tags "$remote_name" "refs/tags/$tag_name")" ]; then
      echo "tag $tag_name already exists on remote $remote_name."
      exit 1
   fi

   echo "Creating tag $tag_name on branch $current_branch"
   git tag -a "$tag_name" -m "Release $tag_name"

   if git push --atomic "$remote_name" "$current_branch" "$tag_name"; then
      echo "Pushed branch $current_branch and tag $tag_name to $remote_name."
      echo "GitHub Actions publish-nuget workflow should start shortly."
   else
      git tag -d "$tag_name" >/dev/null 2>&1 || true
      echo "Push failed. Removed local tag $tag_name."
      exit 1
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
