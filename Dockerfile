FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MyBlog.csproj", "./"]
RUN dotnet restore "MyBlog.csproj"
COPY . .
RUN dotnet build "MyBlog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyBlog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyBlog.dll"]