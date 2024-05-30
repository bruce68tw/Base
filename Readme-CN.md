其他语系：[繁中](Readme-TW.md)、[英文](Readme.md)

### 1.介绍

Base Repo 的性质是公用程式，在开发各类 c# 专案时可以参照，节省开发的时间，它里面包含4个 project，各自使用在不同的场合:
 - Base：最底层的公用程式，主要的功能是：各种基本资料的处理、档案存取、CRUD服务。所有专案像是Console、API、Web MVC都必须参照Base专案。
 - BaseApi：在开发API、MVC专案时需要参照，主要的功能有：权限控制、HTTP档案输出、CRUD功能、报表、签核流程、Excel汇入、交易记录…等。
 - BaseEther：存取以太坊智能合约、区块链和IPFS（分散式档案系统）。
 - BaseWeb：在开发 MVC 专案时需要参照，内容主要是SSR（Server Side Rendering）的自订输入栏位。

### 2.下载 & 执行
从 GitHub 下载 Base Repo 档案，解压缩到本机目录，其他专案可以正确参照 Base 相关专案即可。在系统启动必须呼叫 _Base/Services/_Fun.Init 函数来进行初始化，以确保功能正常运作，它包含以下传入参数：
 * isDev：是否为开发模式，系统会自动读取目前的执行模式然后传入。
 * diBox：ASP.NET Core使用许多DI（dependency injection依赖注入）服务，将这个服务供应者（service provider）以参数传入，可以方便我们在公用程式里面使用这些DI服务。
 * dbType：表示系统所使用的资料库种类，目前的选择有MS SQL、Oracle、MySQL。
 * authType：表示系统可以处理的权限等级，按照宽松程度依次分成4个等级：
    1. 没有任何权限限制，表示所有人可以执行所有的作业。
    2. Controller等级：只控制能不能执行某个完整的作业，当使用者进入该作业之后，他可以执行里面所有的按钮或是子功能。
    3. Action等级：例如系统可以针对某个作业，设定使用者能否执行其中的新增、查询、修改、删除等CRUD子功能的权限。
    4. 资料等级：使用者在执行作业内的每个子功能时，只能存取权限范围内的资料，例如当使用者在某个作业的修改权限被设定为＂个人＂，则无法修改别人拥有的记录。

### 3.作者

 - Bruce Chen - *Initial work*

### 4.版权说明

本专案使用 [MIT 授权许可](https://zh.wikipedia.org/zh-cn/MIT許可證)。