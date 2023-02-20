FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/CrushTest/CrushTest.csproj", "src/CrushTest/"]
RUN dotnet restore "src/CrushTest/CrushTest.csproj"
COPY . .
WORKDIR "/src/src/CrushTest"
RUN dotnet build "CrushTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrushTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrushTest.dll"]
