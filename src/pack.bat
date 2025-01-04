set username=username

dotnet pack LibOXR\LibOXR.csproj /p:Configuration=Release -o LibOXR\bin\Release

"C:\Program Files (x86)\nuget\nuget.exe" delete nkast.liboxr 1.0.0 -Source "C:\Users\%username%\.nuget\localPackages" -NonInteractive

"C:\Program Files (x86)\nuget\nuget.exe" add LibOXR\bin\Release\nkast.LibOXR.1.0.0.nupkg -Source "C:\Users\%username%\.nuget\localPackages"


pause