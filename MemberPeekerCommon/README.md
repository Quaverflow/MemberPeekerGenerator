# Getting Started

THIS VERSION IS IN BETA AND IS SUBJECT TO FREQUENT CHANGES/IMPROVEMENTS

## Add Attribute to the class to peek

`MemberPeekerCommon.CanPeekAttribute`.

![Attribute](https://user-images.githubusercontent.com/81313844/166397510-1a36aa46-9988-44f9-a6e8-6b1934741592.jpg)

## Add initialization Method

In order for the generator to add the files to peek to the compilation, it needs to generate the required MSBuild code. Add the following code to your project:

![Initialization](https://user-images.githubusercontent.com/81313844/166395888-d13e2bb9-23b9-4741-b372-3d4bcffdfbef.png)


Where:

`[assembly: Xunit.TestFramework("YOUR PROJECT NAME + NAME OF THIS CLASS(.initialize in this case)", "ASSEMBLY NAME")]`

Pass the assemblies of the code you want to peek in the `AddToCompilation(params Assembly[] assemblies` method.

Example of the generated Directory.Build.Props:

![GeneratedMsBuild](https://user-images.githubusercontent.com/81313844/166396569-d5c6ec7e-2e0a-4993-b824-79ba5e79530b.jpg)


## Generate the code

To generate the code for `ExampleClass`, you will need to type:
 `MemberPeeker.ExampleClass_Exposed<ExampleClass>(...Constructor Parameters of ExampleClass)`

**Breakdown:**
1) `MemberPeeker` is the namespace required by this generator to access the generated code.
2) `"CLASS NAME"_Exposed` the `_Exposed` part is required.
3) `<YOUR CLASS>` the type parameter is used by the generator to identify the type

Example:
![TestExample](https://user-images.githubusercontent.com/81313844/166396382-814393da-7bbd-431c-903d-1cb1971d922e.jpg)

## Result

**Original:**

![FileToExpose](https://user-images.githubusercontent.com/81313844/166396685-376169d9-1733-4f10-9d8b-88f592c6c86e.jpg)


**Generated (Indentation to be fixed in future release):**

![ExposedVesion](https://user-images.githubusercontent.com/81313844/166396690-e429d651-49d1-4d91-93e4-dfc2c171dbc3.jpg)

