FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["src/BitMeterCollector/BitMeterCollector.csproj", "BitMeterCollector/"]
COPY ["src/BitMeterCollector.Shared/BitMeterCollector.Shared.csproj", "BitMeterCollector.Shared/"]

RUN dotnet restore "BitMeterCollector/BitMeterCollector.csproj"

COPY ["src/BitMeterCollector/", "BitMeterCollector/"]
COPY ["src/BitMeterCollector.Shared/", "BitMeterCollector.Shared/"]

WORKDIR "/src/BitMeterCollector"
RUN dotnet build "BitMeterCollector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BitMeterCollector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BitMeterCollector.dll"]
