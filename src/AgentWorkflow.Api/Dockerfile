FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/AgentWorkflow.Api/AgentWorkflow.Api.csproj src/AgentWorkflow.Api/
COPY src/AgentWorkflow.Core/AgentWorkflow.Core.csproj src/AgentWorkflow.Core/
RUN dotnet restore src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
COPY src/AgentWorkflow.Api/ src/AgentWorkflow.Api/
COPY src/AgentWorkflow.Core/ src/AgentWorkflow.Core/
RUN dotnet publish src/AgentWorkflow.Api/AgentWorkflow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AgentWorkflow.Api.dll"]
