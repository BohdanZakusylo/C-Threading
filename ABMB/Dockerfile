FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ABMB.csproj", "./"]
RUN dotnet restore "ABMB.csproj"

COPY . .
RUN dotnet build "ABMB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABMB.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef

# Ensure the dotnet tools are on the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

COPY Data ./Data

COPY --from=publish /app/publish .
COPY ["ABMB.csproj", "./"] 
ENTRYPOINT ["dotnet", "ABMB.dll"]
