FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app
EXPOSE 8010

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Com.Service/Com.Service.Account/Com.Service.Account.csproj", "Com.Service/Com.Service.Account/"]
RUN dotnet restore "Com.Service/Com.Service.Account/Com.Service.Account.csproj"
COPY . .
WORKDIR "/src/Com.Service/Com.Service.Account"
RUN dotnet build "Com.Service.Account.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Com.Service.Account.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Com.Service.Account.dll"]
