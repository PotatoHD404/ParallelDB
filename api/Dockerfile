#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["/", "api/"]
RUN dotnet restore "api/Api.csproj"
COPY . .
WORKDIR "/src/api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM base AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "Api.dll"]