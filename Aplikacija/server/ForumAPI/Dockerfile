# -------- DEV (hot reload) --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src

# restore cache
COPY *.csproj ./
RUN dotnet restore

# copy everything
COPY . ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

CMD ["dotnet", "watch", "run", "--non-interactive", "--urls", "http://0.0.0.0:8080"]

# -------- BUILD (release publish) --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# -------- RUNTIME (release) --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "ForumAPI.dll"]
