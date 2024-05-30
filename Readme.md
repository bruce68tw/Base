其他語系：[繁中](Readme-TW.md)、[簡中](Readme-CN.md)

### 1 Introduction

Base Repo is a public program that can be used as a reference when developing various C# projects to save development time. It contains 4 projects, each of which is used in different situations:
 - Base: The lowest level utility, its main functions are: processing of various basic data, file access, and CRUD services. All projects such as Console, API, and Web MVC must refer to the Base project.
 - BaseApi: Need to be referenced when developing API and MVC projects. The main functions are: permission control, HTTP file output, CRUD function, reports, sign-off process, Excel import, transaction records, etc.
 - BaseEther: access Ethereum smart contracts, blockchain and IPFS (distributed file system).
 - BaseWeb: You need to refer to it when developing MVC projects. The content is mainly the custom input fields of SSR (Server Side Rendering).

### 2. Download & Execute
Download the Base Repo file from GitHub and unzip it to the local directory. Other projects can correctly refer to the Base-related projects. The _Base/Services/_Fun.Init function must be called at system startup for initialization to ensure normal operation of the function. It contains the following incoming parameters:
 * isDev: Whether it is development mode, the system will automatically read the current execution mode and pass it in.
 * diBox: ASP.NET Core uses many DI (dependency injection) services. Passing this service provider (service provider) as a parameter can facilitate us to use these DI services in the utility program.
 * dbType: Indicates the type of database used by the system. Current choices include MS SQL, Oracle, and MySQL.
 * authType: Indicates the permission level that the system can handle, which is divided into 4 levels according to the degree of laxity:
    1. There are no permission restrictions, which means everyone can perform all tasks.
    2. Controller level: It only controls whether a complete operation can be executed. When the user enters the operation, he can execute all the buttons or sub-functions in it.
    3. Action level: For example, the system can set the permissions for users to perform CRUD sub-functions such as adding, querying, modifying, deleting, etc. for a certain operation.
    4. Data level: When executing each sub-function within the job, the user can only access data within the permission range. For example, when the user's modification permission for a certain job is set to "Personal", the user cannot modify other people's data. Records owned.

### 3. Author

 - Bruce Chen - *Initial work*

### 4. Copyright statement

This project uses the [MIT License](https://en.wikipedia.org/wiki/MIT_License).