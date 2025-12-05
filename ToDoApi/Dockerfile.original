# שלב 1: שלב הבנייה (Builder Stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# העתק קובץ פתרון (Solution) וקובצי פרויקט
# יש לשנות את הנתיבים בהתאם למבנה שלך: /ToDoList/ToDoApi.csproj
COPY ToDoList/ToDoApi.sln .
COPY ToDoList/ToDoApi/*.csproj ToDoList/ToDoApi/
RUN dotnet restore "ToDoList/ToDoApi/ToDoApi.csproj"

# העתק את שאר קבצי הפרויקט ובנה
COPY . .
WORKDIR /src/ToDoList/ToDoApi
RUN dotnet publish -c Release -o /app/publish

# ----------------------------------------------------------------------------------

# שלב 2: שלב הריצה הסופי (Final Stage)
# שימוש בתמונת ריצה קטנה ומאובטחת יותר
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# הגדר את פורט ההאזנה (Render משתמש בפורט 80 כברירת מחדל בתוך הקונטיינר)
ENV ASPNETCORE_URLS=http://+:80

# הפעלת היישום
ENTRYPOINT ["dotnet", "ToDoApi.dll"]