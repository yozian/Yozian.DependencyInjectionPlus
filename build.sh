dotnet publish src/Yozian.DependencyInjectionPlus/Yozian.DependencyInjectionPlus.csproj \
    --force \
    -c Release \
    -o "nuget/lib/netstandard2.0"


find nuget/lib/netstandard2.0/ -type f ! -name "Yozian.DependencyInjectionPlus*" -exec rm -f {} \;
