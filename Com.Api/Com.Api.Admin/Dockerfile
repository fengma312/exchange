FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8082

ENV ASPNETCORE_URLS=http://+:8082

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Com.Api/Com.Api.Admin/Com.Api.Admin.csproj", "Com.Api/Com.Api.Admin/"]
RUN dotnet restore "Com.Api/Com.Api.Admin/Com.Api.Admin.csproj"
COPY . .
WORKDIR "/src/Com.Api/Com.Api.Admin"
RUN dotnet build "Com.Api.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Com.Api.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Com.Api.Admin.dll"]
