# EmailsP


## How to Download the Project

1. Clone the repository from GitHub:
git clone https://github.com/usuario/proyectoEmails.git

2. Open the project folder in Visual Studio with the absolute path 
=> "C:\Users\<your-PC-USER>\source\repos\proyectoEmails"

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
- "PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"
- "PackageReference Include="xunit" Version="2.9.3" />"
- "PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />"
- "PackageReference Include="Moq" Version="4.20.72" />"
- "PackageReference Include="coverlet.collector" Version="6.0.0" />"

3. Verify that the environment appears and can be run with Tests >> Run all tests, otherwise:
- Right-click on the solution >>>> Add >>>> Existing Project... and Select EmailsP.Tests.csproj.
- Open the Test Explorer:
- Menu: Tests >>>> Windows >>>> Test Explorer
- Press Refresh.
- Run the tests:
- Menu: Tests >>>> Run All Tests
![Imagen de WhatsApp 2025-10-08 a las 17 37 07_b9c1f8a2](https://github.com/user-attachments/assets/09c7c1fa-f271-444f-bca9-ae97262b6d32)

![Imagen de WhatsApp 2025-10-08 a las 17 09 47_dca93554](https://github.com/user-attachments/assets/4c52fc17-fee4-4953-845d-2c8414bc3859)
![Imagen de WhatsApp 2025-10-08 a las 14 02 58_fa3d5b72](https://github.com/user-attachments/assets/3cbad051-cf5c-44ac-ae6e-94957ef1557d)
![Imagen de WhatsApp 2025-10-08 a las 14 02 58_67c349d8](https://github.com/user-attachments/assets/0083f8f3-bac9-4efd-a287-5a85b0ddc82d)


