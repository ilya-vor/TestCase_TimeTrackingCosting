# Backend API: .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Собираем проект API напрямую (проектные ссылки тянут Domain/Application/Infrastructure),
# чтобы не зависеть от формата файла решения (.sln/.slnx).
COPY backend/src/ ./src/

RUN dotnet publish src/TimeTracking.Api -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080
ENTRYPOINT ["dotnet", "TimeTracking.Api.dll"]
