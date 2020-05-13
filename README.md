# ICSS-Devbridge-Calendar-Server

## Setupping a project locally

1. Find a location for project
2. git clone https://github.com/karolisrutkauskas/ICSS-Devbridge-Calendar-Server
3. Open project in Visual Studio. Make sure your Visual studio has all required tools and features installed such as "ASP.NET and web development", "Data storage and processing", 4.7.2 .NET Framework.
4. Solution 'DevBridgeAPI' -> Restore NuGet packages.
5. Open Package Manager Console, paste and run this command:
```
Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -r
```
6. Solution 'DevBridgeAPI' -> Build
7. Now you need to create your local database and publish *.sql files located at database project. DevBridgeDB project -> Publish. Click 'Edit' as shown in image. If "Data storage and processing" is installed you should have active database server something like "(localdb)\MSSQLLocalDB" - select it from the list. Set Database Name to your own liking and click 'Ok'. Then clisk button "Load values". Than near variable "UseTestData" will show up value "false". If you want to load test data into database tables change to "true", otherwise keep "false". Then click button 'Publish'. You can optionally create a profile for easy click-to-publish script.
![Publishing database](https://i.imgur.com/aYZQ016.png)
8. Link DevBridgeAPI project with your newly created database by modifying Web.Config file. Instead of directly modifying Web.Config (because these settings are shared in this repository with everyone), create your own personalized AppSettings.config in the same directory as Web.config and follow instructions under "Local application settings" section.
```xml
...
<add key="appSettings--connectionStrings--DevBridgeDB" value="Server=<server name>;Database=<database name>;Trusted_Connection=True;"/>
...
```
and modify 'value' attribute by copying your own server and database names - image below shows how to locate them:

![Server with database names](https://i.imgur.com/A8l4TVa.png)

9. Finally, fill in remaining details in AppSettings.config to replace Web.config "dummy" values.
10. Run the project and test the API with ./help and ./api/users (should return an empty list)

## Local application settings

DevBridgeAPI service can be configured by supplying settings at Web.config file. Most of the tags in Web.config are auto generated by pugins, however applicaiton custom logic uses settings appSettings XML tag to define its behaviour.
![appSettings xml tag](https://i.imgur.com/AYEajpE.png))
1. appSettings can be modified in Web.config directly, only if they should affect everyone globally as this file will be in the repository. For local settings, you should create a new file at the same directory with Web.config.
2. Notice "dummy" values in Web.config appSettings. This is an indicator, that values should be provided locally. In the created AppSettings.config file, put in some content which should be something like this:
```xml
<appSettings>
  <add key="appSettings--emailPassword" value="<hidden>"/>
  <add key="appSettings--emailName" value="devbridgeapitest@gmail.com"/>
  <add key="appSettings--connectionStrings--DevBridgeDB" value="Server=(localdb)\MSSQLLocalDB;Database=DevBridgeDB;Trusted_Connection=True;"/>
  <add key="appSettings--emailHost" value="smtp.gmail.com"/>
</appSettings>
```
3. You can use your own email credentials for testing. Just make sure to provide special permissions, such as Gmail's permission for unsafe app access. Modify emailHost if using something other than Gmail.
