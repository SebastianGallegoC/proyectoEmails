# EmailsP


## How to Download the Project

1. Clone the repository from GitHub:
git clone https://github.com/usuario/proyectoEmails.git

2. Open the project folder in Visual Studio with the absolute path => "C:\Users\<your-PC-USER>\source\repos\proyectoEmails"

3. Verify that the EmailsP.sln solution file is in the root directory.


## Project Compilation

1. Open Visual Studio 2022 (version 17.8 or higher).
- Load the EmailsP.sln solution.
- Ensure that the .NET +8.0 SDK is installed by running the following in the terminal: "dotnet --list-sdks"

2. A line beginning with 8.0 or superior should appear.
- Clean and compile the solution:
- Menu: Compile => Clean Solution
- Then: Compile => Compile Solution

If no compilation errors appear, the solution is ready to run tests.


## Test Envioroment and How to Adjust It

1. Open the VS Community console and run:
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package xunit --version 2.9.3
dotnet add package xunit.runner.visualstudio --version 3.1.5
dotnet add package Moq --version 4.20.72
dotnet add package coverlet.collector --version 6.0.0

2. Verify this lines on the .csproj en el entorno de pruebas "EmailsP.Tests":
- <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" /> 
- <PackageReference Include="xunit" Version="2.9.3" />
- <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" /> 
- <PackageReference Include="Moq" Version="4.20.72" /> 
- <PackageReference Include="coverlet.collector" Version="6.0.0" />

3. Comprobar que aparezca el entorno y se pueda ejecutar con Pruebas >> Ejecutar todas las pruebas, sino:

- Right-click on the solution >>>> Add >>>> Existing Project... and Select EmailsP.Tests.csproj.
- Open the Test Explorer:
- Menu: Tests >>>> Windows >>>> Test Explorer
- Press Refresh.
- Run the tests:
- Menu: Tests >>>> Run All Tests