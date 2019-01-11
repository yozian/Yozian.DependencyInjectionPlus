dotnet publish src/Yozian.DependencyInjectionPlus/Yozian.DependencyInjectionPlus.csproj \
    --force \
    -c Release \
    -o "../../nuget/lib/netstandard2.0"
    
rm nuget/lib/netstandard2.0/Yozian.DependencyInjectionPlus.deps.json